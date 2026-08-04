using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GeneratorsManagementSystem.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _http;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditService(
            ApplicationDbContext db,
            IHttpContextAccessor http,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _http = http;
            _userManager = userManager;
        }

        // ═══ التسجيل الرئيسي ═══
        public async Task LogAsync(
            AuditActionType actionType,
            AuditModule module,
            string description,
            string? entityType = null,
            int? entityId = null,
            string? entityName = null,
            object? oldValues = null,
            object? newValues = null,
            bool isSuccess = true,
            string? errorMessage = null)
        {
            try
            {
                var httpContext = _http.HttpContext;
                var user = httpContext?.User;

                string? userId = null;
                string userName = "System";
                string? userFullName = null;

                if (user?.Identity?.IsAuthenticated == true)
                {
                    var appUser = await _userManager.GetUserAsync(user);
                    if (appUser != null)
                    {
                        userId = appUser.Id;
                        userName = appUser.UserName ?? "Unknown";
                        userFullName = appUser.FullName;
                    }
                }

                var log = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    UserFullName = userFullName,
                    ActionType = actionType,
                    Module = module,
                    Description = description,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityName = entityName,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true }) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true }) : null,
                    IpAddress = GetIpAddress(),
                    UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                    Url = httpContext?.Request.Path.Value,
                    HttpMethod = httpContext?.Request.Method,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.Now
                };

                // احسب التغييرات إذا كان تعديل
                if (actionType == AuditActionType.Update && oldValues != null && newValues != null)
                {
                    log.Changes = CalculateChanges(oldValues, newValues);
                }

                _db.AuditLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // لا نريد أن يوقف الخطأ في التسجيل العملية الأصلية
            }
        }

        // ═══ Helpers ═══
        public Task LogCreateAsync(AuditModule module, string entityType, int entityId, string entityName, object? newValues = null)
            => LogAsync(AuditActionType.Create, module,
                $"إنشاء {entityType}: {entityName}",
                entityType, entityId, entityName, null, newValues);

        public Task LogUpdateAsync(AuditModule module, string entityType, int entityId, string entityName, object? oldValues = null, object? newValues = null)
            => LogAsync(AuditActionType.Update, module,
                $"تعديل {entityType}: {entityName}",
                entityType, entityId, entityName, oldValues, newValues);

        public Task LogDeleteAsync(AuditModule module, string entityType, int entityId, string entityName)
            => LogAsync(AuditActionType.Delete, module,
                $"حذف {entityType}: {entityName}",
                entityType, entityId, entityName);

        public Task LogLoginAsync(string userName, bool isSuccess, string? errorMessage = null)
            => LogAsync(isSuccess ? AuditActionType.Login : AuditActionType.LoginFailed,
                AuditModule.System,
                isSuccess ? $"تسجيل دخول ناجح: {userName}" : $"محاولة دخول فاشلة: {userName}",
                isSuccess: isSuccess, errorMessage: errorMessage);

        public Task LogLogoutAsync()
            => LogAsync(AuditActionType.Logout, AuditModule.System, "تسجيل خروج");

        public Task LogPaymentAsync(int paymentId, string subscriberName, decimal amount)
            => LogAsync(AuditActionType.Payment, AuditModule.Payments,
                $"تسجيل دفعة بمبلغ {amount:N0} د.ع للمشترك {subscriberName}",
                "Payment", paymentId, subscriberName);

        // ═══ الاستعلامات ═══
        public async Task<List<AuditLog>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            return await _db.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByUserAsync(string userId, int limit = 100)
        {
            return await _db.AuditLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByModuleAsync(AuditModule module, int limit = 100)
        {
            return await _db.AuditLogs
                .Where(l => l.Module == module)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId)
        {
            return await _db.AuditLogs
                .Where(l => l.EntityType == entityType && l.EntityId == entityId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentAsync(int limit = 20)
        {
            return await _db.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _db.AuditLogs.FindAsync(id);
        }

        // ═══ البحث المتقدم ═══
        public async Task<(List<AuditLog> Logs, int TotalCount)> SearchAsync(AuditFilter filter)
        {
            var query = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(l =>
                    l.Description.ToLower().Contains(term) ||
                    l.UserName.ToLower().Contains(term) ||
                    (l.EntityName != null && l.EntityName.ToLower().Contains(term)) ||
                    (l.UserFullName != null && l.UserFullName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrEmpty(filter.UserId))
                query = query.Where(l => l.UserId == filter.UserId);

            if (filter.Module.HasValue)
                query = query.Where(l => l.Module == filter.Module.Value);

            if (filter.ActionType.HasValue)
                query = query.Where(l => l.ActionType == filter.ActionType.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(l => l.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
            {
                var endDate = filter.EndDate.Value.AddDays(1);
                query = query.Where(l => l.Timestamp < endDate);
            }

            if (filter.IsSuccess.HasValue)
                query = query.Where(l => l.IsSuccess == filter.IsSuccess.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        // ═══ الإحصائيات ═══
        public async Task<AuditStats> GetStatsAsync()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var logs = await _db.AuditLogs.ToListAsync();

            return new AuditStats
            {
                TotalLogs = logs.Count,
                TodayLogs = logs.Count(l => l.Timestamp.Date == today),
                ThisWeekLogs = logs.Count(l => l.Timestamp.Date >= weekStart),
                ThisMonthLogs = logs.Count(l => l.Timestamp.Date >= monthStart),
                SuccessCount = logs.Count(l => l.IsSuccess),
                FailureCount = logs.Count(l => !l.IsSuccess),
                UniqueUsersCount = logs.Where(l => l.UserId != null).Select(l => l.UserId).Distinct().Count(),
                LogsByModule = logs.GroupBy(l => l.ModuleText).ToDictionary(g => g.Key, g => g.Count()),
                LogsByAction = logs.GroupBy(l => l.ActionTypeText).ToDictionary(g => g.Key, g => g.Count())
            };
        }

        // ═══ حذف السجلات القديمة ═══
        public async Task<int> DeleteOldLogsAsync(int daysToKeep = 90)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var oldLogs = await _db.AuditLogs
                .Where(l => l.Timestamp < cutoffDate)
                .ToListAsync();

            _db.AuditLogs.RemoveRange(oldLogs);
            await _db.SaveChangesAsync();
            return oldLogs.Count;
        }

        // ═══ Private Helpers ═══
        private string? GetIpAddress()
        {
            var context = _http.HttpContext;
            if (context == null) return null;

            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
                return forwardedFor.Split(',')[0].Trim();

            return context.Connection.RemoteIpAddress?.ToString();
        }

        private string CalculateChanges(object oldValues, object newValues)
        {
            try
            {
                var oldJson = JsonSerializer.Serialize(oldValues);
                var newJson = JsonSerializer.Serialize(newValues);
                var oldDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(oldJson);
                var newDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(newJson);

                if (oldDict == null || newDict == null) return "";

                var changes = new List<string>();
                foreach (var key in newDict.Keys)
                {
                    if (oldDict.ContainsKey(key))
                    {
                        var oldVal = oldDict[key]?.ToString();
                        var newVal = newDict[key]?.ToString();
                        if (oldVal != newVal)
                        {
                            changes.Add($"{key}: '{oldVal}' → '{newVal}'");
                        }
                    }
                }

                return string.Join(" | ", changes.Take(5));
            }
            catch
            {
                return "";
            }
        }
    }
}