using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    public class DeviceType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الجهاز مطلوب")]
        [StringLength(100)]
        [Display(Name = "اسم الجهاز")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "السعر الافتراضي")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal DefaultPrice { get; set; }

        [Display(Name = "أمبير افتراضي")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultAmpere { get; set; } = 1;

        [Display(Name = "الأيقونة")]
        [StringLength(50)]
        public string? Icon { get; set; }

        [Display(Name = "اللون")]
        [StringLength(20)]
        public string? Color { get; set; }

        [Display(Name = "الترتيب")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}