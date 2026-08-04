using GeneratorsManagementSystem.Helpers;
using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class RoleListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public int UsersCount { get; set; }
        public int PermissionsCount { get; set; }
        public bool IsSystemRole { get; set; }
    }

    public class RoleFormViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "اسم الدور (بالإنجليزية)")]
        [Required(ErrorMessage = "اسم الدور مطلوب")]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "يجب أن يحتوي على أحرف إنجليزية وأرقام فقط")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الاسم المعروض (بالعربية)")]
        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [Display(Name = "الوصف")]
        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsSystemRole { get; set; } = false;

        public List<string> SelectedPermissions { get; set; } = new();

        public List<PermissionGroup> AvailablePermissions { get; set; } = new();

        public bool IsEditMode => !string.IsNullOrEmpty(Id);
    }
}