using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.Geography
{
    public class Alley
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "الحي")]
        public int NeighborhoodId { get; set; }
        public Neighborhood Neighborhood { get; set; } = null!;

        [Required(ErrorMessage = "اسم الزقاق مطلوب")]
        [StringLength(100)]
        [Display(Name = "الزقاق")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الترتيب")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}