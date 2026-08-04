using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _db;

        public InvoiceService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ═══ توليد رقم: INV-26-00001 ═══
        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"INV-{year}-";

            var lastNumber = await _db.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            var newNumber = $"{prefix}{nextSeq:D5}";

            while (await _db.Invoices.AnyAsync(i => i.InvoiceNumber == newNumber))
            {
                nextSeq++;
                newNumber = $"{prefix}{nextSeq:D5}";
            }

            return newNumber;
        }

        // ═══ الكل ═══
        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.Generator)
                .Include(i => i.Payments)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        // ═══ حسب المشترك ═══
        public async Task<List<Invoice>> GetBySubscriberIdAsync(int subscriberId)
        {
            return await _db.Invoices
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.Generator)
                .Include(i => i.Payments)
                .Where(i => i.SubscriberId == subscriberId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        // ═══ حسب الاشتراك ═══
        public async Task<List<Invoice>> GetBySubscriptionIdAsync(int subscriptionId)
        {
            return await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Payments)
                .Where(i => i.SubscriptionId == subscriptionId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        // ═══ بالمعرّف ═══
        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.Generator)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        // ═══ إنشاء ═══
        public async Task<Invoice> CreateAsync(Invoice invoice, string createdBy)
        {
            invoice.InvoiceNumber = await GenerateInvoiceNumberAsync();
            invoice.CreatedAt = DateTime.Now;
            invoice.CreatedBy = createdBy;

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            return invoice;
        }

        // ═══ تحديث ═══
        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            invoice.UpdatedAt = DateTime.Now;
            _db.Invoices.Update(invoice);
            await _db.SaveChangesAsync();
            return invoice;
        }

        // ═══ حذف ═══
        public async Task<bool> DeleteAsync(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return false;

            if (invoice.Payments.Any())
                throw new Exception("لا يمكن حذف فاتورة لها مدفوعات. قم بإلغائها بدلاً من الحذف.");

            _db.Invoices.Remove(invoice);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ إلغاء ═══
        public async Task<bool> CancelAsync(int id, string reason)
        {
            var invoice = await _db.Invoices.FindAsync(id);
            if (invoice == null) return false;

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.Notes = string.IsNullOrEmpty(invoice.Notes)
                ? $"سبب الإلغاء: {reason}"
                : $"{invoice.Notes}\nسبب الإلغاء: {reason}";
            invoice.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ 🎯 توليد أول فاتورة عند إنشاء العقد ═══
        public async Task<Invoice> GenerateFirstInvoiceAsync(int subscriptionId, string createdBy)
        {
            var subscription = await _db.Subscriptions
                .Include(s => s.Subscriber)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId)
                ?? throw new Exception("العقد غير موجود");

            // حساب فترة الفاتورة
            var periodStart = subscription.StartDate;
            var periodEnd = periodStart.AddMonths(subscription.MonthsInPeriod).AddDays(-1);

            // تاريخ الاستحقاق: يوم الاستحقاق من الشهر التالي للبداية
            var dueDate = CalculateDueDate(periodStart, subscription.DueDay);

            // المبلغ = المبلغ الشهري × عدد الشهور + رسوم التركيب (فقط في أول فاتورة)
            var baseAmount = subscription.MonthlyAmount * subscription.MonthsInPeriod;
            var installationFee = subscription.InstallationFee;

            var invoice = new Invoice
            {
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                SubscriptionId = subscription.Id,
                SubscriberId = subscription.SubscriberId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                IssueDate = DateTime.Today,
                DueDate = dueDate,
                Amount = baseAmount,
                Discount = 0,
                Tax = 0,
                AdditionalCharges = installationFee,
                Status = InvoiceStatus.Unpaid,
                Notes = installationFee > 0
                    ? $"يشمل رسوم التركيب: {installationFee:N0} د.ع"
                    : null,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy
            };

            _db.Invoices.Add(invoice);

            // تحديث تواريخ الفوترة في العقد
            subscription.LastBillingDate = DateTime.Today;
            subscription.NextBillingDate = CalculateNextBillingDate(
                periodStart, subscription.SubscriptionType, subscription.DueDay);

            await _db.SaveChangesAsync();
            return invoice;
        }

        // ═══ توليد الفواتير الشهرية ═══
        public async Task<List<Invoice>> GenerateMonthlyInvoicesAsync(string createdBy)
        {
            var today = DateTime.Today;
            var invoices = new List<Invoice>();

            // الاشتراكات النشطة التي حان موعد فاتورتها
            var subscriptionsDue = await _db.Subscriptions
                .Include(s => s.Subscriber)
                .Where(s => s.Status == SubscriptionStatus.Active
                         && s.NextBillingDate.HasValue
                         && s.NextBillingDate.Value.Date <= today)
                .ToListAsync();

            foreach (var sub in subscriptionsDue)
            {
                var periodStart = sub.LastBillingDate ?? sub.StartDate;
                var periodEnd = periodStart.AddMonths(sub.MonthsInPeriod).AddDays(-1);
                var dueDate = CalculateDueDate(today, sub.DueDay);

                var invoice = new Invoice
                {
                    InvoiceNumber = await GenerateInvoiceNumberAsync(),
                    SubscriptionId = sub.Id,
                    SubscriberId = sub.SubscriberId,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    IssueDate = today,
                    DueDate = dueDate,
                    Amount = sub.MonthlyAmount * sub.MonthsInPeriod,
                    Status = InvoiceStatus.Unpaid,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy
                };

                _db.Invoices.Add(invoice);
                invoices.Add(invoice);

                // تحديث العقد
                sub.LastBillingDate = today;
                sub.NextBillingDate = CalculateNextBillingDate(
                    today, sub.SubscriptionType, sub.DueDay);
            }

            if (invoices.Any())
                await _db.SaveChangesAsync();

            return invoices;
        }

        // ═══ تحديث حالة فاتورة واحدة ═══
        public async Task UpdateInvoiceStatusAsync(int invoiceId)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null || invoice.Status == InvoiceStatus.Cancelled) return;

            var paidAmount = invoice.Payments.Sum(p => p.Amount);
            var totalAmount = invoice.TotalAmount;

            if (paidAmount >= totalAmount)
                invoice.Status = InvoiceStatus.Paid;
            else if (paidAmount > 0)
                invoice.Status = InvoiceStatus.PartiallyPaid;
            else if (invoice.DueDate < DateTime.Today)
                invoice.Status = InvoiceStatus.Overdue;
            else
                invoice.Status = InvoiceStatus.Unpaid;

            invoice.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        // ═══ تحديث كل الفواتير المتأخرة ═══
        public async Task UpdateAllOverdueStatusAsync()
        {
            var today = DateTime.Today;
            var invoices = await _db.Invoices
                .Include(i => i.Payments)
                .Where(i => (i.Status == InvoiceStatus.Unpaid
                          || i.Status == InvoiceStatus.PartiallyPaid)
                         && i.DueDate < today)
                .ToListAsync();

            foreach (var inv in invoices)
            {
                var paid = inv.Payments.Sum(p => p.Amount);
                if (paid < inv.TotalAmount)
                    inv.Status = InvoiceStatus.Overdue;
            }

            if (invoices.Any())
                await _db.SaveChangesAsync();
        }

        // ═══ الإحصائيات ═══
        public async Task<InvoiceStats> GetStatsAsync()
        {
            await UpdateAllOverdueStatusAsync();

            var invoices = await _db.Invoices
                .Include(i => i.Payments)
                .Where(i => i.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            return new InvoiceStats
            {
                Total = invoices.Count,
                Paid = invoices.Count(i => i.Status == InvoiceStatus.Paid),
                Unpaid = invoices.Count(i => i.Status == InvoiceStatus.Unpaid),
                PartiallyPaid = invoices.Count(i => i.Status == InvoiceStatus.PartiallyPaid),
                Overdue = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
                Cancelled = 0,
                TotalInvoiced = invoices.Sum(i => i.TotalAmount),
                TotalPaid = invoices.SelectMany(i => i.Payments).Sum(p => p.Amount),
                TotalOutstanding = invoices
                    .Where(i => i.Status != InvoiceStatus.Paid)
                    .Sum(i => i.RemainingAmount),
                TotalOverdue = invoices
                    .Where(i => i.Status == InvoiceStatus.Overdue)
                    .Sum(i => i.RemainingAmount)
            };
        }

        // ═══ 🎯 التنبيهات ═══
        public async Task<List<InvoiceAlert>> GetAlertsAsync(int daysAhead = 7)
        {
            await UpdateAllOverdueStatusAsync();

            var today = DateTime.Today;
            var futureDate = today.AddDays(daysAhead);

            var invoices = await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Payments)
                .Where(i => (i.Status == InvoiceStatus.Unpaid
                          || i.Status == InvoiceStatus.PartiallyPaid
                          || i.Status == InvoiceStatus.Overdue)
                         && (i.DueDate <= futureDate))
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            return invoices.Select(inv =>
            {
                var daysUntilDue = (inv.DueDate.Date - today).Days;

                var alertType = daysUntilDue < 0 ? "overdue"
                             : daysUntilDue == 0 ? "today"
                             : "upcoming";

                var alertLevel = daysUntilDue < 0 ? "danger"
                              : daysUntilDue <= 2 ? "warning"
                              : "info";

                var alertMessage = daysUntilDue < 0
                    ? $"متأخرة بـ {Math.Abs(daysUntilDue)} يوم"
                    : daysUntilDue == 0 ? "تستحق اليوم"
                    : daysUntilDue == 1 ? "تستحق غداً"
                    : $"بقي {daysUntilDue} أيام على الاستحقاق";

                var alertIcon = daysUntilDue < 0 ? "fa-exclamation-triangle"
                             : daysUntilDue <= 2 ? "fa-bell"
                             : "fa-calendar-alt";

                var badgeClass = daysUntilDue < 0 ? "bg-danger"
                              : daysUntilDue <= 2 ? "bg-warning"
                              : "bg-info";

                return new InvoiceAlert
                {
                    InvoiceId = inv.Id,
                    SubscriberId = inv.SubscriberId,
                    InvoiceNumber = inv.InvoiceNumber,
                    SubscriberName = inv.Subscriber?.FullName ?? "",
                    SubscriberPhone = inv.Subscriber?.Phone ?? "",
                    Amount = inv.TotalAmount,
                    RemainingAmount = inv.RemainingAmount,
                    DueDate = inv.DueDate,
                    DaysUntilDue = daysUntilDue,
                    AlertType = alertType,
                    AlertLevel = alertLevel,
                    AlertMessage = alertMessage,
                    AlertIcon = alertIcon,
                    BadgeClass = badgeClass
                };
            }).ToList();
        }

        // ═══ الفواتير المتأخرة ═══
        public async Task<List<Invoice>> GetOverdueInvoicesAsync()
        {
            await UpdateAllOverdueStatusAsync();

            return await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.Generator)
                .Include(i => i.Payments)
                .Where(i => i.Status == InvoiceStatus.Overdue)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        // ═══ الفواتير القادمة ═══
        public async Task<List<Invoice>> GetUpcomingInvoicesAsync(int days = 7)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(days);

            return await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Payments)
                .Where(i => (i.Status == InvoiceStatus.Unpaid
                          || i.Status == InvoiceStatus.PartiallyPaid)
                         && i.DueDate >= today
                         && i.DueDate <= futureDate)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        // ═══ كل الفواتير غير المسددة ═══
        public async Task<List<Invoice>> GetUnpaidInvoicesAsync()
        {
            return await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.Generator)
                .Include(i => i.Payments)
                .Where(i => i.Status == InvoiceStatus.Unpaid
                         || i.Status == InvoiceStatus.PartiallyPaid
                         || i.Status == InvoiceStatus.Overdue)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        // ═══ البحث ═══
        public async Task<List<Invoice>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await GetAllAsync();

            term = term.Trim().ToLower();

            return await _db.Invoices
                .Include(i => i.Subscriber)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.Generator)
                .Include(i => i.Payments)
                .Where(i =>
                    i.InvoiceNumber.ToLower().Contains(term) ||
                    i.Subscriber.FullName.ToLower().Contains(term) ||
                    i.Subscriber.SubscriberNumber.ToLower().Contains(term))
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        // ═══ Helpers ═══
        private DateTime CalculateDueDate(DateTime baseDate, int dueDay)
        {
            var nextMonth = baseDate.AddMonths(1);
            var day = Math.Min(dueDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
            return new DateTime(nextMonth.Year, nextMonth.Month, day);
        }

        private DateTime CalculateNextBillingDate(
            DateTime baseDate, SubscriptionType type, int dueDay)
        {
            var monthsToAdd = type switch
            {
                SubscriptionType.Monthly => 1,
                SubscriptionType.Quarterly => 3,
                SubscriptionType.SemiAnnual => 6,
                SubscriptionType.Annual => 12,
                _ => 1
            };

            var nextDate = baseDate.AddMonths(monthsToAdd);
            var day = Math.Min(dueDay, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
            return new DateTime(nextDate.Year, nextDate.Month, day);
        }
    }
}