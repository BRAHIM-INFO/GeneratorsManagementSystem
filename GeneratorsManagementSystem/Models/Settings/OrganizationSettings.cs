using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.Settings
{
    public class OrganizationSettings
    {
        [Key]
        public int Id { get; set; }

        // بيانات المؤسسة الأساسية
        [Required(ErrorMessage = "اسم المؤسسة مطلوب")]
        [MaxLength(200)]
        public string OrganizationName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? OrganizationNameEn { get; set; }

        [MaxLength(500)]
        public string? Slogan { get; set; }

        [MaxLength(500)]
        public string? LogoPath { get; set; }

        [MaxLength(500)]
        public string? FaviconPath { get; set; }

        // معلومات الاتصال
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone1 { get; set; }

        [MaxLength(20)]
        public string? Phone2 { get; set; }

        [MaxLength(20)]
        public string? Fax { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        // العنوان
        [MaxLength(100)]
        public string? Country { get; set; } = "ليبيا";

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        // البيانات القانونية
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [MaxLength(50)]
        public string? CommercialRegister { get; set; }

        [MaxLength(50)]
        public string? LicenseNumber { get; set; }

        // وسائل التواصل الاجتماعي
        [MaxLength(200)]
        public string? Facebook { get; set; }

        [MaxLength(200)]
        public string? Twitter { get; set; }

        [MaxLength(200)]
        public string? Instagram { get; set; }

        [MaxLength(200)]
        public string? LinkedIn { get; set; }

        [MaxLength(200)]
        public string? WhatsApp { get; set; }

        // معلومات إضافية
        [MaxLength(1000)]
        public string? AboutUs { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(450)]
        public string? UpdatedBy { get; set; }
    }
}