using GeneratorsManagementSystem.Models.ViewModels.Settings;

namespace GeneratorsManagementSystem.Services
{
    public interface IRoleManagementService
    {
        Task<List<RoleListViewModel>> GetAllRolesAsync();
        Task<RoleFormViewModel?> GetRoleByIdAsync(string id);
        Task<(bool Success, string Message, string? RoleId)> CreateRoleAsync(RoleFormViewModel model);
        Task<(bool Success, string Message)> UpdateRoleAsync(RoleFormViewModel model);
        Task<(bool Success, string Message)> DeleteRoleAsync(string id);
        Task<List<string>> GetRolePermissionsAsync(string roleId);
    }
}