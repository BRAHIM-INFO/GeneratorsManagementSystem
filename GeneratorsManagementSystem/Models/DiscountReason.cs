using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models
{
    public class DiscountReason
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم السبب مطلوب")]
        [StringLength(100)]
        [Display(Name = "سبب الخصم")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Display(Name = "نسبة الخصم الافتراضية (%)")]
        public decimal? DefaultPercentage { get; set; }

        [Display(Name = "اللون")]
        [StringLength(20)]
        public string? Color { get; set; }

        [Display(Name = "الأيقونة")]
        [StringLength(50)]
        public string? Icon { get; set; }

        [Display(Name = "الترتيب")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}