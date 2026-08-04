using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Accounting;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class AccountingController : Controller
    {
        private readonly IAccountingService _service;
        private readonly IGeneratorService _generatorService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountingController(
            IAccountingService service,
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
        //  DASHBOARD - لوحة التحكم المالية
        // ══════════════════════════════════════
        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["PageTitle"] = "لوحة المحاسبة المالية";

            var dashboard = await _service.GetDashboardAsync(startDate, endDate);
            var profitability = await _service.GetGeneratorsProfitabilityAsync(startDate, endDate);

            ViewBag.Dashboard = dashboard;
            ViewBag.Profitability = profitability;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Accounting,
                "عرض لوحة المحاسبة المالية");

            return View();
        }

        // ══════════════════════════════════════
        //  EXPENSES - المصاريف
        // ══════════════════════════════════════
        public async Task<IActionResult> Expenses(DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["PageTitle"] = "إدارة المصاريف";
            var expenses = await _service.GetAllExpensesAsync(startDate, endDate);
            var stats = await _service.GetExpensesByCategoryAsync(startDate, endDate);

            ViewBag.Stats = stats;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalAmount = expenses.Sum(e => e.Amount);

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Accounting,
                $"عرض قائمة المصاريف ({expenses.Count} مصروف)");

            return View(expenses);
        }

        // ══════════════════════════════════════
        //  CREATE EXPENSE
        // ══════════════════════════════════════
        public async Task<IActionResult> CreateExpense()
        {
            var generators = await _generatorService.GetAllAsync();
            ViewBag.Generators = generators;

            var expense = new Expense
            {
                ExpenseNumber = await _service.GenerateExpenseNumberAsync(),
                ExpenseDate = DateTime.Today,
                PaymentMethod = PaymentMethod.Cash
            };

            return PartialView("_CreateExpenseModal", expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(Expense expense)
        {
            try
            {
                ModelState.Remove(nameof(expense.ExpenseNumber));
                ModelState.Remove(nameof(expense.CreatedBy));
                ModelState.Remove(nameof(expense.Generator));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);

                if (string.IsNullOrWhiteSpace(expense.Reference)) expense.Reference = null;
                if (string.IsNullOrWhiteSpace(expense.Beneficiary)) expense.Beneficiary = null;
                if (string.IsNullOrWhiteSpace(expense.Notes)) expense.Notes = null;
                if (expense.GeneratorId == 0) expense.GeneratorId = null;

                var created = await _service.CreateExpenseAsync(expense, user?.FullName ?? "النظام");

                await _auditService.LogCreateAsync(
                    AuditModule.Accounting,
                    "مصروف",
                    created.Id,
                    $"{created.CategoryText} - {created.Description}",
                    new { created.ExpenseNumber, created.Amount, created.CategoryText });

                return Json(new
                {
                    success = true,
                    message = $"تم تسجيل المصروف ({created.ExpenseNumber}) بنجاح"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  EDIT EXPENSE
        // ══════════════════════════════════════
        public async Task<IActionResult> EditExpense(int id)
        {
            var expense = await _service.GetExpenseByIdAsync(id);
            if (expense == null) return NotFound();

            var generators = await _generatorService.GetAllAsync();
            ViewBag.Generators = generators;

            return PartialView("_EditExpenseModal", expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExpense(Expense expense)
        {
            try
            {
                ModelState.Remove(nameof(expense.CreatedBy));
                ModelState.Remove(nameof(expense.Generator));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);
                if (expense.GeneratorId == 0) expense.GeneratorId = null;

                await _service.UpdateExpenseAsync(expense, user?.FullName ?? "النظام");

                await _auditService.LogUpdateAsync(
                    AuditModule.Accounting,
                    "مصروف",
                    expense.Id,
                    expense.Description);

                return Json(new { success = true, message = "تم التحديث بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var expense = await _service.GetExpenseByIdAsync(id);
                if (expense == null)
                    return Json(new { success = false, message = "غير موجود" });

                var result = await _service.DeleteExpenseAsync(id);

                if (result)
                {
                    await _auditService.LogDeleteAsync(
                        AuditModule.Accounting,
                        "مصروف",
                        id,
                        $"{expense.ExpenseNumber} - {expense.Description}");
                }

                return Json(new { success = result, message = result ? "تم الحذف" : "فشل" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  REVENUES - الإيرادات
        // ══════════════════════════════════════
        public async Task<IActionResult> Revenues(DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["PageTitle"] = "الإيرادات";
            var revenues = await _service.GetAllRevenuesAsync(startDate, endDate);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalAmount = revenues.Sum(r => r.Amount);

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Accounting,
                $"عرض الإيرادات ({revenues.Count} دفعة)");

            return View(revenues);
        }

        // ══════════════════════════════════════
        //  REPORTS - التقارير
        // ══════════════════════════════════════
        public async Task<IActionResult> ProfitLoss(DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["PageTitle"] = "تقرير الأرباح والخسائر";

            var start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var end = endDate ?? DateTime.Today;

            var report = await _service.GetProfitLossReportAsync(start, end);
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.Accounting,
                $"عرض تقرير الأرباح والخسائر من {start:yyyy/MM/dd} إلى {end:yyyy/MM/dd}");

            return View(report);
        }

        public async Task<IActionResult> Profitability(DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewData["PageTitle"] = "ربحية المولدات";
            var profitability = await _service.GetGeneratorsProfitabilityAsync(startDate, endDate);
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(profitability);
        }

        // ══════════════════════════════════════
        //  API - Chart Data
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> MonthlyChartData(int months = 12)
        {
            var data = await _service.GetMonthlyStatsAsync(months);
            return Json(new
            {
                labels = data.Select(m => m.MonthName).ToArray(),
                revenue = data.Select(m => m.Revenue).ToArray(),
                expenses = data.Select(m => m.Expenses).ToArray(),
                profit = data.Select(m => m.Profit).ToArray()
            });
        }

        [HttpGet]
        public async Task<IActionResult> CategoryChartData(DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = await _service.GetExpensesByCategoryAsync(startDate, endDate);
            return Json(new
            {
                labels = data.Select(c => c.CategoryText).ToArray(),
                amounts = data.Select(c => c.Amount).ToArray(),
                colors = data.Select(c => c.CategoryColor).ToArray()
            });
        }
    }
}