using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "الاسم الكامل")]
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "اسم المستخدم")]
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "البريد الإلكتروني")]
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
        public string? Email { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "القسم")]
        [MaxLength(100)]
        public string? Department { get; set; }

        [Display(Name = "المسمى الوظيفي")]
        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [Display(Name = "الصورة الشخصية")]
        public string? ProfilePicture { get; set; }

        public IFormFile? ProfilePictureFile { get; set; }

        [Display(Name = "كلمة المرور")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "يجب أن تكون كلمة المرور 6 أحرف على الأقل")]
        public string? Password { get; set; }

        [Display(Name = "تأكيد كلمة المرور")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمة المرور غير متطابقة")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "اللغة")]
        public string Language { get; set; } = "ar";

        [Display(Name = "الأدوار")]
        public List<string> SelectedRoles { get; set; } = new();

        [Display(Name = "الحالة")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تأكيد البريد تلقائياً")]
        public bool EmailConfirmed { get; set; } = true;

        public List<RoleSelectItem> AvailableRoles { get; set; } = new();

        public bool IsEditMode => !string.IsNullOrEmpty(Id);
    }

    public class RoleSelectItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsSelected { get; set; }
    }
}