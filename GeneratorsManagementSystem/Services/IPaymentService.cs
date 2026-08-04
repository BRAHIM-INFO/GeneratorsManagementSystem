using GeneratorsManagementSystem.Models;

namespace GeneratorsManagementSystem.Services
{
    public interface IPaymentService
    {
        Task<string> GenerateReceiptNumberAsync();
        Task<List<Payment>> GetAllAsync();
        Task<List<Payment>> GetByInvoiceIdAsync(int invoiceId);
        Task<List<Payment>> GetBySubscriberIdAsync(int subscriberId);
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment> CreateAsync(Payment payment, string createdBy);
        Task<bool> DeleteAsync(int id);
        Task<PaymentStats> GetStatsAsync();
    }

    public class PaymentStats
    {
        public int Total { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TodayAmount { get; set; }
        public decimal MonthAmount { get; set; }
    }
}