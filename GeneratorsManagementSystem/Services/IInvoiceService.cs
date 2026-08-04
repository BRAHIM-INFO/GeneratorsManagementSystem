using GeneratorsManagementSystem.Models;

namespace GeneratorsManagementSystem.Services
{
    public interface IInvoiceService
    {
        // ═══ توليد الأرقام ═══
        Task<string> GenerateInvoiceNumberAsync();

        // ═══ CRUD ═══
        Task<List<Invoice>> GetAllAsync();
        Task<List<Invoice>> GetBySubscriberIdAsync(int subscriberId);
        Task<List<Invoice>> GetBySubscriptionIdAsync(int subscriptionId);
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice> CreateAsync(Invoice invoice, string createdBy);
        Task<Invoice> UpdateAsync(Invoice invoice);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id, string reason);

        // ═══ توليد تلقائي ═══
        Task<Invoice> GenerateFirstInvoiceAsync(int subscriptionId, string createdBy);
        Task<List<Invoice>> GenerateMonthlyInvoicesAsync(string createdBy);

        // ═══ حالة الفاتورة ═══
        Task UpdateInvoiceStatusAsync(int invoiceId);
        Task UpdateAllOverdueStatusAsync();

        // ═══ إحصائيات وتنبيهات ═══
        Task<InvoiceStats> GetStatsAsync();
        Task<List<InvoiceAlert>> GetAlertsAsync(int daysAhead = 7);
        Task<List<Invoice>> GetOverdueInvoicesAsync();
        Task<List<Invoice>> GetUpcomingInvoicesAsync(int days = 7);
        Task<List<Invoice>> GetUnpaidInvoicesAsync();

        // ═══ البحث ═══
        Task<List<Invoice>> SearchAsync(string term);
    }

    public class InvoiceStats
    {
        public int Total { get; set; }
        public int Paid { get; set; }
        public int Unpaid { get; set; }
        public int PartiallyPaid { get; set; }
        public int Overdue { get; set; }
        public int Cancelled { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalOverdue { get; set; }
    }

    public class InvoiceAlert
    {
        public int InvoiceId { get; set; }
        public int SubscriberId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string SubscriberName { get; set; } = string.Empty;
        public string SubscriberPhone { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }  // موجب = قبل الاستحقاق، سالب = متأخر
        public string AlertType { get; set; } = string.Empty; // upcoming, today, overdue
        public string AlertLevel { get; set; } = string.Empty; // info, warning, danger
        public string AlertMessage { get; set; } = string.Empty;
        public string AlertIcon { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = string.Empty;
    }
}