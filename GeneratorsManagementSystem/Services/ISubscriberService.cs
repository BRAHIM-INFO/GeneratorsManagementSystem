using GeneratorsManagementSystem.Models;

namespace GeneratorsManagementSystem.Services
{
    public interface ISubscriberService
    {
        Task<string> GenerateNumberAsync();
        Task<List<Subscriber>> GetAllAsync();
        Task<Subscriber?> GetByIdAsync(int id);
        Task<Subscriber> CreateAsync(Subscriber subscriber, string createdBy);
        Task<Subscriber> UpdateAsync(Subscriber subscriber, string updatedBy);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<List<Subscriber>> SearchAsync(string searchTerm);
        Task<SubscriberStats> GetStatsAsync();
    }

    public class SubscriberStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int WithSubscriptions { get; set; }
        public int WithoutSubscriptions { get; set; }
    }
}