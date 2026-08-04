using GeneratorsManagementSystem.Models;

namespace GeneratorsManagementSystem.Services
{
    public interface ISubscriptionService
    {
        Task<string> GenerateContractNumberAsync();
        Task<List<Subscription>> GetAllAsync();
        Task<List<Subscription>> GetBySubscriberIdAsync(int subscriberId);
        Task<List<Subscription>> GetByGeneratorIdAsync(int generatorId);
        Task<Subscription?> GetByIdAsync(int id);
        Task<Subscription> CreateAsync(Subscription subscription, string createdBy);
        Task<Subscription> UpdateAsync(Subscription subscription, string updatedBy);
        Task<bool> DeleteAsync(int id);
        Task<bool> SuspendAsync(int id, string reason, string updatedBy);
        Task<bool> ReactivateAsync(int id, string updatedBy);
        Task<bool> CancelAsync(int id, string reason, string updatedBy);
        Task<List<Subscription>> SearchAsync(string term);
        Task<SubscriptionStats> GetStatsAsync();
        Task<bool> CanCreateAsync(int subscriberId, int generatorId);
        Task<decimal> GetAvailableAmpereAsync(int generatorId);
    }

    public class SubscriptionStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Suspended { get; set; }
        public int Expired { get; set; }
        public int Cancelled { get; set; }
        public decimal TotalMonthlyRevenue { get; set; }
        public decimal TotalAmpereUsed { get; set; }
    }
}