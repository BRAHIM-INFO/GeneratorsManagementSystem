using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class SubscribersController : Controller
    {
        private readonly ISubscriberService _service;
        private readonly IGeographyService _geoService;
        private readonly IGeneratorService _genService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public SubscribersController(
            ISubscriberService service,
            IGeographyService geoService,
            IGeneratorService genService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _service = service;
            _geoService = geoService;
            _genService = genService;
            _auditService = auditService;
            _userManager = userManager;
            _db = db;
        }

        // ══════════════════════════════════════
        //  INDEX
        // ══════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "قائمة المشتركين";
            var subscribers = await _service.GetAllAsync();
            var stats = await _service.GetStatsAsync();
            ViewBag.Stats = stats;

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Subscribers,
                $"عرض قائمة المشتركين ({subscribers.Count} مشترك)");

            return View(subscribers);
        }

        // ══════════════════════════════════════
        //  CREATE - نافذة إضافة
        // ══════════════════════════════════════
        public async Task<IActionResult> Create()
        {
            var subscriber = new Subscriber
            {
                SubscriberNumber = await _service.GenerateNumberAsync(),
                IsActive = true
            };

            // تحميل البيانات للـ Dropdowns
            ViewBag.Governorates = await _geoService.GetAllGovernoratesAsync();
            ViewBag.Generators = await _genService.GetAllAsync();
            ViewBag.DeviceTypes = await _db.DeviceTypes
                .Where(d => d.IsActive)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();
            ViewBag.DiscountReasons = await _db.DiscountReasons
                .Where(d => d.IsActive)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();

            // Default: نسبة العمولة من الإعدادات (يمكن جلبها من SystemSettings)
            ViewBag.DefaultCommissionPercentage = 2.5m;

            // التحقق من صلاحية "الإعفاء الكامل" (المدير فقط)
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
            ViewBag.CanFullExempt = roles.Contains("Admin") || roles.Contains("Administrator");

            return PartialView("_CreateModal", subscriber);
        }

        // ══════════════════════════════════════
        //  CREATE POST
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Subscriber subscriber,
            int? generatorId,
            int? deviceTypeId,
            int deviceCount = 1,
            decimal ampere = 0,
            decimal pricePerAmpere = 0,
            decimal adminCommissionPercentage = 0,
            decimal discountAmount = 0,
            int? discountReasonId = null,
            string? discountNotes = null,
            bool isFullExempt = false,
            string? exemptReason = null,
            int dueDay = 1,
            DateTime? startDate = null)
        {
            try
            {
                ModelState.Remove(nameof(subscriber.SubscriberNumber));
                ModelState.Remove(nameof(subscriber.CreatedBy));
                ModelState.Remove(nameof(subscriber.CreatedAt));
                ModelState.Remove(nameof(subscriber.Subscriptions));
                ModelState.Remove(nameof(subscriber.Payments));
                ModelState.Remove(nameof(subscriber.Invoices));
                ModelState.Remove(nameof(subscriber.Governorate));
                ModelState.Remove(nameof(subscriber.District));
                ModelState.Remove(nameof(subscriber.Neighborhood));
                ModelState.Remove(nameof(subscriber.Alley));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();

                    return Json(new
                    {
                        success = false,
                        message = "يرجى تصحيح الأخطاء",
                        errors
                    });
                }

                var user = await _userManager.GetUserAsync(User);

                // تنظيف الحقول
                if (string.IsNullOrWhiteSpace(subscriber.Email)) subscriber.Email = null;
                if (string.IsNullOrWhiteSpace(subscriber.Phone2)) subscriber.Phone2 = null;
                if (string.IsNullOrWhiteSpace(subscriber.IdNumber)) subscriber.IdNumber = null;

                // إنشاء المشترك
                var created = await _service.CreateAsync(subscriber, user?.FullName ?? "النظام");

                // ═══ إنشاء اشتراك افتراضي إن تم اختيار مولد ═══
                if (generatorId.HasValue && generatorId.Value > 0 && ampere > 0 && pricePerAmpere > 0)
                {
                    // التحقق من صلاحية الإعفاء
                    var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
                    var canFullExempt = roles.Contains("Admin") || roles.Contains("Administrator");

                    if (isFullExempt && !canFullExempt)
                    {
                        isFullExempt = false; // منع الإعفاء لغير المدير
                    }

                    // توليد رقم عقد
                    var year = DateTime.Now.ToString("yy");
                    var prefix = $"CON-{year}-";
                    var lastContract = await _db.Subscriptions
                        .Where(s => s.ContractNumber.StartsWith(prefix))
                        .OrderByDescending(s => s.ContractNumber)
                        .Select(s => s.ContractNumber)
                        .FirstOrDefaultAsync();

                    int nextSeq = 1;
                    if (!string.IsNullOrEmpty(lastContract))
                    {
                        var lastSeq = lastContract.Replace(prefix, "");
                        if (int.TryParse(lastSeq, out int parsed))
                            nextSeq = parsed + 1;
                    }

                    // حساب العمولة
                    var baseAmount = ampere * pricePerAmpere;
                    var commissionAmount = baseAmount * (adminCommissionPercentage / 100m);

                    var subscription = new Subscription
                    {
                        ContractNumber = $"{prefix}{nextSeq:D4}",
                        SubscriberId = created.Id,
                        GeneratorId = generatorId.Value,
                        DeviceTypeId = deviceTypeId,
                        DeviceCount = deviceCount,
                        Ampere = ampere,
                        PricePerAmpere = pricePerAmpere,
                        AdminCommissionPercentage = adminCommissionPercentage,
                        AdminCommissionAmount = commissionAmount,
                        DiscountAmount = discountAmount,
                        DiscountReasonId = discountReasonId,
                        DiscountNotes = discountNotes,
                        IsFullExempt = isFullExempt,
                        ExemptReason = isFullExempt ? exemptReason : null,
                        ExemptDate = isFullExempt ? DateTime.Now : null,
                        ExemptBy = isFullExempt ? user?.FullName : null,
                        DueDay = dueDay,
                        StartDate = startDate ?? DateTime.Today,
                        Status = SubscriptionStatus.Active,
                        CreatedAt = DateTime.Now,
                        CreatedBy = user?.FullName ?? "النظام"
                    };

                    _db.Subscriptions.Add(subscription);
                    await _db.SaveChangesAsync();
                }

                // تسجيل النشاط
                await _auditService.LogCreateAsync(
                    AuditModule.Subscribers,
                    "مشترك",
                    created.Id,
                    created.FullName,
                    new
                    {
                        created.SubscriberNumber,
                        created.FullName,
                        created.Phone,
                        HasSubscription = generatorId.HasValue
                    });

                return Json(new
                {
                    success = true,
                    message = $"تم إضافة المشترك ({created.SubscriberNumber}) بنجاح" +
                              (generatorId.HasValue ? " مع إنشاء اشتراك افتراضي" : ""),
                    subscriberId = created.Id,
                    subscriberNumber = created.SubscriberNumber
                });
            }
            catch (Exception ex)
            {
                var fullMessage = "حدث خطأ: " + ex.Message;
                if (ex.InnerException != null)
                    fullMessage += " | " + ex.InnerException.Message;

                await _auditService.LogAsync(
                    AuditActionType.Create,
                    AuditModule.Subscribers,
                    "فشل إضافة مشترك جديد",
                    isSuccess: false,
                    errorMessage: fullMessage);

                return Json(new { success = false, message = fullMessage });
            }
        }

        // ══════════════════════════════════════
        //  EDIT
        // ══════════════════════════════════════
        public async Task<IActionResult> Edit(int id)
        {
            var subscriber = await _service.GetByIdAsync(id);
            if (subscriber == null) return NotFound();

            ViewBag.Governorates = await _geoService.GetAllGovernoratesAsync();

            // إذا كان له محافظة، حمّل الأقضية
            if (subscriber.GovernorateId.HasValue)
                ViewBag.Districts = await _geoService.GetDistrictsByGovernorateAsync(subscriber.GovernorateId.Value);

            // إذا كان له قضاء، حمّل الأحياء
            if (subscriber.DistrictId.HasValue)
                ViewBag.Neighborhoods = await _geoService.GetNeighborhoodsByDistrictAsync(subscriber.DistrictId.Value);

            // إذا كان له حي، حمّل الأزقة
            if (subscriber.NeighborhoodId.HasValue)
                ViewBag.Alleys = await _geoService.GetAlleysByNeighborhoodAsync(subscriber.NeighborhoodId.Value);

            return PartialView("_EditModal", subscriber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subscriber subscriber)
        {
            try
            {
                ModelState.Remove(nameof(subscriber.CreatedBy));
                ModelState.Remove(nameof(subscriber.Subscriptions));
                ModelState.Remove(nameof(subscriber.Payments));
                ModelState.Remove(nameof(subscriber.Invoices));
                ModelState.Remove(nameof(subscriber.Governorate));
                ModelState.Remove(nameof(subscriber.District));
                ModelState.Remove(nameof(subscriber.Neighborhood));
                ModelState.Remove(nameof(subscriber.Alley));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);
                var updated = await _service.UpdateAsync(subscriber, user?.FullName ?? "النظام");

                await _auditService.LogUpdateAsync(
                    AuditModule.Subscribers,
                    "مشترك",
                    updated.Id,
                    updated.FullName);

                return Json(new { success = true, message = "تم التحديث بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
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
                var subscriber = await _service.GetByIdAsync(id);
                if (subscriber == null)
                    return Json(new { success = false, message = "المشترك غير موجود" });

                var name = subscriber.FullName;
                var number = subscriber.SubscriberNumber;

                var result = await _service.DeleteAsync(id);

                if (result)
                {
                    await _auditService.LogDeleteAsync(
                        AuditModule.Subscribers,
                        "مشترك",
                        id,
                        $"{name} ({number})");
                }

                return Json(new
                {
                    success = result,
                    message = result ? "تم حذف المشترك بنجاح" : "المشترك غير موجود"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  TOGGLE ACTIVE
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var subscriber = await _service.GetByIdAsync(id);
                if (subscriber == null)
                    return Json(new { success = false, message = "غير موجود" });

                var result = await _service.ToggleActiveAsync(id);

                await _auditService.LogAsync(
                    AuditActionType.ToggleStatus,
                    AuditModule.Subscribers,
                    $"{(!subscriber.IsActive ? "تفعيل" : "تعطيل")} المشترك: {subscriber.FullName}",
                    entityType: "مشترك",
                    entityId: id,
                    entityName: subscriber.FullName);

                return Json(new
                {
                    success = result,
                    message = result ? "تم تغيير الحالة بنجاح" : "فشل التغيير"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  DETAILS
        // ══════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            var subscriber = await _service.GetByIdAsync(id);
            if (subscriber == null) return NotFound();

            ViewData["PageTitle"] = $"بطاقة المشترك - {subscriber.FullName}";

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Subscribers,
                $"عرض بطاقة المشترك: {subscriber.FullName}",
                "مشترك", id, subscriber.FullName);

            return View(subscriber);
        }

        // ══════════════════════════════════════
        //  SEARCH API
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var results = await _service.SearchAsync(term ?? "");
            return Json(results.Select(s => new
            {
                s.Id,
                s.SubscriberNumber,
                s.FullName,
                s.Phone,
                Area = s.FullAddress,
                s.IsActive
            }));
        }
    }
}