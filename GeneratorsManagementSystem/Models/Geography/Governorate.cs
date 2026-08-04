using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.Geography
{
    public class Governorate
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المحافظة مطلوب")]
        [StringLength(100)]
        [Display(Name = "المحافظة")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "الاسم بالإنجليزية")]
        public string? NameEn { get; set; }

        [Display(Name = "الترتيب")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // العلاقات
        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}