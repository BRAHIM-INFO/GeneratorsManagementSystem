using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class SubscriptionsController : Controller
    {
        private readonly ISubscriptionService _service;
        private readonly ISubscriberService _subscriberService;
        private readonly IGeneratorService _generatorService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService; // 🆕
        private readonly IInvoiceService _invoiceService;  // 🆕

        public SubscriptionsController(
            ISubscriptionService service,
            ISubscriberService subscriberService,
            IGeneratorService generatorService,
            IAuditService auditService, // 🆕
            IInvoiceService invoiceService,  // 🆕
            UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _subscriberService = subscriberService;
            _generatorService = generatorService;
            _auditService = auditService; // 🆕
            _invoiceService = invoiceService;  // 🆕
            _userManager = userManager;
        }

        // ══════════════════════════════════════
        //  INDEX
        // ══════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "إدارة الاشتراكات (العقود)";
            var subscriptions = await _service.GetAllAsync();
            var stats = await _service.GetStatsAsync();
            ViewBag.Stats = stats;

            // 📝 تسجيل
            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Subscriptions,
                $"عرض قائمة العقود ({subscriptions.Count} عقد)");


            return View(subscriptions);
        }

        // ══════════════════════════════════════
        //  CREATE - نافذة (Partial)
        // ══════════════════════════════════════
        public async Task<IActionResult> Create(int? subscriberId = null)
        {
            var subscribers = await _subscriberService.GetAllAsync();
            var generators = await _generatorService.GetAllAsync();

            ViewBag.Subscribers = subscribers.Where(s => s.IsActive).ToList();
            ViewBag.Generators = generators
                .Where(g => g.Status == GeneratorStatus.Active)
                .ToList();

            // معلومات الأمبير المتاح لكل مولد
            var availableAmpere = new Dictionary<int, decimal>();
            foreach (var gen in generators)
            {
                availableAmpere[gen.Id] = await _service.GetAvailableAmpereAsync(gen.Id);
            }
            ViewBag.AvailableAmpere = availableAmpere;

            var subscription = new Subscription
            {
                ContractNumber = await _service.GenerateContractNumberAsync(),
                StartDate = DateTime.Today,
                DueDay = 1,
                Ampere = 5,
                PricePerAmpere = 15000,
                Status = SubscriptionStatus.Active,
                SubscriberId = subscriberId ?? 0
            };

            return PartialView("_CreateModal", subscription);
        }

        // ═══════════════════════════════════════
        //  CREATE POST - مُحدَّث
        // ═══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subscription subscription)
        {
            try
            {
                ModelState.Remove(nameof(subscription.ContractNumber));
                ModelState.Remove(nameof(subscription.CreatedBy));
                ModelState.Remove(nameof(subscription.Subscriber));
                ModelState.Remove(nameof(subscription.Generator));
                ModelState.Remove(nameof(subscription.Invoices));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct()
                        .ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);
                var created = await _service.CreateAsync(subscription, user?.FullName ?? "النظام");

                // 📝 تسجيل النشاط
                await _auditService.LogCreateAsync(
                    AuditModule.Subscriptions,
                    "عقد اشتراك",
                    created.Id,
                    created.ContractNumber,
                    new
                    {
                        created.ContractNumber,
                        created.SubscriberId,
                        created.GeneratorId,
                        created.Ampere,
                        created.MonthlyAmount,
                        SubscriptionType = created.SubscriptionType.ToString()
                    });


                // 🆕 توليد أول فاتورة تلقائياً
                Invoice? firstInvoice = null;
                try
                {
                    firstInvoice = await _invoiceService.GenerateFirstInvoiceAsync(
                        created.Id, user?.FullName ?? "النظام");
                }
                catch (Exception invEx)
                {
                    // تسجيل الخطأ لكن لا نوقف العملية
                    Console.WriteLine("خطأ في توليد الفاتورة: " + invEx.Message);
                }
                 

                return Json(new
                {
                    success = true,
                    message = $"تم إنشاء العقد ({created.ContractNumber}) بنجاح" +
                              (firstInvoice != null
                                ? $" وتم توليد الفاتورة {firstInvoice.InvoiceNumber}"
                                : ""),
                    contractNumber = created.ContractNumber,
                    invoiceId = firstInvoice?.Id,
                    invoiceNumber = firstInvoice?.InvoiceNumber,
                    showInvoice = firstInvoice != null,
                    printUrl = firstInvoice != null
                        ? Url.Action("Print", "Invoices", new { id = firstInvoice.Id })
                        : null
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
 
        // ══════════════════════════════════════
        //  EDIT - نافذة
        // ══════════════════════════════════════
        public async Task<IActionResult> Edit(int id)
        {
            var subscription = await _service.GetByIdAsync(id);
            if (subscription == null) return NotFound();

            var subscribers = await _subscriberService.GetAllAsync();
            var generators = await _generatorService.GetAllAsync();

            ViewBag.Subscribers = subscribers;
            ViewBag.Generators = generators;

            return PartialView("_EditModal", subscription);
        }

        // ══════════════════════════════════════
        //  EDIT POST
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subscription subscription)
        {
            try
            {
                ModelState.Remove(nameof(subscription.CreatedBy));
                ModelState.Remove(nameof(subscription.Subscriber));
                ModelState.Remove(nameof(subscription.Generator));
                ModelState.Remove(nameof(subscription.Invoices));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct()
                        .ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var oldSubscription = await _service.GetByIdAsync(subscription.Id);
                var oldValues = oldSubscription == null ? null : new
                {
                    oldSubscription.Ampere,
                    oldSubscription.PricePerAmpere,
                    oldSubscription.MonthlyAmount,
                    Status = oldSubscription.Status.ToString()
                };

                var user = await _userManager.GetUserAsync(User);
                //await _service.UpdateAsync(subscription, user?.FullName ?? "النظام");
                var updated = await _service.UpdateAsync(subscription, user?.FullName ?? "النظام");


                // 📝 تسجيل
                await _auditService.LogUpdateAsync(
                    AuditModule.Subscriptions,
                    "عقد اشتراك",
                    updated.Id,
                    updated.ContractNumber,
                    oldValues,
                    new { updated.Ampere, updated.PricePerAmpere, updated.MonthlyAmount });

                 


                return Json(new { success = true, message = "تم تحديث العقد بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  DELETE
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var subscription = await _service.GetByIdAsync(id);
                if (subscription == null)
                    return Json(new { success = false, message = "غير موجود" });

                var contractNumber = subscription.ContractNumber;
                var subscriberName = subscription.Subscriber?.FullName ?? "";


                var result = await _service.DeleteAsync(id);

                if (result)
                {
                    // 📝 تسجيل
                    await _auditService.LogDeleteAsync(
                        AuditModule.Subscriptions,
                        "عقد اشتراك",
                        id,
                        $"{contractNumber} - {subscriberName}");
                }


                return Json(new
                {
                    success = result,
                    message = result ? "تم حذف العقد بنجاح" : "العقد غير موجود"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  SUSPEND (إيقاف مؤقت)
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id, string reason)
        {
            try
            {
                var subscription = await _service.GetByIdAsync(id);
                if (subscription == null) return Json(new { success = false });


                var user = await _userManager.GetUserAsync(User);
                var result = await _service.SuspendAsync(id, reason ?? "", user?.FullName ?? "النظام");

                if (result)
                {
                    // 📝 تسجيل
                    await _auditService.LogAsync(
                        AuditActionType.ToggleStatus,
                        AuditModule.Subscriptions,
                        $"إيقاف مؤقت للعقد {subscription.ContractNumber}. السبب: {reason}",
                        "عقد اشتراك",
                        id,
                        subscription.ContractNumber);
                }


                return Json(new
                {
                    success = result,
                    message = result ? "تم إيقاف العقد مؤقتاً" : "فشل"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  REACTIVATE
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var subscription = await _service.GetByIdAsync(id);
                if (subscription == null) return Json(new { success = false });



                var user = await _userManager.GetUserAsync(User);
                var result = await _service.ReactivateAsync(id, user?.FullName ?? "النظام");

                if (result)
                {
                    await _auditService.LogAsync(
                        AuditActionType.ToggleStatus,
                        AuditModule.Subscriptions,
                        $"إعادة تفعيل العقد {subscription.ContractNumber}",
                        "عقد اشتراك",
                        id,
                        subscription.ContractNumber);
                }


                return Json(new
                {
                    success = result,
                    message = result ? "تم إعادة تفعيل العقد" : "فشل"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  CANCEL
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            try
            {
                var subscription = await _service.GetByIdAsync(id);
                if (subscription == null) return Json(new { success = false });


                var user = await _userManager.GetUserAsync(User);
                var result = await _service.CancelAsync(id, reason ?? "", user?.FullName ?? "النظام");

                if (result)
                {
                    await _auditService.LogAsync(
                        AuditActionType.Cancel,
                        AuditModule.Subscriptions,
                        $"إلغاء العقد {subscription.ContractNumber}. السبب: {reason}",
                        "عقد اشتراك",
                        id,
                        subscription.ContractNumber);
                }


                return Json(new
                {
                    success = result,
                    message = result ? "تم إلغاء العقد" : "فشل"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  DETAILS
        // ══════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            var subscription = await _service.GetByIdAsync(id);
            if (subscription == null) return NotFound();
            ViewData["PageTitle"] = $"العقد {subscription.ContractNumber}";

            // 📝 تسجيل
            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Subscriptions,
                $"عرض تفاصيل العقد: {subscription.ContractNumber}",
                "عقد اشتراك",
                id,
                subscription.ContractNumber);


            return View(subscription);
        }

        // ══════════════════════════════════════
        //  API: الأمبير المتاح
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAvailableAmpere(int generatorId)
        {
            var available = await _service.GetAvailableAmpereAsync(generatorId);
            var generator = await _generatorService.GetByIdAsync(generatorId);

            return Json(new
            {
                success = true,
                available,
                maxAmpere = generator?.MaxAmpere ?? 0,
                generatorName = generator?.Name ?? "",
                generatorNumber = generator?.GeneratorNumber ?? ""
            });
        }
    }
}