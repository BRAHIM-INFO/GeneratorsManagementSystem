using GeneratorsManagementSystem.Models.Settings;
using GeneratorsManagementSystem.Models.ViewModels.Settings;

namespace GeneratorsManagementSystem.Services
{
    public interface ISettingsService
    {
        // General Settings
        Task<GeneralSettingsViewModel> GetGeneralSettingsAsync();
        Task<bool> SaveGeneralSettingsAsync(GeneralSettingsViewModel model, string userId);

        // Organization Settings
        Task<OrganizationSettingsViewModel> GetOrganizationSettingsAsync();
        Task<bool> SaveOrganizationSettingsAsync(OrganizationSettingsViewModel model, string userId);

        // Generic Settings
        Task<string?> GetSettingAsync(string key);
        Task<T?> GetSettingAsync<T>(string key);
        Task<bool> SetSettingAsync(string key, string value, string userId, string? category = null, string dataType = "string");
        Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category);

        // Dashboard
        Task<SettingsDashboardViewModel> GetDashboardAsync();

        // Generator Settings
        Task<GeneratorSettingsViewModel> GetGeneratorSettingsAsync();
        Task<bool> SaveGeneratorSettingsAsync(GeneratorSettingsViewModel model, string userId);

        // Subscription Settings
        Task<SubscriptionSettingsViewModel> GetSubscriptionSettingsAsync();
        Task<bool> SaveSubscriptionSettingsAsync(SubscriptionSettingsViewModel model, string userId);

        // Billing Settings
        Task<BillingSettingsViewModel> GetBillingSettingsAsync();
        Task<bool> SaveBillingSettingsAsync(BillingSettingsViewModel model, string userId);
    }
}