using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Accounting;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GeneratorsManagementSystem.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly ApplicationDbContext _db;

        public AccountingService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ═══ توليد رقم المصروف: EXP-26-00001 ═══
        public async Task<string> GenerateExpenseNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"EXP-{year}-";

            var lastNumber = await _db.Expenses
                .Where(e => e.ExpenseNumber.StartsWith(prefix))
                .OrderByDescending(e => e.ExpenseNumber)
                .Select(e => e.ExpenseNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            return $"{prefix}{nextSeq:D5}";
        }

        // ═══ كل المصاريف ═══
        public async Task<List<Expense>> GetAllExpensesAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.Expenses
                .Include(e => e.Generator)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= endDate.Value);

            return await query
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<Expense?> GetExpenseByIdAsync(int id)
        {
            return await _db.Expenses
                .Include(e => e.Generator)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Expense> CreateExpenseAsync(Expense expense, string createdBy)
        {
            expense.ExpenseNumber = await GenerateExpenseNumberAsync();
            expense.CreatedAt = DateTime.Now;
            expense.CreatedBy = createdBy;

            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();
            return expense;
        }

        public async Task<Expense> UpdateExpenseAsync(Expense expense, string updatedBy)
        {
            var existing = await _db.Expenses.FindAsync(expense.Id)
                ?? throw new Exception("المصروف غير موجود");

            existing.Category = expense.Category;
            existing.Description = expense.Description;
            existing.Amount = expense.Amount;
            existing.ExpenseDate = expense.ExpenseDate;
            existing.GeneratorId = expense.GeneratorId;
            existing.PaymentMethod = expense.PaymentMethod;
            existing.Reference = expense.Reference;
            existing.Beneficiary = expense.Beneficiary;
            existing.Notes = expense.Notes;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _db.Expenses.FindAsync(id);
            if (expense == null) return false;

            _db.Expenses.Remove(expense);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ الإيرادات ═══
        public async Task<List<Payment>> GetAllRevenuesAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.Payments
                .Include(p => p.Subscriber)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Subscription)
                        .ThenInclude(s => s.Generator)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.PaymentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PaymentDate <= endDate.Value);

            return await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.Payments.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.PaymentDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(p => p.PaymentDate <= endDate.Value);

            return await query.SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetTotalExpensesAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.Expenses.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= endDate.Value);

            return await query.SumAsync(e => e.Amount);
        }

        // ═══ لوحة التحكم المالية ═══
        public async Task<AccountingDashboard> GetDashboardAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var allRevenue = await GetTotalRevenueAsync(startDate, endDate);
            var allExpenses = await GetTotalExpensesAsync(startDate, endDate);

            var todayRev = await _db.Payments
                .Where(p => p.PaymentDate.Date == today).SumAsync(p => p.Amount);
            var todayExp = await _db.Expenses
                .Where(e => e.ExpenseDate.Date == today).SumAsync(e => e.Amount);

            var weekRev = await _db.Payments
                .Where(p => p.PaymentDate.Date >= weekStart).SumAsync(p => p.Amount);
            var weekExp = await _db.Expenses
                .Where(e => e.ExpenseDate.Date >= weekStart).SumAsync(e => e.Amount);

            var monthRev = await _db.Payments
                .Where(p => p.PaymentDate.Date >= monthStart).SumAsync(p => p.Amount);
            var monthExp = await _db.Expenses
                .Where(e => e.ExpenseDate.Date >= monthStart).SumAsync(e => e.Amount);

            var pendingInvoices = await _db.Invoices
                .Include(i => i.Payments)
                .Where(i => i.Status == InvoiceStatus.Unpaid
                         || i.Status == InvoiceStatus.PartiallyPaid
                         || i.Status == InvoiceStatus.Overdue)
                .ToListAsync();

            var pendingAmount = pendingInvoices
                .Sum(i => i.TotalAmount - (i.Payments?.Sum(p => p.Amount) ?? 0));

            var netProfit = allRevenue - allExpenses;
            var profitMargin = allRevenue > 0 ? (netProfit / allRevenue * 100) : 0;

            return new AccountingDashboard
            {
                TotalRevenue = allRevenue,
                TotalExpenses = allExpenses,
                NetProfit = netProfit,
                ProfitMargin = profitMargin,

                TodayRevenue = todayRev,
                TodayExpenses = todayExp,
                WeekRevenue = weekRev,
                WeekExpenses = weekExp,
                MonthRevenue = monthRev,
                MonthExpenses = monthExp,

                RevenueTransactionsCount = await _db.Payments.CountAsync(),
                ExpenseTransactionsCount = await _db.Expenses.CountAsync(),

                PendingReceivables = pendingAmount,
                UnpaidInvoicesCount = pendingInvoices.Count(i => !i.IsOverdue),
                OverdueInvoicesCount = pendingInvoices.Count(i => i.IsOverdue),

                TopExpenseCategories = await GetExpensesByCategoryAsync(startDate, endDate),
                RecentMonths = await GetMonthlyStatsAsync(6)
            };
        }

        // ═══ ربحية المولدات ═══
        public async Task<List<GeneratorProfitability>> GetGeneratorsProfitabilityAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var generators = await _db.Generators
                .Include(g => g.Subscriptions)
                    .ThenInclude(s => s.Invoices)
                        .ThenInclude(i => i.Payments)
                .Include(g => g.Expenses)
                .ToListAsync();

            var result = new List<GeneratorProfitability>();

            foreach (var gen in generators)
            {
                var payments = gen.Subscriptions?
                    .SelectMany(s => s.Invoices ?? new List<Invoice>())
                    .SelectMany(i => i.Payments ?? new List<Payment>())
                    .AsQueryable() ?? Enumerable.Empty<Payment>().AsQueryable();

                if (startDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate >= startDate.Value);
                if (endDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate <= endDate.Value);

                var revenue = payments.Sum(p => p.Amount);

                var expenses = gen.Expenses?.AsEnumerable() ?? Enumerable.Empty<Expense>();
                if (startDate.HasValue)
                    expenses = expenses.Where(e => e.ExpenseDate >= startDate.Value);
                if (endDate.HasValue)
                    expenses = expenses.Where(e => e.ExpenseDate <= endDate.Value);

                var totalExpenses = expenses.Sum(e => e.Amount);
                var profit = revenue - totalExpenses;

                result.Add(new GeneratorProfitability
                {
                    GeneratorId = gen.Id,
                    GeneratorNumber = gen.GeneratorNumber,
                    GeneratorName = gen.Name,
                    ActiveSubscribers = gen.ActiveSubscribersCount,
                    Revenue = revenue,
                    Expenses = totalExpenses,
                    Profit = profit,
                    ProfitMargin = revenue > 0 ? (profit / revenue * 100) : 0
                });
            }

            return result.OrderByDescending(g => g.Profit).ToList();
        }

        // ═══ تقرير الربح والخسارة ═══
        public async Task<ProfitLossReport> GetProfitLossReportAsync(
            DateTime startDate, DateTime endDate)
        {
            var payments = await _db.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Subscription)
                        .ThenInclude(s => s.Generator)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .ToListAsync();

            var expenses = await _db.Expenses
                .Include(e => e.Generator)
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .ToListAsync();

            var totalRevenue = payments.Sum(p => p.Amount);
            var totalExpenses = expenses.Sum(e => e.Amount);
            var netProfit = totalRevenue - totalExpenses;

            var revenueByGen = payments
                .Where(p => p.Invoice?.Subscription?.Generator != null)
                .GroupBy(p => p.Invoice!.Subscription!.Generator!.Name)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var expensesByCat = expenses
                .GroupBy(e => e.CategoryText)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            return new ProfitLossReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue * 100) : 0,
                RevenueByGenerator = revenueByGen,
                ExpensesByCategory = expensesByCat,
                Payments = payments,
                Expenses = expenses
            };
        }

        // ═══ إحصائيات شهرية ═══
        public async Task<List<MonthlyStats>> GetMonthlyStatsAsync(int months = 12)
        {
            var result = new List<MonthlyStats>();
            var arabicCulture = new CultureInfo("ar-SA");

            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.Today.AddMonths(-i);
                var monthStart = new DateTime(date.Year, date.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var revenue = await _db.Payments
                    .Where(p => p.PaymentDate >= monthStart && p.PaymentDate <= monthEnd)
                    .SumAsync(p => p.Amount);

                var expenses = await _db.Expenses
                    .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
                    .SumAsync(e => e.Amount);

                result.Add(new MonthlyStats
                {
                    Year = date.Year,
                    Month = date.Month,
                    MonthName = $"{date.ToString("MMMM", arabicCulture)} {date.Year}",
                    Revenue = revenue,
                    Expenses = expenses
                });
            }

            return result;
        }

        // ═══ المصاريف حسب الفئة ═══
        public async Task<List<CategoryExpenseStats>> GetExpensesByCategoryAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.Expenses.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= endDate.Value);

            var expenses = await query.ToListAsync();
            var totalAmount = expenses.Sum(e => e.Amount);

            return expenses
                .GroupBy(e => e.Category)
                .Select(g =>
                {
                    var sample = g.First();
                    var sum = g.Sum(e => e.Amount);
                    return new CategoryExpenseStats
                    {
                        Category = g.Key,
                        CategoryText = sample.CategoryText,
                        CategoryIcon = sample.CategoryIcon,
                        CategoryColor = sample.CategoryColor,
                        Amount = sum,
                        Count = g.Count(),
                        Percentage = totalAmount > 0 ? (sum / totalAmount * 100) : 0
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
        }
    }
}