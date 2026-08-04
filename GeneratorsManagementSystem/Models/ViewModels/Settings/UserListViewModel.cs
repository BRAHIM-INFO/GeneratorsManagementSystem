namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string? ProfilePicture { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class UsersStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int OnlineNow { get; set; }
        public List<UserListViewModel> Users { get; set; } = new();
    }
}