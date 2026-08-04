using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public interface IGeneratorService
    {
        Task<string> GenerateNumberAsync();
        Task<List<Generator>> GetAllAsync();
        Task<Generator?> GetByIdAsync(int id);
        Task<Generator> CreateAsync(Generator generator, string createdBy);
        Task<Generator> UpdateAsync(Generator generator);
        Task<bool> DeleteAsync(int id);
        Task<bool> ChangeStatusAsync(int id, GeneratorStatus status, string? reason);
        Task UpdateRealtimeDataAsync(int id, RealtimeData data);
        Task<List<GeneratorLog>> GetLogsAsync(int generatorId, int count = 50);
        Task AddLogAsync(GeneratorLog log);
        Task<GeneratorDashboardStats> GetDashboardStatsAsync();
    }

    public class GeneratorService : IGeneratorService
    {
        private readonly ApplicationDbContext _db;

        public GeneratorService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─── Generate Number ───
        public async Task<string> GenerateNumberAsync()
        {
            var count = await _db.Generators.CountAsync();
            var seq = (count + 1).ToString("D2");
            var num = $"GEN-{seq}";

            while (await _db.Generators.AnyAsync(g => g.GeneratorNumber == num))
            {
                var n = int.Parse(seq) + 1;
                seq = n.ToString("D2");
                num = $"GEN-{seq}";
            }
            return num;
        }

        // ─── Get All ───
        public async Task<List<Generator>> GetAllAsync()
        {
            return await _db.Generators
                .Include(g => g.Subscriptions)
                    .ThenInclude(s => s.Subscriber)
                .OrderBy(g => g.GeneratorNumber)
                .ToListAsync();
        }

        // ─── Get By Id ───
        public async Task<Generator?> GetByIdAsync(int id)
        {
            return await _db.Generators
                .Include(g => g.Subscriptions)
                    .ThenInclude(s => s.Subscriber)
                .Include(g => g.Logs.OrderByDescending(l => l.LogTime).Take(20))
                .Include(g => g.FuelRecords.OrderByDescending(f => f.Date).Take(10))
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        // ─── Create ───
        public async Task<Generator> CreateAsync(
            Generator generator, string createdBy)
        {
            generator.GeneratorNumber = await GenerateNumberAsync();
            generator.CreatedAt = DateTime.Now;
            generator.CreatedBy = createdBy;

            _db.Generators.Add(generator);
            await _db.SaveChangesAsync();
            return generator;
        }

        // ─── Update ───
        public async Task<Generator> UpdateAsync(Generator generator)
        {
            generator.UpdatedAt = DateTime.Now;
            _db.Generators.Update(generator);
            await _db.SaveChangesAsync();
            return generator;
        }

        // ─── Delete ───
        public async Task<bool> DeleteAsync(int id)
        {
            var gen = await _db.Generators
                .Include(g => g.Subscriptions)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gen == null) return false;

            // منع الحذف إذا كان هناك اشتراكات نشطة
            if (gen.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active))
                throw new Exception(
                    "لا يمكن حذف المولد لوجود اشتراكات نشطة مرتبطة به. " +
                    "قم بإلغاء الاشتراكات أولاً.");

            _db.Generators.Remove(gen);
            await _db.SaveChangesAsync();
            return true;
        }

        // ─── Change Status ───
        public async Task<bool> ChangeStatusAsync(
            int id, GeneratorStatus status, string? reason)
        {
            var gen = await _db.Generators.FindAsync(id);
            if (gen == null) return false;

            gen.Status = status;
            gen.StopReason = reason;
            gen.UpdatedAt = DateTime.Now;

            // سجل الحدث
            _db.GeneratorLogs.Add(new GeneratorLog
            {
                GeneratorId = id,
                LogTime = DateTime.Now,
                LogType = status == GeneratorStatus.Active
                    ? GeneratorLogType.Normal
                    : GeneratorLogType.Shutdown,
                Notes = reason ?? $"تم تغيير الحالة إلى {status}"
            });

            await _db.SaveChangesAsync();
            return true;
        }

        // ─── Update Realtime Data ───
        public async Task UpdateRealtimeDataAsync(int id, RealtimeData data)
        {
            var gen = await _db.Generators.FindAsync(id);
            if (gen == null) return;

            gen.CurrentLoad = data.CurrentLoad;
            gen.Temperature = data.Temperature;
            gen.OilPressure = data.OilPressure;
            gen.CurrentFuelLevel = data.FuelLevel;
            gen.LastDataUpdate = DateTime.Now;

            // تحديث ساعات التشغيل
            if (data.RunningMinutes > 0)
            {
                gen.TodayRunningHours += data.RunningMinutes / 60m;
                gen.TotalRunningHours += data.RunningMinutes / 60m;
            }

            // تسجيل في السجلات
            _db.GeneratorLogs.Add(new GeneratorLog
            {
                GeneratorId = id,
                LogTime = DateTime.Now,
                LogType = GeneratorLogType.IoT,
                CurrentLoad = data.CurrentLoad,
                FuelLevel = data.FuelLevel,
                Temperature = data.Temperature,
                OilPressure = data.OilPressure,
                Voltage = data.Voltage
            });

            await _db.SaveChangesAsync();
        }

        // ─── Get Logs ───
        public async Task<List<GeneratorLog>> GetLogsAsync(
            int generatorId, int count = 50)
        {
            return await _db.GeneratorLogs
                .Where(l => l.GeneratorId == generatorId)
                .OrderByDescending(l => l.LogTime)
                .Take(count)
                .ToListAsync();
        }

        // ─── Add Log ───
        public async Task AddLogAsync(GeneratorLog log)
        {
            _db.GeneratorLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        // ─── Dashboard Stats ───
        public async Task<GeneratorDashboardStats> GetDashboardStatsAsync()
        {
            var generators = await _db.Generators
                .Include(g => g.Subscriptions)
                .ToListAsync();

            return new GeneratorDashboardStats
            {
                Total = generators.Count,
                Active = generators.Count(g => g.Status == GeneratorStatus.Active),
                Stopped = generators.Count(g => g.Status == GeneratorStatus.Stopped),
                Maintenance = generators.Count(g => g.Status == GeneratorStatus.Maintenance),
                Fault = generators.Count(g => g.Status == GeneratorStatus.Fault),
                TotalSubscribers = generators.Sum(g => g.ActiveSubscribersCount),
                NeedsMaintenance = generators.Count(g => g.NeedsMaintenanceSoon),
                LowFuel = generators.Count(g =>
                    g.FuelLevelPercentage.HasValue && g.FuelLevelPercentage < 20)
            };
        }
    }

    // ─── DTOs ───
    public class RealtimeData
    {
        public decimal? CurrentLoad { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OilPressure { get; set; }
        public decimal? FuelLevel { get; set; }
        public decimal? Voltage { get; set; }
        public decimal RunningMinutes { get; set; } = 0;
    }

    public class GeneratorDashboardStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Stopped { get; set; }
        public int Maintenance { get; set; }
        public int Fault { get; set; }
        public int TotalSubscribers { get; set; }
        public int NeedsMaintenance { get; set; }
        public int LowFuel { get; set; }
    }
}