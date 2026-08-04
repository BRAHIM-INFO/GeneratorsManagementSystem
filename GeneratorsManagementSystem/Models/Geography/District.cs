using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.Geography
{
    public class District
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "المحافظة")]
        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; } = null!;

        [Required(ErrorMessage = "اسم القضاء مطلوب")]
        [StringLength(100)]
        [Display(Name = "القضاء")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NameEn { get; set; }

        [Display(Name = "الترتيب")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // العلاقات
        public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    }
}