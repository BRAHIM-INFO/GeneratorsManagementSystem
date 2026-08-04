using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Helpers;
using GeneratorsManagementSystem.Models.ViewModels.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GeneratorsManagementSystem.Services
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleManagementService(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<List<RoleListViewModel>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var result = new List<RoleListViewModel>();

            var systemRoles = new[] { "Admin", "Manager", "Employee", "Subscriber" };

            foreach (var role in roles)
            {
                var usersInRole = await _context.UserRoles
                    .Where(ur => ur.RoleId == role.Id)
                    .CountAsync();

                var permissionsCount = await _context.RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "Permission")
                    .CountAsync();

                result.Add(new RoleListViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? "",
                    DisplayName = GetRoleDisplayName(role.Name ?? ""),
                    Description = GetRoleDescription(role.Name ?? ""),
                    UsersCount = usersInRole,
                    PermissionsCount = permissionsCount,
                    IsSystemRole = systemRoles.Contains(role.Name)
                });
            }

            return result;
        }

        public async Task<RoleFormViewModel?> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return null;

            var permissions = await GetRolePermissionsAsync(id);
            var allPermissions = PermissionsList.GetAllPermissions();

            return new RoleFormViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                DisplayName = GetRoleDisplayName(role.Name ?? ""),
                Description = GetRoleDescription(role.Name ?? ""),
                IsSystemRole = new[] { "Admin", "Manager", "Employee", "Subscriber" }.Contains(role.Name),
                SelectedPermissions = permissions,
                AvailablePermissions = allPermissions
            };
        }

        public async Task<(bool Success, string Message, string? RoleId)> CreateRoleAsync(RoleFormViewModel model)
        {
            try
            {
                if (await _roleManager.RoleExistsAsync(model.Name))
                    return (false, "اسم الدور موجود بالفعل", null);

                var role = new IdentityRole(model.Name);
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

                // إضافة الصلاحيات
                foreach (var permission in model.SelectedPermissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }

                return (true, "تم إنشاء الدور بنجاح", role.Id);
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateRoleAsync(RoleFormViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Id))
                    return (false, "معرف الدور مطلوب");

                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                    return (false, "الدور غير موجود");

                // منع تعديل اسم الأدوار النظام
                var systemRoles = new[] { "Admin", "Manager", "Employee", "Subscriber" };
                if (!systemRoles.Contains(role.Name) && role.Name != model.Name)
                {
                    role.Name = model.Name;
                    role.NormalizedName = model.Name.ToUpper();
                    await _roleManager.UpdateAsync(role);
                }

                // تحديث الصلاحيات
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var existingPermissions = existingClaims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();

                var permissionsToRemove = existingPermissions.Except(model.SelectedPermissions).ToList();
                var permissionsToAdd = model.SelectedPermissions.Except(existingPermissions).ToList();

                foreach (var perm in permissionsToRemove)
                {
                    var claim = existingClaims.First(c => c.Type == "Permission" && c.Value == perm);
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                foreach (var perm in permissionsToAdd)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", perm));
                }

                return (true, "تم تحديث الدور بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteRoleAsync(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return (false, "الدور غير موجود");

                // منع حذف الأدوار الأساسية
                var systemRoles = new[] { "Admin", "Manager", "Employee", "Subscriber" };
                if (systemRoles.Contains(role.Name))
                    return (false, "لا يمكن حذف الأدوار الأساسية للنظام");

                // التحقق من وجود مستخدمين
                var usersInRole = await _context.UserRoles.CountAsync(ur => ur.RoleId == id);
                if (usersInRole > 0)
                    return (false, $"لا يمكن حذف الدور لوجود {usersInRole} مستخدم مرتبط به");

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return (true, "تم حذف الدور بنجاح");

                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleId)
        {
            return await _context.RoleClaims
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == "Permission")
                .Select(rc => rc.ClaimValue!)
                .ToListAsync();
        }

        private string GetRoleDisplayName(string roleName)
        {
            return roleName switch
            {
                "Admin" => "مدير النظام",
                "Manager" => "مدير",
                "Employee" => "موظف",
                "Subscriber" => "مشترك",
                _ => roleName
            };
        }

        private string GetRoleDescription(string roleName)
        {
            return roleName switch
            {
                "Admin" => "صلاحيات كاملة على النظام",
                "Manager" => "إدارة العمليات اليومية",
                "Employee" => "الوصول للمهام الأساسية",
                "Subscriber" => "مستخدم مشترك في النظام",
                _ => "دور مخصص"
            };
        }
    }
}