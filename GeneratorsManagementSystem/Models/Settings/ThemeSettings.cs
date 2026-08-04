namespace GeneratorsManagementSystem.Models.Settings
{
    public class ThemeSettings
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Layout
        public string LayoutMode { get; set; } = "light"; // light, dark, transparent
        public string NavbarType { get; set; } = "fixed"; // fixed, static

        // Sidebar
        public string SidebarColor { get; set; } = "gradient-purple";
        public string? SidebarBgImage { get; set; }
        public bool SidebarBgImageEnabled { get; set; } = false;
        public bool CompactMenu { get; set; } = false;
        public string SidebarWidth { get; set; } = "medium"; // large, medium, small

        // Colors
        public string PrimaryColor { get; set; } = "#7367F0";
        public string BackgroundColor { get; set; } = "#F8F8F8";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    } 
     
}