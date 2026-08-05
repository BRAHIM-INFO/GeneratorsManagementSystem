using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Fuel
{
    public class FuelRefill
    {
        public int Id { get; set; }

        [Display(Name = "رقم التزويد")]
        [StringLength(30)]
        public string RefillNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "المولد")]
        public int GeneratorId { get; set; }
        public Generator Generator { get; set; } = null!;

        [Display(Name = "من حصة")]
        public int? FuelAllocationId { get; set; }
        public FuelAllocation? FuelAllocation { get; set; }

        // ═══ التاريخ ═══

        [Required]
        [Display(Name = "تاريخ التزويد")]
        public DateTime RefillDate { get; set; } = DateTime.Now;

        // ═══ القياسات ═══

        [Display(Name = "ارتفاع الوقود قبل التزويد (سم)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? LevelBefore_CM { get; set; }

        [Display(Name = "الكمية قبل التزويد (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? LevelBefore_Liters { get; set; }

        [Display(Name = "ارتفاع الوقود بعد التزويد (سم)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? LevelAfter_CM { get; set; }

        [Display(Name = "الكمية بعد التزويد (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? LevelAfter_Liters { get; set; }

        [Required]
        [Display(Name = "كمية التزويد (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal RefilledLiters { get; set; }

        // ═══ التكلفة ═══

        [Display(Name = "سعر اللتر")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PricePerLiter { get; set; }

        [Display(Name = "إجمالي التكلفة")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? TotalCost { get; set; }

        // ═══ معلومات ═══

        [StringLength(100)]
        [Display(Name = "قام بالتزويد")]
        public string? RefilledBy { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}