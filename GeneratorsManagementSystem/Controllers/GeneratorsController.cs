using GeneratorsManagementSystem.Hubs;
using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class GeneratorsController : Controller
    {
        private readonly IGeneratorService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;  // 🆕
        private readonly IHubContext<GeneratorsHub> _hub;

        public GeneratorsController(
            IGeneratorService service,
        IAuditService auditService,  // 🆕
            UserManager<ApplicationUser> userManager,
            IHubContext<GeneratorsHub> hub)
        {
            _service = service;
            _userManager = userManager;
            _auditService = auditService;  // 🆕
            _hub = hub;
        }

        // ══════════════════════════════════════
        // INDEX
        // ══════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "إدارة المولدات";
            var generators = await _service.GetAllAsync();
            var stats = await _service.GetDashboardStatsAsync();
            ViewBag.Stats = stats;

            // 📝 تسجيل
            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Generators,
                $"عرض قائمة المولدات ({generators.Count} مولد)");

            return View(generators);
        }

        // ══════════════════════════════════════
        // DETAILS + REALTIME
        // ══════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            var gen = await _service.GetByIdAsync(id);
            if (gen == null)
            {
                TempData["Error"] = "المولد غير موجود";
                return RedirectToAction(nameof(Index));
            }


            ViewData["PageTitle"] = $"مراقبة المولد - {gen.Name}";


            // 📝 تسجيل العرض
            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Generators,
                $"عرض تفاصيل المولد: {gen.Name}",
                "مولد",
                id,
                gen.Name);


            return View(gen);
        }

        // ══════════════════════════════════════
        // CREATE GET
        // ══════════════════════════════════════
        public async Task<IActionResult> Create()
        {
            ViewData["PageTitle"] = "إضافة مولد جديد";
            ViewBag.NewNumber = await _service.GenerateNumberAsync();
            return View(new Generator
            {
                FuelType = FuelType.Diesel,
                Status = GeneratorStatus.Active,
                Voltage = 380,
                Frequency = 50,
                StartDate = DateTime.Today,
                MaintenanceIntervalHours = 250
            });
        }

        // ══════════════════════════════════════
        // CREATE POST
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Generator model)
        {
            try
            {
                // 🔴 إزالة جميع الحقول الاختيارية من الـ Validation
                var fieldsToRemove = new[] {
                "GeneratorNumber", "CreatedBy", "UpdatedBy", "StopReason",
                "Notes", "Brand", "Model", "SerialNumber", "Area", "Location",
                "Subscribers", "Logs", "FuelRecords", "CreatedAt", "UpdatedAt"
            };

                foreach (var field in fieldsToRemove)
                    ModelState.Remove(field);

                // 🔴 التحقق اليدوي من الاسم فقط
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("Name", "اسم المولد مطلوب");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => $"• {x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                        .ToList();

                    ViewData["PageTitle"] = "إضافة مولد جديد";
                    ViewBag.NewNumber = await _service.GenerateNumberAsync();
                    ViewBag.ValidationErrors = errors;
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                var gen = await _service.CreateAsync(model, user?.FullName ?? "النظام");

                // 📝 تسجيل النشاط
                await _auditService.LogCreateAsync(
                    AuditModule.Generators,
                    "مولد",
                    gen.Id,
                    $"{gen.Name} ({gen.GeneratorNumber})",
                    new
                    {
                        gen.GeneratorNumber,
                        gen.Name,
                        gen.Brand,
                        gen.PowerKVA,
                        gen.MaxAmpere,
                        gen.Area,
                        Status = gen.Status.ToString()
                    });

                try
                {
                    await _hub.Clients.Group("dashboard")
                        .SendAsync("GeneratorAdded", new
                        {
                            gen.Id,
                            gen.GeneratorNumber,
                            gen.Name,
                            gen.Area,
                            Status = gen.Status.ToString()
                        });
                }
                catch { /* تجاهل خطأ SignalR */ }

                TempData["Success"] = $"تم إضافة المولد ({gen.GeneratorNumber}) بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ: " + ex.Message +
                    (ex.InnerException != null ? "<br/>Inner: " + ex.InnerException.Message : "");
                ViewData["PageTitle"] = "إضافة مولد جديد";
                ViewBag.NewNumber = await _service.GenerateNumberAsync();
                return View(model);
            }
        }

        // ══════════════════════════════════════
        // EDIT GET
        // ══════════════════════════════════════
        public async Task<IActionResult> Edit(int id)
        {
            var gen = await _service.GetByIdAsync(id);
            if (gen == null)
            {
                TempData["Error"] = "المولد غير موجود";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PageTitle"] = $"تعديل - {gen.Name}";
            return View(gen);
        }

        // ══════════════════════════════════════
        // EDIT POST
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Generator model)
        {
            if (id != model.Id) return BadRequest();
            ModelState.Remove("GeneratorNumber");

            if (!ModelState.IsValid)
            {
                ViewData["PageTitle"] = $"تعديل - {model.Name}";
                return View(model);
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                TempData["Error"] = "المولد غير موجود";
                return RedirectToAction(nameof(Index));
            }

            model.GeneratorNumber = existing.GeneratorNumber;
            model.CreatedAt = existing.CreatedAt;
            model.CreatedBy = existing.CreatedBy;

            //await _service.UpdateAsync(model);

            var oldGenerator = await _service.GetByIdAsync(model.Id);
            var oldValues = oldGenerator == null ? null : new
            {
                oldGenerator.Name,
                oldGenerator.Brand,
                oldGenerator.PowerKVA,
                oldGenerator.MaxAmpere,
                oldGenerator.Status
            };


            var user = await _userManager.GetUserAsync(User);
            model.CreatedBy = user?.FullName ?? "النظام";
            var updated = await _service.UpdateAsync(model);



            // 📝 تسجيل النشاط
            await _auditService.LogUpdateAsync(
                AuditModule.Generators,
                "مولد",
                updated.Id,
                $"{updated.Name} ({updated.GeneratorNumber})",
                oldValues,
                new { updated.Name, updated.Brand, updated.PowerKVA, updated.MaxAmpere, updated.Status });


            TempData["Success"] = "تم تحديث بيانات المولد بنجاح";
            return RedirectToAction(nameof(Details), new { id });
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
                // جلب المولد أولاً لمعرفة عدد الاشتراكات
                var generator = await _service.GetByIdAsync(id);
                if (generator == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "المولد غير موجود"
                    });
                }

                var activeSubscriptions = generator.Subscriptions?
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .ToList() ?? new List<Subscription>();

                var totalSubscriptions = generator.Subscriptions?.Count ?? 0;

                // منع الحذف إذا كان هناك اشتراكات نشطة
                if (activeSubscriptions.Any())
                {
                    return Json(new
                    {
                        success = false,
                        errorType = "hasActiveSubscriptions",
                        message = "لا يمكن حذف المولد",
                        details = new
                        {
                            generatorName = generator.Name,
                            generatorNumber = generator.GeneratorNumber,
                            activeCount = activeSubscriptions.Count,
                            totalCount = totalSubscriptions,
                            subscribers = activeSubscriptions.Select(s => new
                            {
                                contractNumber = s.ContractNumber,
                                subscriberName = s.Subscriber?.FullName ?? "غير معروف",
                                subscriberNumber = s.Subscriber?.SubscriberNumber ?? "",
                                ampere = s.Ampere,
                                monthlyAmount = s.MonthlyAmount,
                                subscriberId = s.SubscriberId,
                                subscriptionId = s.Id
                            }).ToList()
                        }
                    });
                }

                // إذا لم يكن هناك اشتراكات، احذف
                var result = await _service.DeleteAsync(id);

                if (result)
                {
                    // 📝 تسجيل النشاط
                    await _auditService.LogDeleteAsync(
                        AuditModule.Generators,
                        "مولد",
                        id,
                        $"{generator.Name} ({generator.GeneratorNumber})");
                }

                return Json(new
                {
                    success = result,
                    message = result
                        ? $"تم حذف المولد ({generator.GeneratorNumber}) بنجاح"
                        : "فشل الحذف"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var result = await _service.DeleteAsync(id);

        //    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //        return Json(new
        //        {
        //            success = result,
        //            message = result ? "تم حذف المولد" : "فشل الحذف"
        //        });

        //    TempData[result ? "Success" : "Error"] =
        //        result ? "تم حذف المولد بنجاح" : "فشل في الحذف";
        //    return RedirectToAction(nameof(Index));
        //}

        // ══════════════════════════════════════
        // CHANGE STATUS (AJAX)
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(
            int id, GeneratorStatus status, string? reason)
        {
            var generator = await _service.GetByIdAsync(id);
            if (generator == null)
                return Json(new { success = false, message = "غير موجود" });

            var oldStatus = generator.Status;

            var result = await _service.ChangeStatusAsync(id, status, reason);

            if (result)
            {
                // إشعار SignalR
                await _hub.Clients.Group("dashboard")
                    .SendAsync("GeneratorStatusChanged", new
                    {
                        GeneratorId = id,
                        Status = status.ToString(),
                        Reason = reason
                    });

                await _hub.Clients.Group($"generator_{id}")
                    .SendAsync("StatusChanged", new
                    {
                        Status = status.ToString(),
                        Reason = reason
                    });

                // 📝 تسجيل النشاط
                await _auditService.LogAsync(
                    AuditActionType.ToggleStatus,
                    AuditModule.Generators,
                    $"تغيير حالة المولد {generator.Name} من {oldStatus} إلى {status}",
                    "مولد",
                    id,
                    generator.Name,
                    newValues: new { OldStatus = oldStatus.ToString(), NewStatus = status.ToString(), Reason = reason });
            }

            return Json(new
            {
                success = result,
                message = result
                    ? "تم تغيير الحالة بنجاح"
                    : "فشل في تغيير الحالة"
            });
        }

        // ══════════════════════════════════════
        // GET REALTIME DATA (AJAX)
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetRealtimeData(int id)
        {
            var gen = await _service.GetByIdAsync(id);
            if (gen == null) return NotFound();

            return Json(new
            {
                gen.Id,
                gen.Name,
                gen.GeneratorNumber,
                Status = gen.Status.ToString(),
                StatusLabel = GetStatusLabel(gen.Status),
                CurrentLoad = gen.CurrentLoad ?? 0,
                MaxAmpere = gen.MaxAmpere ?? 0,
                LoadPercentage = gen.LoadPercentage,
                Temperature = gen.Temperature ?? 0,
                OilPressure = gen.OilPressure ?? 0,
                FuelLevel = gen.CurrentFuelLevel ?? 0,
                FuelTankCapacity = gen.FuelTankCapacity ?? 0,
                FuelLevelPct = gen.FuelLevelPercentage ?? 0,
                TodayHours = gen.TodayRunningHours,
                TotalHours = gen.TotalRunningHours,
                LastUpdate = gen.LastDataUpdate?.ToString("HH:mm:ss") ?? "—",
                ActiveSubscribers = gen.ActiveSubscribersCount,
                NeedsMaintenance = gen.NeedsMaintenanceSoon
            });
        }

        // ══════════════════════════════════════
        // GET ALL REALTIME (للوحة التحكم)
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAllRealtimeData()
        {
            var generators = await _service.GetAllAsync();
            var data = generators.Select(g => new
            {
                g.Id,
                g.Name,
                g.GeneratorNumber,
                g.Area,
                Status = g.Status.ToString(),
                StatusLabel = GetStatusLabel(g.Status),
                StatusColor = GetStatusColor(g.Status),
                CurrentLoad = g.CurrentLoad ?? 0,
                MaxAmpere = g.MaxAmpere ?? 0,
                LoadPercentage = g.LoadPercentage,
                FuelLevelPct = g.FuelLevelPercentage ?? 0,
                Temperature = g.Temperature ?? 0,
                ActiveSubscribers = g.ActiveSubscribersCount,
                NeedsMaintenance = g.NeedsMaintenanceSoon,
                LastUpdate = g.LastDataUpdate?.ToString("HH:mm:ss") ?? "—"
            });

            return Json(data);
        }

        // ══════════════════════════════════════
        // GET LOGS (AJAX)
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetLogs(int id, int count = 20)
        {
            var logs = await _service.GetLogsAsync(id, count);
            return Json(logs.Select(l => new
            {
                l.Id,
                Time = l.LogTime.ToString("yyyy/MM/dd HH:mm:ss"),
                Type = l.LogType.ToString(),
                TypeLabel = GetLogTypeLabel(l.LogType),
                TypeColor = GetLogTypeColor(l.LogType),
                l.CurrentLoad,
                l.FuelLevel,
                l.Temperature,
                l.OilPressure,
                l.Voltage,
                l.Notes
            }));
        }

        // ══════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════
        private static string GetStatusLabel(GeneratorStatus s) => s switch
        {
            GeneratorStatus.Active => "يعمل",
            GeneratorStatus.Stopped => "متوقف",
            GeneratorStatus.Maintenance => "صيانة",
            GeneratorStatus.Fault => "عطل",
            GeneratorStatus.Standby => "احتياط",
            _ => "غير معروف"
        };

        private static string GetStatusColor(GeneratorStatus s) => s switch
        {
            GeneratorStatus.Active => "#48BB78",
            GeneratorStatus.Stopped => "#718096",
            GeneratorStatus.Maintenance => "#F6AD55",
            GeneratorStatus.Fault => "#FC8181",
            GeneratorStatus.Standby => "#63B3ED",
            _ => "#718096"
        };

        private static string GetLogTypeLabel(GeneratorLogType t) => t switch
        {
            GeneratorLogType.Normal => "تشغيل عادي",
            GeneratorLogType.Warning => "تحذير",
            GeneratorLogType.Fault => "عطل",
            GeneratorLogType.Maintenance => "صيانة",
            GeneratorLogType.Shutdown => "إيقاف",
            GeneratorLogType.IoT => "بيانات مباشرة",
            _ => "غير محدد"
        };

        private static string GetLogTypeColor(GeneratorLogType t) => t switch
        {
            GeneratorLogType.Normal => "#48BB78",
            GeneratorLogType.Warning => "#F6AD55",
            GeneratorLogType.Fault => "#FC8181",
            GeneratorLogType.Maintenance => "#63B3ED",
            GeneratorLogType.Shutdown => "#718096",
            GeneratorLogType.IoT => "#9F7AEA",
            _ => "#718096"
        };
    }
}