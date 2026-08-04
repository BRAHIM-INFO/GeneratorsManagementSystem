using GeneratorsManagementSystem.Models.Settings;
using Microsoft.AspNetCore.Identity;

namespace GeneratorsManagementSystem.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public string? UserLanguage { get; set; } = "ar";

        // Navigation
        public virtual ThemeSettings? ThemeSettings { get; set; }
    }
}