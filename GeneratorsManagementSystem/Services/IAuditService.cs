using GeneratorsManagementSystem.Models;

namespace GeneratorsManagementSystem.Services
{
    public interface IAuditService
    {
        // ═══ التسجيل ═══
        Task LogAsync(
            AuditActionType actionType,
            AuditModule module,
            string description,
            string? entityType = null,
            int? entityId = null,
            string? entityName = null,
            object? oldValues = null,
            object? newValues = null,
            bool isSuccess = true,
            string? errorMessage = null);

        Task LogCreateAsync(AuditModule module, string entityType, int entityId, string entityName, object? newValues = null);
        Task LogUpdateAsync(AuditModule module, string entityType, int entityId, string entityName, object? oldValues = null, object? newValues = null);
        Task LogDeleteAsync(AuditModule module, string entityType, int entityId, string entityName);
        Task LogLoginAsync(string userName, bool isSuccess, string? errorMessage = null);
        Task LogLogoutAsync();
        Task LogPaymentAsync(int paymentId, string subscriberName, decimal amount);

        // ═══ الاستعلامات ═══
        Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<List<AuditLog>> GetByUserAsync(string userId, int limit = 100);
        Task<List<AuditLog>> GetByModuleAsync(AuditModule module, int limit = 100);
        Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId);
        Task<List<AuditLog>> GetRecentAsync(int limit = 20);
        Task<AuditLog?> GetByIdAsync(int id);

        // ═══ الفلترة ═══
        Task<(List<AuditLog> Logs, int TotalCount)> SearchAsync(AuditFilter filter);

        // ═══ الإحصائيات ═══
        Task<AuditStats> GetStatsAsync();

        // ═══ الحذف ═══
        Task<int> DeleteOldLogsAsync(int daysToKeep = 90);
    }

    public class AuditFilter
    {
        public string? SearchTerm { get; set; }
        public string? UserId { get; set; }
        public AuditModule? Module { get; set; }
        public AuditActionType? ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsSuccess { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class AuditStats
    {
        public int TotalLogs { get; set; }
        public int TodayLogs { get; set; }
        public int ThisWeekLogs { get; set; }
        public int ThisMonthLogs { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int UniqueUsersCount { get; set; }
        public Dictionary<string, int> LogsByModule { get; set; } = new();
        public Dictionary<string, int> LogsByAction { get; set; } = new();
    }
}