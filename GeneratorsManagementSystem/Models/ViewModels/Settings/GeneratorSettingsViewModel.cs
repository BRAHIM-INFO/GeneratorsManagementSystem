using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class GeneratorSettingsViewModel
    {
        public int Id { get; set; }

        // ترقيم المولدات
        [Display(Name = "بادئة رقم المولد")]
        [Required]
        [MaxLength(20)]
        public string GeneratorNumberPrefix { get; set; } = "GEN";

        [Display(Name = "طول الرقم")]
        [Range(1, 10)]
        public int GeneratorNumberLength { get; set; } = 2;

        [Display(Name = "بداية الترقيم")]
        [Range(1, 999999)]
        public int GeneratorNumberStart { get; set; } = 1;

        // الوقود
        [Display(Name = "نوع الوقود الافتراضي")]
        public string DefaultFuelType { get; set; } = "Diesel";

        [Display(Name = "سعة الخزان الافتراضية (لتر)")]
        [Range(0, 100000)]
        public decimal DefaultFuelTankCapacity { get; set; } = 200;

        [Display(Name = "معدل استهلاك الوقود (لتر/ساعة)")]
        [Range(0, 1000)]
        public decimal DefaultFuelConsumptionRate { get; set; } = 15;

        [Display(Name = "سعر لتر الوقود")]
        [Range(0, 1000)]
        public decimal FuelPricePerLiter { get; set; } = 3.5m;

        // تنبيهات الوقود
        [Display(Name = "نسبة تحذير انخفاض الوقود %")]
        [Range(5, 90)]
        public int LowFuelAlertPercentage { get; set; } = 20;

        [Display(Name = "نسبة الوقود الحرجة %")]
        [Range(1, 50)]
        public int CriticalFuelAlertPercentage { get; set; } = 10;

        [Display(Name = "تفعيل تنبيهات الوقود")]
        public bool EnableFuelAlerts { get; set; } = true;

        [Display(Name = "إرسال تنبيهات البريد الإلكتروني")]
        public bool EnableEmailFuelAlerts { get; set; } = false;

        [Display(Name = "إرسال تنبيهات SMS")]
        public bool EnableSmsFuelAlerts { get; set; } = false;

        // الصيانة
        [Display(Name = "فترة الصيانة الافتراضية (ساعة)")]
        [Range(10, 10000)]
        public int DefaultMaintenanceIntervalHours { get; set; } = 250;

        [Display(Name = "تنبيه قبل الصيانة (ساعة)")]
        [Range(1, 200)]
        public int MaintenanceAlertBeforeHours { get; set; } = 20;

        [Display(Name = "تفعيل تنبيهات الصيانة")]
        public bool EnableMaintenanceAlerts { get; set; } = true;

        // المواصفات
        [Display(Name = "الجهد الافتراضي (V)")]
        public decimal DefaultVoltage { get; set; } = 380;

        [Display(Name = "التردد الافتراضي (Hz)")]
        public decimal DefaultFrequency { get; set; } = 50;

        // حدود التشغيل
        [Display(Name = "الحد الأقصى لدرجة الحرارة (°C)")]
        public decimal MaxTemperature { get; set; } = 95;

        [Display(Name = "الحد الأدنى لضغط الزيت (PSI)")]
        public decimal MinOilPressure { get; set; } = 25;

        [Display(Name = "الحد الأقصى للحمل %")]
        [Range(50, 100)]
        public decimal MaxLoadPercentage { get; set; } = 85;

        // المراقبة
        [Display(Name = "تفعيل المراقبة المباشرة")]
        public bool EnableRealTimeMonitoring { get; set; } = true;

        [Display(Name = "فترة تحديث المراقبة (ثانية)")]
        [Range(5, 300)]
        public int MonitoringIntervalSeconds { get; set; } = 30;

        [Display(Name = "تسجيل أحداث المولد تلقائياً")]
        public bool AutoLogGeneratorEvents { get; set; } = true;
    }
}