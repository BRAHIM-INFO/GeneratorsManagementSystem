using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IInvoiceService _invoiceService;

        public PaymentService(ApplicationDbContext db, IInvoiceService invoiceService)
        {
            _db = db;
            _invoiceService = invoiceService;
        }

        // ═══ توليد رقم الإيصال: REC-26-00001 ═══
        public async Task<string> GenerateReceiptNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"REC-{year}-";

            var lastNumber = await _db.Payments
                .Where(p => p.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(p => p.ReceiptNumber)
                .Select(p => p.ReceiptNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            var newNumber = $"{prefix}{nextSeq:D5}";
            while (await _db.Payments.AnyAsync(p => p.ReceiptNumber == newNumber))
            {
                nextSeq++;
                newNumber = $"{prefix}{nextSeq:D5}";
            }

            return newNumber;
        }

        // ═══ الكل ═══
        public async Task<List<Payment>> GetAllAsync()
        {
            return await _db.Payments
                .Include(p => p.Subscriber)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Subscription)
                        .ThenInclude(s => s.Generator)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        // ═══ حسب الفاتورة ═══
        public async Task<List<Payment>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _db.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        // ═══ حسب المشترك ═══
        public async Task<List<Payment>> GetBySubscriberIdAsync(int subscriberId)
        {
            return await _db.Payments
                .Include(p => p.Invoice)
                .Where(p => p.SubscriberId == subscriberId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        // ═══ بالمعرّف ═══
        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _db.Payments
                .Include(p => p.Subscriber)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Subscription)
                        .ThenInclude(s => s.Generator)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ═══ إنشاء دفعة ═══
        public async Task<Payment> CreateAsync(Payment payment, string createdBy)
        {
            // التحقق من الفاتورة
            var invoice = await _db.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId)
                ?? throw new Exception("الفاتورة غير موجودة");

            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new Exception("لا يمكن الدفع على فاتورة ملغاة");

            var totalAmount = invoice.Amount - invoice.Discount + invoice.Tax + invoice.AdditionalCharges;
            var paidSoFar = invoice.Payments.Sum(p => p.Amount);
            var remainingAmount = totalAmount - paidSoFar;

            if (payment.Amount <= 0)
                throw new Exception("يجب أن يكون المبلغ أكبر من صفر");

            if (payment.Amount > remainingAmount)
                throw new Exception(
                    $"المبلغ المدخل ({payment.Amount:N0} د.ع) أكبر من المتبقي ({remainingAmount:N0} د.ع)");

            // إعدادات الدفعة
            payment.ReceiptNumber = await GenerateReceiptNumberAsync();
            payment.SubscriberId = invoice.SubscriberId;
            payment.CreatedAt = DateTime.Now;
            payment.CreatedBy = createdBy;

            if (string.IsNullOrWhiteSpace(payment.ReceivedBy))
                payment.ReceivedBy = createdBy;

            if (string.IsNullOrWhiteSpace(payment.Reference))
                payment.Reference = null;

            if (string.IsNullOrWhiteSpace(payment.Notes))
                payment.Notes = null;

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // تحديث حالة الفاتورة تلقائياً
            try
            {
                await _invoiceService.UpdateInvoiceStatusAsync(invoice.Id);
            }
            catch { /* تجاهل خطأ التحديث */ }

            return payment;
        }

        // ═══ حذف ═══
        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment == null) return false;

            var invoiceId = payment.InvoiceId;

            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();

            // تحديث حالة الفاتورة بعد الحذف
            try
            {
                await _invoiceService.UpdateInvoiceStatusAsync(invoiceId);
            }
            catch { }

            return true;
        }

        // ═══ الإحصائيات ═══
        public async Task<PaymentStats> GetStatsAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var payments = await _db.Payments.ToListAsync();

            return new PaymentStats
            {
                Total = payments.Count,
                TotalAmount = payments.Sum(p => p.Amount),
                TodayAmount = payments.Where(p => p.PaymentDate.Date == today).Sum(p => p.Amount),
                MonthAmount = payments.Where(p => p.PaymentDate.Date >= monthStart).Sum(p => p.Amount)
            };
        }
    }
}