using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Fuel;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class FuelController : Controller
    {
        private readonly IFuelService _service;
        private readonly IGeneratorService _generatorService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FuelController(
            IFuelService service,
            IGeneratorService generatorService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _generatorService = generatorService;
            _auditService = auditService;
            _userManager = userManager;
        }

        // ══════════════════════════════════════
        //  INDEX - لوحة تحكم الوقود
        // ══════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "لوحة الوقود والتشغيل";

            var dashboard = await _service.GetDashboardAsync();
            var generatorStats = await _service.GetGeneratorFuelStatsAsync();

            ViewBag.Dashboard = dashboard;
            ViewBag.GeneratorStats = generatorStats;

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Fuel,
                "عرض لوحة تحكم الوقود");

            return View();
        }

        // ══════════════════════════════════════
        //  ALLOCATIONS - حصص الوقود
        // ══════════════════════════════════════
        public async Task<IActionResult> Allocations(DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["PageTitle"] = "حصص الوقود";
            var allocations = await _service.GetAllAllocationsAsync(startDate, endDate);
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(allocations);
        }

        public async Task<IActionResult> CreateAllocation()
        {
            var allocation = new FuelAllocation
            {
                AllocationNumber = await _service.GenerateAllocationNumberAsync(),
                AllocationDate = DateTime.Today,
                AllocationMonth = DateTime.Today.Month,
                AllocationYear = DateTime.Today.Year,
                FuelKind = FuelKind.Diesel,
                Source = FuelSource.Government
            };
            return PartialView("_CreateAllocationModal", allocation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAllocation(FuelAllocation allocation)
        {
            try
            {
                ModelState.Remove(nameof(allocation.AllocationNumber));
                ModelState.Remove(nameof(allocation.CreatedBy));
                ModelState.Remove(nameof(allocation.Consumptions));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);

                if (string.IsNullOrWhiteSpace(allocation.Supplier)) allocation.Supplier = null;
                if (string.IsNullOrWhiteSpace(allocation.ReferenceNumber)) allocation.ReferenceNumber = null;
                if (string.IsNullOrWhiteSpace(allocation.ReceivedBy)) allocation.ReceivedBy = null;
                if (string.IsNullOrWhiteSpace(allocation.Notes)) allocation.Notes = null;

                var created = await _service.CreateAllocationAsync(allocation, user?.FullName ?? "النظام");

                await _auditService.LogCreateAsync(
                    AuditModule.Fuel,
                    "حصة وقود",
                    created.Id,
                    $"{created.AllocationNumber} - {created.Quantity} لتر {created.FuelKindText}");

                return Json(new
                {
                    success = true,
                    message = $"تم تسجيل الحصة ({created.AllocationNumber}) بنجاح - {created.Quantity:N0} لتر"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> EditAllocation(int id)
        {
            var allocation = await _service.GetAllocationByIdAsync(id);
            if (allocation == null) return NotFound();
            return PartialView("_EditAllocationModal", allocation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAllocation(FuelAllocation allocation)
        {
            try
            {
                ModelState.Remove(nameof(allocation.CreatedBy));
                ModelState.Remove(nameof(allocation.Consumptions));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);
                await _service.UpdateAllocationAsync(allocation, user?.FullName ?? "النظام");

                return Json(new { success = true, message = "تم التحديث بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllocation(int id)
        {
            try
            {
                var allocation = await _service.GetAllocationByIdAsync(id);
                if (allocation == null)
                    return Json(new { success = false, message = "غير موجود" });

                var result = await _service.DeleteAllocationAsync(id);

                if (result)
                {
                    await _auditService.LogDeleteAsync(
                        AuditModule.Fuel,
                        "حصة وقود",
                        id,
                        allocation.AllocationNumber);
                }

                return Json(new { success = result, message = result ? "تم الحذف" : "فشل" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  CONSUMPTIONS - سجل الاستهلاك
        // ══════════════════════════════════════
        public async Task<IActionResult> Consumptions(DateTime? startDate = null, DateTime? endDate = null, int? generatorId = null)
        {
            ViewData["PageTitle"] = "سجل استهلاك الوقود";
            var consumptions = await _service.GetAllConsumptionsAsync(startDate, endDate, generatorId);
            var generators = await _generatorService.GetAllAsync();

            ViewBag.Generators = generators;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.GeneratorId = generatorId;

            return View(consumptions);
        }

        public async Task<IActionResult> CreateConsumption()
        {
            var generators = await _generatorService.GetAllAsync();
            var allocations = await _service.GetAvailableAllocationsAsync(FuelKind.Diesel);

            ViewBag.Generators = generators;
            ViewBag.Allocations = allocations;

            var consumption = new FuelConsumption
            {
                ConsumptionNumber = await _service.GenerateConsumptionNumberAsync(),
                ConsumptionDate = DateTime.Now,
                FuelKind = FuelKind.Diesel,
                Method = ConsumptionMethod.Manual
            };

            return PartialView("_CreateConsumptionModal", consumption);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConsumption(FuelConsumption consumption)
        {
            try
            {
                ModelState.Remove(nameof(consumption.ConsumptionNumber));
                ModelState.Remove(nameof(consumption.CreatedBy));
                ModelState.Remove(nameof(consumption.Generator));
                ModelState.Remove(nameof(consumption.FuelAllocation));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);

                if (consumption.FuelAllocationId == 0) consumption.FuelAllocationId = null;
                if (string.IsNullOrWhiteSpace(consumption.FilledBy)) consumption.FilledBy = null;
                if (string.IsNullOrWhiteSpace(consumption.Notes)) consumption.Notes = null;

                var created = await _service.CreateConsumptionAsync(consumption, user?.FullName ?? "النظام");

                await _auditService.LogCreateAsync(
                    AuditModule.Fuel,
                    "استهلاك وقود",
                    created.Id,
                    $"{created.ConsumptionNumber} - {created.Quantity} لتر");

                return Json(new
                {
                    success = true,
                    message = $"تم تسجيل الاستهلاك ({created.ConsumptionNumber}) - {created.Quantity:N2} لتر"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConsumption(int id)
        {
            try
            {
                var result = await _service.DeleteConsumptionAsync(id);
                return Json(new { success = result, message = result ? "تم الحذف" : "فشل" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  API: الحصص المتاحة حسب نوع الوقود
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAvailableAllocations(FuelKind fuelKind)
        {
            var allocations = await _service.GetAvailableAllocationsAsync(fuelKind);
            return Json(allocations.Select(a => new
            {
                a.Id,
                a.AllocationNumber,
                sourceText = a.SourceText,
                totalQuantity = a.Quantity,
                remaining = a.RemainingQuantity,
                pricePerLiter = a.PricePerLiter,
                display = $"{a.AllocationNumber} ({a.SourceText}) - متبقي: {a.RemainingQuantity:N2} لتر"
            }));
        }
    }
}