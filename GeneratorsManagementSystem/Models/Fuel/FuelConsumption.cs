using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Fuel
{
    // ══════════════════════════════════════
    //  طريقة تسجيل الاستهلاك
    // ══════════════════════════════════════
    public enum ConsumptionMethod
    {
        [Display(Name = "تعبئة يدوية")]
        Manual = 1,

        [Display(Name = "من حساس (IoT)")]
        IoTSensor = 2
    }

    // ══════════════════════════════════════
    //  سجل استهلاك الوقود
    // ══════════════════════════════════════
    public class FuelConsumption
    {
        public int Id { get; set; }

        [Display(Name = "رقم السجل")]
        [StringLength(30)]
        public string ConsumptionNumber { get; set; } = string.Empty;

        // ═══ العلاقات ═══
        [Required]
        [Display(Name = "المولد")]
        public int GeneratorId { get; set; }
        public Generator Generator { get; set; } = null!;

        [Display(Name = "الحصة")]
        public int? FuelAllocationId { get; set; }
        public FuelAllocation? FuelAllocation { get; set; }

        // ═══ بيانات الاستهلاك ═══
        [Required]
        [Display(Name = "نوع الوقود")]
        public FuelKind FuelKind { get; set; } = FuelKind.Diesel;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        [Display(Name = "الكمية المستهلكة (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Display(Name = "المستوى قبل التعبئة")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? LevelBefore { get; set; }

        [Display(Name = "المستوى بعد التعبئة")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? LevelAfter { get; set; }

        [Display(Name = "تكلفة الاستهلاك")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; } = 0;

        [Required]
        [Display(Name = "تاريخ الاستهلاك")]
        public DateTime ConsumptionDate { get; set; } = DateTime.Now;

        [Display(Name = "طريقة التسجيل")]
        public ConsumptionMethod Method { get; set; } = ConsumptionMethod.Manual;

        [StringLength(100)]
        [Display(Name = "المسؤول عن التعبئة")]
        public string? FilledBy { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ═══ بيانات النظام ═══
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        // ═══ Computed Properties ═══
        [NotMapped]
        public string FuelKindText => FuelKind switch
        {
            FuelKind.Diesel => "ديزل",
            FuelKind.Gasoline => "بنزين",
            FuelKind.Gas => "غاز",
            FuelKind.HeavyOil => "زيت أسود",
            _ => "—"
        };

        [NotMapped]
        public string MethodText => Method switch
        {
            ConsumptionMethod.Manual => "يدوي",
            ConsumptionMethod.IoTSensor => "حساس",
            _ => "—"
        };

        [NotMapped]
        public string MethodBadgeClass => Method switch
        {
            ConsumptionMethod.Manual => "bg-primary",
            ConsumptionMethod.IoTSensor => "bg-info",
            _ => "bg-secondary"
        };
    }
}