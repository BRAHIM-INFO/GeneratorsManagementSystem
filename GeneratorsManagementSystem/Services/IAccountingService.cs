using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Accounting;

namespace GeneratorsManagementSystem.Services
{
    public interface IAccountingService
    {
        // ═══ المصاريف ═══
        Task<string> GenerateExpenseNumberAsync();
        Task<List<Expense>> GetAllExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Expense?> GetExpenseByIdAsync(int id);
        Task<Expense> CreateExpenseAsync(Expense expense, string createdBy);
        Task<Expense> UpdateExpenseAsync(Expense expense, string updatedBy);
        Task<bool> DeleteExpenseAsync(int id);

        // ═══ الإيرادات (من المدفوعات) ═══
        Task<List<Payment>> GetAllRevenuesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);

        // ═══ لوحة التحكم المالية ═══
        Task<AccountingDashboard> GetDashboardAsync(DateTime? startDate = null, DateTime? endDate = null);

        // ═══ ربحية المولدات ═══
        Task<List<GeneratorProfitability>> GetGeneratorsProfitabilityAsync(DateTime? startDate = null, DateTime? endDate = null);

        // ═══ التقارير ═══
        Task<ProfitLossReport> GetProfitLossReportAsync(DateTime startDate, DateTime endDate);
        Task<List<MonthlyStats>> GetMonthlyStatsAsync(int months = 12);
        Task<List<CategoryExpenseStats>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    // ═══ Models ═══

    public class AccountingDashboard
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        public decimal TodayRevenue { get; set; }
        public decimal TodayExpenses { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal WeekExpenses { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal MonthExpenses { get; set; }

        public int RevenueTransactionsCount { get; set; }
        public int ExpenseTransactionsCount { get; set; }

        public decimal PendingReceivables { get; set; } // المستحقات
        public int UnpaidInvoicesCount { get; set; }
        public int OverdueInvoicesCount { get; set; }

        public List<CategoryExpenseStats> TopExpenseCategories { get; set; } = new();
        public List<MonthlyStats> RecentMonths { get; set; } = new();
    }

    public class GeneratorProfitability
    {
        public int GeneratorId { get; set; }
        public string GeneratorNumber { get; set; } = string.Empty;
        public string GeneratorName { get; set; } = string.Empty;
        public int ActiveSubscribers { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class ProfitLossReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public Dictionary<string, decimal> RevenueByGenerator { get; set; } = new();
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
    }

    public class MonthlyStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit => Revenue - Expenses;
    }

    public class CategoryExpenseStats
    {
        public ExpenseCategory Category { get; set; }
        public string CategoryText { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}