using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.IoT
{
    public class SensorReading
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "الجهاز")]
        public int IoTDeviceId { get; set; }
        public IoTDevice IoTDevice { get; set; } = null!;

        [Required]
        [Display(Name = "المولد")]
        public int GeneratorId { get; set; }
        public Generator Generator { get; set; } = null!;

        [Required]
        [Display(Name = "نوع القراءة")]
        public SensorReadingType ReadingType { get; set; } = SensorReadingType.FuelLevel;

        // ═══ القيمة ═══

        [Required]
        [Display(Name = "القيمة")]
        [Column(TypeName = "decimal(12,4)")]
        public decimal Value { get; set; }

        [Display(Name = "الوحدة")]
        public SensorReadingUnit Unit { get; set; } = SensorReadingUnit.CM;

        [Display(Name = "القيمة المحسوبة (لتر)")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? CalculatedLiters { get; set; }

        // ═══ حالة الحساس ═══

        [Display(Name = "حالة الحساس")]
        public SensorStatus SensorStatus { get; set; } = SensorStatus.OK;

        [StringLength(200)]
        [Display(Name = "رسالة الحالة")]
        public string? StatusMessage { get; set; }

        // ═══ التوقيتات ═══

        [Required]
        [Display(Name = "وقت القراءة")]
        public DateTime ReadingTime { get; set; } = DateTime.Now;

        [Display(Name = "وقت الاستلام")]
        public DateTime ReceivedAt { get; set; } = DateTime.Now;

        // ═══ بيانات إضافية ═══

        [StringLength(50)]
        [Display(Name = "IP المُرسِل")]
        public string? SenderIp { get; set; }

        [StringLength(1000)]
        [Display(Name = "بيانات خام (JSON)")]
        public string? RawData { get; set; }

        // ═══ Computed Properties ═══

        [NotMapped]
        public string UnitText => Unit switch
        {
            SensorReadingUnit.CM => "سم",
            SensorReadingUnit.Liters => "لتر",
            SensorReadingUnit.Percentage => "%",
            SensorReadingUnit.Volts => "V",
            SensorReadingUnit.Amperes => "A",
            _ => ""
        };

        [NotMapped]
        public string StatusBadgeClass => SensorStatus switch
        {
            SensorStatus.OK => "bg-success",
            SensorStatus.Warning => "bg-warning",
            SensorStatus.Fault => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public enum SensorReadingType
    {
        [Display(Name = "مستوى الوقود")]
        FuelLevel = 1,

        [Display(Name = "الفولت")]
        Voltage = 2,

        [Display(Name = "الأمبير")]
        Current = 3,

        [Display(Name = "درجة الحرارة")]
        Temperature = 4,

        [Display(Name = "ضغط الزيت")]
        OilPressure = 5,

        [Display(Name = "ساعات التشغيل")]
        OperatingHours = 6,

        [Display(Name = "أخرى")]
        Other = 99
    }

    public enum SensorReadingUnit
    {
        [Display(Name = "سنتيمتر")]
        CM = 1,

        [Display(Name = "لتر")]
        Liters = 2,

        [Display(Name = "نسبة مئوية")]
        Percentage = 3,

        [Display(Name = "فولت")]
        Volts = 4,

        [Display(Name = "أمبير")]
        Amperes = 5,

        [Display(Name = "درجة")]
        Degrees = 6,

        [Display(Name = "ساعة")]
        Hours = 7
    }

    public enum SensorStatus
    {
        [Display(Name = "سليم")]
        OK = 1,

        [Display(Name = "تحذير")]
        Warning = 2,

        [Display(Name = "عطل")]
        Fault = 3
    }
}