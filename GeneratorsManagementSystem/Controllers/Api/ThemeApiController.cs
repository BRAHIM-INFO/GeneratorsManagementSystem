using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers.Api
{
    [Route("api/theme")]
    [ApiController]
    [Authorize]
    public class ThemeApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Models.Identity.ApplicationUser> _userManager;

        public ThemeApiController(
            ApplicationDbContext context,
            UserManager<Models.Identity.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveTheme([FromBody] ThemeSettingsDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existing = _context.ThemeSettings
                .FirstOrDefault(t => t.UserId == user.Id);

            if (existing != null)
            {
                existing.LayoutMode = dto.LayoutMode;
                existing.NavbarType = dto.NavbarType;
                existing.SidebarColor = dto.SidebarColor;
                existing.SidebarBgImage = dto.SidebarBgImage;
                existing.SidebarBgImageEnabled = dto.SidebarBgImageEnabled;
                existing.CompactMenu = dto.CompactMenu;
                existing.SidebarWidth = dto.SidebarWidth;
                existing.BackgroundColor = dto.BackgroundColor;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                _context.ThemeSettings.Add(new ThemeSettings
                {
                    UserId = user.Id,
                    LayoutMode = dto.LayoutMode,
                    NavbarType = dto.NavbarType,
                    SidebarColor = dto.SidebarColor,
                    SidebarBgImage = dto.SidebarBgImage,
                    SidebarBgImageEnabled = dto.SidebarBgImageEnabled,
                    CompactMenu = dto.CompactMenu,
                    SidebarWidth = dto.SidebarWidth,
                    BackgroundColor = dto.BackgroundColor,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("load")]
        public async Task<IActionResult> LoadTheme()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var settings = _context.ThemeSettings
                .FirstOrDefault(t => t.UserId == user.Id);

            if (settings == null)
                return Ok(new ThemeSettingsDto());

            return Ok(new ThemeSettingsDto
            {
                LayoutMode = settings.LayoutMode,
                NavbarType = settings.NavbarType,
                SidebarColor = settings.SidebarColor,
                SidebarBgImage = settings.SidebarBgImage,
                SidebarBgImageEnabled = settings.SidebarBgImageEnabled,
                CompactMenu = settings.CompactMenu,
                SidebarWidth = settings.SidebarWidth,
                BackgroundColor = settings.BackgroundColor
            });
        }
    }

    public class ThemeSettingsDto
    {
        public string LayoutMode { get; set; } = "light";
        public string NavbarType { get; set; } = "fixed";
        public string SidebarColor { get; set; } = "gradient-purple";
        public string? SidebarBgImage { get; set; }
        public bool SidebarBgImageEnabled { get; set; } = false;
        public bool CompactMenu { get; set; } = false;
        public string SidebarWidth { get; set; } = "medium";
        public string BackgroundColor { get; set; } = "#F8F8F8";
    }
}