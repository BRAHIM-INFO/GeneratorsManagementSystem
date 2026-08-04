using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Models.ViewModels.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _environment = environment;
        }

        public async Task<UsersStatsViewModel> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    Department = user.Department,
                    JobTitle = user.JobTitle,
                    ProfilePicture = user.ProfilePicture,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                });
            }

            return new UsersStatsViewModel
            {
                TotalUsers = userList.Count,
                ActiveUsers = userList.Count(u => u.IsActive),
                InactiveUsers = userList.Count(u => !u.IsActive),
                OnlineNow = userList.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value > DateTime.Now.AddMinutes(-15)),
                Users = userList.OrderByDescending(u => u.CreatedAt).ToList()
            };
        }

        public async Task<UserFormViewModel?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await GetAvailableRolesAsync();

            foreach (var role in allRoles)
            {
                role.IsSelected = userRoles.Contains(role.Name);
            }

            return new UserFormViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                JobTitle = user.JobTitle,
                ProfilePicture = user.ProfilePicture,
                Language = user.UserLanguage ?? "ar",
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                SelectedRoles = userRoles.ToList(),
                AvailableRoles = allRoles
            };
        }

        public async Task<(bool Success, string Message, string? UserId)> CreateUserAsync(UserFormViewModel model)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                    return (false, "البريد الإلكتروني مستخدم بالفعل", null);

                var existingUserName = await _userManager.FindByNameAsync(model.UserName);
                if (existingUserName != null)
                    return (false, "اسم المستخدم مستخدم بالفعل", null);

                // معالجة الصورة الشخصية
                string? profilePicturePath = null;
                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    profilePicturePath = await UploadProfilePictureAsync(model.ProfilePictureFile);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FullName = model.FullName,
                    Department = model.Department,
                    JobTitle = model.JobTitle,
                    ProfilePicture = profilePicturePath,
                    UserLanguage = model.Language,
                    IsActive = model.IsActive,
                    EmailConfirmed = model.EmailConfirmed,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password ?? "Admin@123456");

                if (!result.Succeeded)
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

                // إضافة الأدوار
                if (model.SelectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                return (true, "تم إنشاء المستخدم بنجاح", user.Id);
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(UserFormViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Id))
                    return (false, "معرف المستخدم مطلوب");

                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return (false, "المستخدم غير موجود");

                // التحقق من البريد الإلكتروني
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != model.Id)
                        return (false, "البريد الإلكتروني مستخدم بالفعل");
                }

                // معالجة الصورة الشخصية
                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    var newPath = await UploadProfilePictureAsync(model.ProfilePictureFile);
                    if (newPath != null) user.ProfilePicture = newPath;
                }

                user.FullName = model.FullName;
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Department = model.Department;
                user.JobTitle = model.JobTitle;
                user.UserLanguage = model.Language;
                user.IsActive = model.IsActive;
                user.EmailConfirmed = model.EmailConfirmed;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return (false, string.Join(", ", updateResult.Errors.Select(e => e.Description)));

                // تحديث الأدوار
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();
                var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                if (rolesToAdd.Any())
                    await _userManager.AddToRolesAsync(user, rolesToAdd);

                // تغيير كلمة المرور إذا تم توفيرها
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    if (!passResult.Succeeded)
                        return (false, "تم تحديث البيانات لكن فشل تغيير كلمة المرور");
                }

                return (true, "تم تحديث المستخدم بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return (false, "المستخدم غير موجود");

                // منع حذف المستخدم Admin الرئيسي
                if (user.Email == "admin@gms.com")
                    return (false, "لا يمكن حذف حساب المدير الرئيسي");

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return (true, "تم حذف المستخدم بنجاح");

                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ToggleUserStatusAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return (false, "المستخدم غير موجود");

                if (user.Email == "admin@gms.com")
                    return (false, "لا يمكن تعطيل حساب المدير الرئيسي");

                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                return (true, user.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string userId, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return (false, "المستخدم غير موجود");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                    return (true, "تم إعادة تعيين كلمة المرور بنجاح");

                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<List<RoleSelectItem>> GetAvailableRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(r => new RoleSelectItem
            {
                Id = r.Id,
                Name = r.Name ?? "",
                DisplayName = r.Name
            }).ToList();
        }

        private async Task<string?> UploadProfilePictureAsync(IFormFile file)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/profiles/{fileName}";
            }
            catch
            {
                return null;
            }
        }
    }
}