namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class SettingsDashboardViewModel
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRoles { get; set; }
        public int ActiveUsers { get; set; }
        public string SystemVersion { get; set; } = "1.0.0";
        public DateTime LastBackup { get; set; }
        public int TotalSettings { get; set; }

        public List<SettingCard> Cards { get; set; } = new();
    }

    public class SettingCard
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Badge { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}