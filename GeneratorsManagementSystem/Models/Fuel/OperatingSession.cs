using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Fuel
{
    public class OperatingSession
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "المولد")]
        public int GeneratorId { get; set; }
        public Generator Generator { get; set; } = null!;

        // ═══ التوقيتات ═══

        [Required]
        [Display(Name = "وقت بدء التشغيل")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Display(Name = "وقت الإيقاف")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "ساعات التشغيل")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? DurationHours { get; set; }

        // ═══ قياسات الوقود ═══

        [Display(Name = "ارتفاع الوقود قبل التشغيل (سم)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelLevelBefore_CM { get; set; }

        [Display(Name = "كمية الوقود قبل التشغيل (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelLevelBefore_Liters { get; set; }

        [Display(Name = "ارتفاع الوقود بعد الإيقاف (سم)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelLevelAfter_CM { get; set; }

        [Display(Name = "كمية الوقود بعد الإيقاف (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelLevelAfter_Liters { get; set; }

        [Display(Name = "الاستهلاك (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelConsumed_Liters { get; set; }

        [Display(Name = "معدل الاستهلاك (لتر/ساعة)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ConsumptionRate { get; set; }

        // ═══ التكلفة ═══

        [Display(Name = "سعر اللتر وقت التشغيل")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PricePerLiter { get; set; }

        [Display(Name = "إجمالي التكلفة")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? TotalCost { get; set; }

        [Display(Name = "كلفة الساعة")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? HourlyCost { get; set; }

        // ═══ نوع المصدر ═══

        [Display(Name = "مصدر البيانات")]
        public SessionDataSource DataSource { get; set; } = SessionDataSource.Manual;

        [Display(Name = "نوع وقود مستخدم")]
        public FuelSourceType FuelSource { get; set; } = FuelSourceType.Service;

        // ═══ المستخدمون ═══

        [StringLength(100)]
        [Display(Name = "بدأ التشغيل بواسطة")]
        public string? StartedBy { get; set; }

        [StringLength(100)]
        [Display(Name = "أوقف التشغيل بواسطة")]
        public string? StoppedBy { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ═══ Timestamps ═══

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ═══ Computed Properties ═══

        [NotMapped]
        public bool IsActive => EndTime == null;

        [NotMapped]
        public string StatusText => IsActive ? "قيد التشغيل" : "منتهية";

        [NotMapped]
        public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";

        [NotMapped]
        public string StatusIcon => IsActive ? "fa-play-circle" : "fa-stop-circle";

        [NotMapped]
        public string FuelSourceText => FuelSource switch
        {
            FuelSourceType.Service => "وقود خدمة (حصة)",
            FuelSourceType.Commercial => "وقود ذمة (تجاري)",
            _ => "غير محدد"
        };

        [NotMapped]
        public string FuelSourceBadgeClass => FuelSource switch
        {
            FuelSourceType.Service => "bg-info",
            FuelSourceType.Commercial => "bg-warning",
            _ => "bg-secondary"
        };

        [NotMapped]
        public string DataSourceText => DataSource switch
        {
            SessionDataSource.Manual => "يدوي",
            SessionDataSource.IoT => "IoT (تلقائي)",
            _ => "غير محدد"
        };

        [NotMapped]
        public TimeSpan? Duration =>
            EndTime.HasValue ? EndTime.Value - StartTime : (TimeSpan?)null;

        [NotMapped]
        public string DurationText
        {
            get
            {
                if (!Duration.HasValue) return "قيد التشغيل";
                var d = Duration.Value;
                return $"{(int)d.TotalHours}س {d.Minutes}د";
            }
        }
    }

    public enum SessionDataSource
    {
        [Display(Name = "يدوي")]
        Manual = 1,

        [Display(Name = "IoT (تلقائي)")]
        IoT = 2
    }

    public enum FuelSourceType
    {
        [Display(Name = "وقود خدمة (حصة)")]
        Service = 1,

        [Display(Name = "وقود ذمة (تجاري)")]
        Commercial = 2
    }
}