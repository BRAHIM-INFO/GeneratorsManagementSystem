using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.Geography
{
    public class Neighborhood
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "القضاء")]
        public int DistrictId { get; set; }
        public District District { get; set; } = null!;

        [Required(ErrorMessage = "اسم الحي مطلوب")]
        [StringLength(100)]
        [Display(Name = "الحي")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NameEn { get; set; }

        [Display(Name = "الترتيب")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // العلاقات
        public ICollection<Alley> Alleys { get; set; } = new List<Alley>();
    }
}