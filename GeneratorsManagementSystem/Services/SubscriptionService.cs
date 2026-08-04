using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _db;

        public SubscriptionService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ═══ توليد رقم العقد: CON-26-0001 ═══
        public async Task<string> GenerateContractNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"CON-{year}-";

            var lastNumber = await _db.Subscriptions
                .Where(s => s.ContractNumber.StartsWith(prefix))
                .OrderByDescending(s => s.ContractNumber)
                .Select(s => s.ContractNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            var newNumber = $"{prefix}{nextSeq:D4}";

            while (await _db.Subscriptions.AnyAsync(s => s.ContractNumber == newNumber))
            {
                nextSeq++;
                newNumber = $"{prefix}{nextSeq:D4}";
            }

            return newNumber;
        }

        // ═══ الكل ═══
        public async Task<List<Subscription>> GetAllAsync()
        {
            return await _db.Subscriptions
                .Include(s => s.Subscriber)
                .Include(s => s.Generator)
                .Include(s => s.Invoices)
                    .ThenInclude(i => i.Payments)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // ═══ حسب المشترك ═══
        public async Task<List<Subscription>> GetBySubscriberIdAsync(int subscriberId)
        {
            return await _db.Subscriptions
                .Include(s => s.Generator)
                .Include(s => s.Invoices)
                    .ThenInclude(i => i.Payments)
                .Where(s => s.SubscriberId == subscriberId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // ═══ حسب المولد ═══
        public async Task<List<Subscription>> GetByGeneratorIdAsync(int generatorId)
        {
            return await _db.Subscriptions
                .Include(s => s.Subscriber)
                .Include(s => s.Invoices)
                    .ThenInclude(i => i.Payments)
                .Where(s => s.GeneratorId == generatorId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // ═══ بالمعرّف ═══
        public async Task<Subscription?> GetByIdAsync(int id)
        {
            return await _db.Subscriptions
                .Include(s => s.Subscriber)
                .Include(s => s.Generator)
                .Include(s => s.Invoices)
                    .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // ═══ إنشاء ═══
        public async Task<Subscription> CreateAsync(Subscription subscription, string createdBy)
        {
            // تحقق من وجود المشترك والمولد
            var subscriber = await _db.Subscribers.FindAsync(subscription.SubscriberId)
                ?? throw new Exception("المشترك غير موجود");

            var generator = await _db.Generators.FindAsync(subscription.GeneratorId)
                ?? throw new Exception("المولد غير موجود");

            // تحقق من الأمبير المتاح
            var availableAmpere = await GetAvailableAmpereAsync(subscription.GeneratorId);
            if (subscription.Ampere > availableAmpere)
                throw new Exception(
                    $"الأمبير المطلوب ({subscription.Ampere}) يتجاوز " +
                    $"الأمبير المتاح في المولد ({availableAmpere}). " +
                    $"الأمبير المتبقي: {availableAmpere} A");

            subscription.ContractNumber = await GenerateContractNumberAsync();
            subscription.CreatedAt = DateTime.Now;
            subscription.CreatedBy = createdBy;

            // حساب تاريخ الفوترة القادمة
            subscription.NextBillingDate = CalculateNextBillingDate(
                subscription.StartDate, subscription.SubscriptionType, subscription.DueDay);

            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync();

            return subscription;
        }

        // ═══ تحديث ═══
        public async Task<Subscription> UpdateAsync(Subscription subscription, string updatedBy)
        {
            var existing = await _db.Subscriptions.FindAsync(subscription.Id)
                ?? throw new Exception("العقد غير موجود");

            // إذا تغيّر الأمبير، تحقق من التوفر
            if (existing.Ampere != subscription.Ampere)
            {
                var currentAmpere = existing.Ampere;
                var availableAmpere = await GetAvailableAmpereAsync(subscription.GeneratorId);
                var totalAvailable = availableAmpere + currentAmpere;

                if (subscription.Ampere > totalAvailable)
                    throw new Exception(
                        $"الأمبير المطلوب ({subscription.Ampere}) يتجاوز " +
                        $"الأمبير المتاح في المولد ({totalAvailable})");
            }

            existing.SubscriptionType = subscription.SubscriptionType;
            existing.Ampere = subscription.Ampere;
            existing.PricePerAmpere = subscription.PricePerAmpere;
            existing.FixedFee = subscription.FixedFee;
            existing.InstallationFee = subscription.InstallationFee;
            existing.MonthlyDiscount = subscription.MonthlyDiscount;
            existing.DueDay = subscription.DueDay;
            existing.CabinetNumber = subscription.CabinetNumber;
            existing.CircuitNumber = subscription.CircuitNumber;
            existing.MeterNumber = subscription.MeterNumber;
            existing.StartDate = subscription.StartDate;
            existing.EndDate = subscription.EndDate;
            existing.Notes = subscription.Notes;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return existing;
        }

        // ═══ حذف ═══
        public async Task<bool> DeleteAsync(int id)
        {
            var subscription = await _db.Subscriptions
                .Include(s => s.Invoices)
                    .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null) return false;

            // منع الحذف إذا كان هناك مدفوعات
            if (subscription.Invoices.SelectMany(i => i.Payments).Any())
                throw new Exception(
                    "لا يمكن حذف عقد به فواتير مدفوعة. " +
                    "يمكنك إلغاء العقد بدلاً من حذفه.");

            _db.Subscriptions.Remove(subscription);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ إيقاف مؤقت ═══
        public async Task<bool> SuspendAsync(int id, string reason, string updatedBy)
        {
            var subscription = await _db.Subscriptions.FindAsync(id);
            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Suspended;
            subscription.SuspensionReason = reason;
            subscription.SuspensionDate = DateTime.Now;
            subscription.UpdatedAt = DateTime.Now;
            subscription.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ إعادة تفعيل ═══
        public async Task<bool> ReactivateAsync(int id, string updatedBy)
        {
            var subscription = await _db.Subscriptions.FindAsync(id);
            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Active;
            subscription.SuspensionReason = null;
            subscription.SuspensionDate = null;
            subscription.UpdatedAt = DateTime.Now;
            subscription.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ إلغاء ═══
        public async Task<bool> CancelAsync(int id, string reason, string updatedBy)
        {
            var subscription = await _db.Subscriptions.FindAsync(id);
            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.SuspensionReason = reason;
            subscription.EndDate = DateTime.Today;
            subscription.UpdatedAt = DateTime.Now;
            subscription.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ البحث ═══
        public async Task<List<Subscription>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await GetAllAsync();

            term = term.Trim().ToLower();

            return await _db.Subscriptions
                .Include(s => s.Subscriber)
                .Include(s => s.Generator)
                .Where(s =>
                    s.ContractNumber.ToLower().Contains(term) ||
                    s.Subscriber.FullName.ToLower().Contains(term) ||
                    s.Subscriber.SubscriberNumber.ToLower().Contains(term) ||
                    s.Generator.GeneratorNumber.ToLower().Contains(term) ||
                    (s.CabinetNumber != null && s.CabinetNumber.Contains(term)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // ═══ الإحصائيات ═══
        public async Task<SubscriptionStats> GetStatsAsync()
        {
            var subs = await _db.Subscriptions.ToListAsync();

            return new SubscriptionStats
            {
                Total = subs.Count,
                Active = subs.Count(s => s.Status == SubscriptionStatus.Active),
                Suspended = subs.Count(s => s.Status == SubscriptionStatus.Suspended),
                Expired = subs.Count(s => s.Status == SubscriptionStatus.Expired),
                Cancelled = subs.Count(s => s.Status == SubscriptionStatus.Cancelled),
                TotalMonthlyRevenue = subs
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .Sum(s => s.MonthlyAmount),
                TotalAmpereUsed = subs
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .Sum(s => s.Ampere)
            };
        }

        // ═══ التحقق من إمكانية الإنشاء ═══
        public async Task<bool> CanCreateAsync(int subscriberId, int generatorId)
        {
            // يمكن للمشترك أن يستغل عدة مولدات، لكن لا يمكن اشتراك مكرر لنفس المولد
            return !await _db.Subscriptions.AnyAsync(s =>
                s.SubscriberId == subscriberId &&
                s.GeneratorId == generatorId &&
                s.Status == SubscriptionStatus.Active);
        }

        // ═══ الأمبير المتاح في مولد ═══
        public async Task<decimal> GetAvailableAmpereAsync(int generatorId)
        {
            var generator = await _db.Generators.FindAsync(generatorId);
            if (generator == null) return 0;

            var usedAmpere = await _db.Subscriptions
                .Where(s => s.GeneratorId == generatorId
                         && s.Status == SubscriptionStatus.Active)
                .SumAsync(s => s.Ampere);

            return (generator.MaxAmpere ?? 0) - usedAmpere;
        }

        // ═══ حساب تاريخ الفوترة القادمة ═══
        private DateTime CalculateNextBillingDate(
            DateTime startDate, SubscriptionType type, int dueDay)
        {
            var monthsToAdd = type switch
            {
                SubscriptionType.Monthly => 1,
                SubscriptionType.Quarterly => 3,
                SubscriptionType.SemiAnnual => 6,
                SubscriptionType.Annual => 12,
                _ => 1
            };

            var nextDate = startDate.AddMonths(monthsToAdd);
            var day = Math.Min(dueDay, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
            return new DateTime(nextDate.Year, nextDate.Month, day);
        }
    }
}