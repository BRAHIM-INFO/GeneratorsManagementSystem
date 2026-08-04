using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class OrganizationSettingsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "اسم المؤسسة (عربي)")]
        [Required(ErrorMessage = "اسم المؤسسة مطلوب")]
        [MaxLength(200)]
        public string OrganizationName { get; set; } = string.Empty;

        [Display(Name = "اسم المؤسسة (إنجليزي)")]
        [MaxLength(200)]
        public string? OrganizationNameEn { get; set; }

        [Display(Name = "الشعار / الوصف")]
        [MaxLength(500)]
        public string? Slogan { get; set; }

        [Display(Name = "شعار المؤسسة")]
        public string? LogoPath { get; set; }

        [Display(Name = "أيقونة الموقع")]
        public string? FaviconPath { get; set; }

        public IFormFile? LogoFile { get; set; }
        public IFormFile? FaviconFile { get; set; }

        // معلومات الاتصال
        [Display(Name = "البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Display(Name = "الهاتف الأساسي")]
        [MaxLength(20)]
        public string? Phone1 { get; set; }

        [Display(Name = "الهاتف الثانوي")]
        [MaxLength(20)]
        public string? Phone2 { get; set; }

        [Display(Name = "الفاكس")]
        [MaxLength(20)]
        public string? Fax { get; set; }

        [Display(Name = "الموقع الإلكتروني")]
        [MaxLength(200)]
        public string? Website { get; set; }

        // العنوان
        [Display(Name = "الدولة")]
        [MaxLength(100)]
        public string? Country { get; set; } = "ليبيا";

        [Display(Name = "المدينة")]
        [MaxLength(100)]
        public string? City { get; set; }

        [Display(Name = "المنطقة / الحي")]
        [MaxLength(100)]
        public string? District { get; set; }

        [Display(Name = "العنوان التفصيلي")]
        [MaxLength(500)]
        public string? Address { get; set; }

        [Display(Name = "الرمز البريدي")]
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        // البيانات القانونية
        [Display(Name = "الرقم الضريبي")]
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [Display(Name = "رقم السجل التجاري")]
        [MaxLength(50)]
        public string? CommercialRegister { get; set; }

        [Display(Name = "رقم الرخصة")]
        [MaxLength(50)]
        public string? LicenseNumber { get; set; }

        // وسائل التواصل الاجتماعي
        [Display(Name = "فيسبوك")]
        [MaxLength(200)]
        public string? Facebook { get; set; }

        [Display(Name = "تويتر")]
        [MaxLength(200)]
        public string? Twitter { get; set; }

        [Display(Name = "إنستجرام")]
        [MaxLength(200)]
        public string? Instagram { get; set; }

        [Display(Name = "لينكد إن")]
        [MaxLength(200)]
        public string? LinkedIn { get; set; }

        [Display(Name = "واتساب")]
        [MaxLength(200)]
        public string? WhatsApp { get; set; }

        // معلومات إضافية
        [Display(Name = "نبذة عن المؤسسة")]
        [MaxLength(1000)]
        public string? AboutUs { get; set; }

        [Display(Name = "ملاحظات")]
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}