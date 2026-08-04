using GeneratorsManagementSystem.Helpers;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Models.ViewModels.Settings;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserManagementService _userService;
        private readonly IRoleManagementService _roleService;
        public SettingsController(
            ISettingsService settingsService,
            UserManager<ApplicationUser> userManager,
            IUserManagementService userService,
            IRoleManagementService roleService,
            IWebHostEnvironment environment)
        {
            _settingsService = settingsService;
            _userManager = userManager;
            _userService = userService;
            _roleService = roleService;
            _environment = environment;
        }

        // GET: /Settings
        public async Task<IActionResult> Index()
        {
            var model = await _settingsService.GetDashboardAsync();
            return View(model);
        }

        #region General Settings

        // GET: /Settings/General
        public async Task<IActionResult> General()
        {
            var model = await _settingsService.GetGeneralSettingsAsync();
            return View(model);
        }

        // POST: /Settings/General
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> General(GeneralSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "يوجد أخطاء في البيانات المدخلة";
                return View(model);
            }

            var userId = _userManager.GetUserId(User) ?? "";
            var result = await _settingsService.SaveGeneralSettingsAsync(model, userId);

            if (result)
                TempData["SuccessMessage"] = "تم حفظ الإعدادات العامة بنجاح";
            else
                TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ الإعدادات";

            return RedirectToAction(nameof(General));
        }

        #endregion

        #region Organization Settings

        // GET: /Settings/Organization
        public async Task<IActionResult> Organization()
        {
            var model = await _settingsService.GetOrganizationSettingsAsync();
            return View(model);
        }

        // POST: /Settings/Organization
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Organization(OrganizationSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "يوجد أخطاء في البيانات المدخلة";
                return View(model);
            }

            // معالجة رفع الشعار
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                var logoPath = await UploadFileAsync(model.LogoFile, "logos");
                if (!string.IsNullOrEmpty(logoPath))
                    model.LogoPath = logoPath;
            }

            // معالجة رفع الأيقونة
            if (model.FaviconFile != null && model.FaviconFile.Length > 0)
            {
                var faviconPath = await UploadFileAsync(model.FaviconFile, "favicons");
                if (!string.IsNullOrEmpty(faviconPath))
                    model.FaviconPath = faviconPath;
            }

            var userId = _userManager.GetUserId(User) ?? "";
            var result = await _settingsService.SaveOrganizationSettingsAsync(model, userId);

            if (result)
                TempData["SuccessMessage"] = "تم حفظ بيانات المؤسسة بنجاح";
            else
                TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ البيانات";

            return RedirectToAction(nameof(Organization));
        }

        #endregion

        #region Helpers

        private async Task<string?> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/{folder}/{fileName}";
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Users Management

        // GET: /Settings/Users
        public async Task<IActionResult> Users()
        {
            var model = await _userService.GetAllUsersAsync();
            return View(model);
        }

        // GET: /Settings/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var model = new UserFormViewModel
            {
                AvailableRoles = await _userService.GetAvailableRolesAsync()
            };
            return View(model);
        }

        // POST: /Settings/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> CreateUser(UserFormViewModel model)
        {
            // إزالة validation للحقول غير المطلوبة
            ModelState.Remove(nameof(model.Id));
            ModelState.Remove(nameof(model.ProfilePicture));

            if (!ModelState.IsValid)
            {
                // لطباعة الأخطاء للتشخيص
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ErrorMessage"] = "أخطاء: " + string.Join(" | ", errors);
                model.AvailableRoles = await _userService.GetAvailableRolesAsync();
                return View(model);
            }

            var result = await _userService.CreateUserAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Users));
            }

            TempData["ErrorMessage"] = result.Message;
            model.AvailableRoles = await _userService.GetAvailableRolesAsync();
            return View(model);
        }

        // GET: /Settings/EditUser/{id}
        public async Task<IActionResult> EditUser(string id)
        {
            var model = await _userService.GetUserByIdAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود";
                return RedirectToAction(nameof(Users));
            }
            return View(model);
        }

        // POST: /Settings/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserFormViewModel model)
        {
            // إزالة التحقق من كلمة المرور في التعديل
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _userService.GetAvailableRolesAsync();
                return View(model);
            }

            var result = await _userService.UpdateUserAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Users));
            }

            TempData["ErrorMessage"] = result.Message;
            model.AvailableRoles = await _userService.GetAvailableRolesAsync();
            return View(model);
        }

        // POST: /Settings/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        // POST: /Settings/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var result = await _userService.ToggleUserStatusAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        #endregion

        #region Roles Management

        // GET: /Settings/Roles
        public async Task<IActionResult> Roles()
        {
            var model = await _roleService.GetAllRolesAsync();
            return View(model);
        }

        // GET: /Settings/CreateRole
        public IActionResult CreateRole()
        {
            var model = new RoleFormViewModel
            {
                AvailablePermissions = PermissionsList.GetAllPermissions()
            };
            return View(model);
        }

        // POST: /Settings/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailablePermissions = PermissionsList.GetAllPermissions();
                return View(model);
            }

            var result = await _roleService.CreateRoleAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Roles));
            }

            TempData["ErrorMessage"] = result.Message;
            model.AvailablePermissions = PermissionsList.GetAllPermissions();
            return View(model);
        }

        // GET: /Settings/EditRole/{id}
        public async Task<IActionResult> EditRole(string id)
        {
            var model = await _roleService.GetRoleByIdAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "الدور غير موجود";
                return RedirectToAction(nameof(Roles));
            }
            return View(model);
        }

        // POST: /Settings/EditRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(RoleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailablePermissions = PermissionsList.GetAllPermissions();
                return View(model);
            }

            var result = await _roleService.UpdateRoleAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Roles));
            }

            TempData["ErrorMessage"] = result.Message;
            model.AvailablePermissions = PermissionsList.GetAllPermissions();
            return View(model);
        }

        // POST: /Settings/DeleteRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        #endregion


        #region Generator Settings

        public async Task<IActionResult> Generators()
        {
            var model = await _settingsService.GetGeneratorSettingsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generators(GeneratorSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User) ?? "";
            var result = await _settingsService.SaveGeneratorSettingsAsync(model, userId);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] =
                result ? "تم حفظ إعدادات المولدات بنجاح" : "حدث خطأ أثناء الحفظ";

            return RedirectToAction(nameof(Generators));
        }

        #endregion

        #region Subscription Settings

        public async Task<IActionResult> Subscriptions()
        {
            var model = await _settingsService.GetSubscriptionSettingsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscriptions(SubscriptionSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User) ?? "";
            var result = await _settingsService.SaveSubscriptionSettingsAsync(model, userId);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] =
                result ? "تم حفظ إعدادات الاشتراكات بنجاح" : "حدث خطأ أثناء الحفظ";

            return RedirectToAction(nameof(Subscriptions));
        }

        #endregion

        #region Billing Settings

        public async Task<IActionResult> Billing()
        {
            var model = await _settingsService.GetBillingSettingsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Billing(BillingSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User) ?? "";
            var result = await _settingsService.SaveBillingSettingsAsync(model, userId);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] =
                result ? "تم حفظ إعدادات الفوترة بنجاح" : "حدث خطأ أثناء الحفظ";

            return RedirectToAction(nameof(Billing));
        }

        #endregion
    }
}