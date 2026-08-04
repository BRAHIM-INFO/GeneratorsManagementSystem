using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class SubscriberService : ISubscriberService
    {
        private readonly ApplicationDbContext _db;

        public SubscriberService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ═══ توليد الرقم: SUB-26-0001 ═══
        public async Task<string> GenerateNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"SUB-{year}-";

            var lastNumber = await _db.Subscribers
                .Where(s => s.SubscriberNumber.StartsWith(prefix))
                .OrderByDescending(s => s.SubscriberNumber)
                .Select(s => s.SubscriberNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            var newNumber = $"{prefix}{nextSeq:D4}";

            while (await _db.Subscribers.AnyAsync(s => s.SubscriberNumber == newNumber))
            {
                nextSeq++;
                newNumber = $"{prefix}{nextSeq:D4}";
            }

            return newNumber;
        }

        // ═══ الكل ═══
        public async Task<List<Subscriber>> GetAllAsync()
        {
            return await _db.Subscribers
                .Include(s => s.Governorate)
                .Include(s => s.District)
                .Include(s => s.Neighborhood)
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.Generator)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // ═══ بالمعرّف ═══
        public async Task<Subscriber?> GetByIdAsync(int id)
        {
            return await _db.Subscribers
                .Include(s => s.Governorate)
                .Include(s => s.District)
                .Include(s => s.Neighborhood)
                .Include(s => s.Alley)
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.Generator)
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.DeviceType)
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.DiscountReason)
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.Invoices)
                        .ThenInclude(i => i.Payments)
                .Include(s => s.Payments)
                    .ThenInclude(p => p.Invoice)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // ═══ إنشاء ═══
        public async Task<Subscriber> CreateAsync(Subscriber subscriber, string createdBy)
        {
            subscriber.SubscriberNumber = await GenerateNumberAsync();
            subscriber.CreatedAt = DateTime.Now;
            subscriber.CreatedBy = createdBy;
            subscriber.IsActive = true;

            _db.Subscribers.Add(subscriber);
            await _db.SaveChangesAsync();
            return subscriber;
        }

        // ═══ تحديث ═══
        public async Task<Subscriber> UpdateAsync(Subscriber subscriber, string updatedBy)
        {
            var existing = await _db.Subscribers.FindAsync(subscriber.Id)
                ?? throw new Exception("المشترك غير موجود");

            existing.FullName = subscriber.FullName;
            existing.IdNumber = subscriber.IdNumber;
            existing.Phone = subscriber.Phone;
            existing.Phone2 = subscriber.Phone2;
            existing.Email = subscriber.Email;
            existing.Area = subscriber.Area;
            existing.Street = subscriber.Street;
            existing.BuildingNumber = subscriber.BuildingNumber;
            existing.Floor = subscriber.Floor;
            existing.ApartmentNumber = subscriber.ApartmentNumber;
            existing.AddressNotes = subscriber.AddressNotes;
            existing.Notes = subscriber.Notes;
            existing.IsActive = subscriber.IsActive;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return existing;
        }

        // ═══ حذف ═══
        public async Task<bool> DeleteAsync(int id)
        {
            var subscriber = await _db.Subscribers
                .Include(s => s.Subscriptions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscriber == null) return false;

            if (subscriber.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active))
                throw new Exception("لا يمكن حذف مشترك لديه اشتراكات نشطة");

            _db.Subscribers.Remove(subscriber);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ تفعيل/تعطيل ═══
        public async Task<bool> ToggleActiveAsync(int id)
        {
            var subscriber = await _db.Subscribers.FindAsync(id);
            if (subscriber == null) return false;

            subscriber.IsActive = !subscriber.IsActive;
            subscriber.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ بحث ═══
        public async Task<List<Subscriber>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.Trim().ToLower();

            return await _db.Subscribers
                .Include(s => s.Subscriptions)
                    .ThenInclude(sub => sub.Generator)
                .Where(s =>
                    s.FullName.ToLower().Contains(searchTerm) ||
                    s.SubscriberNumber.ToLower().Contains(searchTerm) ||
                    (s.Phone != null && s.Phone.Contains(searchTerm)) ||
                    (s.IdNumber != null && s.IdNumber.Contains(searchTerm)) ||
                    (s.Area != null && s.Area.ToLower().Contains(searchTerm)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // ═══ إحصائيات ═══
        public async Task<SubscriberStats> GetStatsAsync()
        {
            var subscribers = await _db.Subscribers
                .Include(s => s.Subscriptions)
                .ToListAsync();

            return new SubscriberStats
            {
                Total = subscribers.Count,
                Active = subscribers.Count(s => s.IsActive),
                Inactive = subscribers.Count(s => !s.IsActive),
                WithSubscriptions = subscribers.Count(s => s.Subscriptions.Any()),
                WithoutSubscriptions = subscribers.Count(s => !s.Subscriptions.Any())
            };
        }
    }
}