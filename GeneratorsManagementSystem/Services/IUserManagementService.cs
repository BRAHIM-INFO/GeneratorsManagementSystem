using GeneratorsManagementSystem.Models.ViewModels.Settings;

namespace GeneratorsManagementSystem.Services
{
    public interface IUserManagementService
    {
        Task<UsersStatsViewModel> GetAllUsersAsync();
        Task<UserFormViewModel?> GetUserByIdAsync(string id);
        Task<(bool Success, string Message, string? UserId)> CreateUserAsync(UserFormViewModel model);
        Task<(bool Success, string Message)> UpdateUserAsync(UserFormViewModel model);
        Task<(bool Success, string Message)> DeleteUserAsync(string id);
        Task<(bool Success, string Message)> ToggleUserStatusAsync(string id);
        Task<(bool Success, string Message)> ResetPasswordAsync(string userId, string newPassword);
        Task<List<RoleSelectItem>> GetAvailableRolesAsync();
    }
}