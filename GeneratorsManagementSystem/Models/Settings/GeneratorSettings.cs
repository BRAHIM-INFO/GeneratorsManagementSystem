using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Settings
{
    public class GeneratorSettings
    {
        [Key]
        public int Id { get; set; }

        // إعدادات ترقيم المولدات
        [MaxLength(20)]
        public string GeneratorNumberPrefix { get; set; } = "GEN";

        public int GeneratorNumberLength { get; set; } = 2;
        public int GeneratorNumberStart { get; set; } = 1;

        // إعدادات الوقود الافتراضية
        [MaxLength(50)]
        public string DefaultFuelType { get; set; } = "Diesel"; // Diesel, Gasoline, Gas

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultFuelTankCapacity { get; set; } = 200;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultFuelConsumptionRate { get; set; } = 15; // لتر/ساعة

        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelPricePerLiter { get; set; } = 3.5m;

        // تنبيهات الوقود
        [Range(5, 90)]
        public int LowFuelAlertPercentage { get; set; } = 20; // نسبة تحذير انخفاض الوقود

        [Range(1, 50)]
        public int CriticalFuelAlertPercentage { get; set; } = 10; // نسبة حرجة

        public bool EnableFuelAlerts { get; set; } = true;
        public bool EnableEmailFuelAlerts { get; set; } = false;
        public bool EnableSmsFuelAlerts { get; set; } = false;

        // إعدادات الصيانة
        public int DefaultMaintenanceIntervalHours { get; set; } = 250; // كل 250 ساعة
        public int MaintenanceAlertBeforeHours { get; set; } = 20; // تنبيه قبل 20 ساعة
        public bool EnableMaintenanceAlerts { get; set; } = true;

        // مواصفات افتراضية
        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultVoltage { get; set; } = 380;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultFrequency { get; set; } = 50;

        // حدود التشغيل
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxTemperature { get; set; } = 95; // درجة مئوية

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinOilPressure { get; set; } = 25; // PSI

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxLoadPercentage { get; set; } = 85;

        // إعدادات المراقبة
        public bool EnableRealTimeMonitoring { get; set; } = true;
        public int MonitoringIntervalSeconds { get; set; } = 30;
        public bool AutoLogGeneratorEvents { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(450)]
        public string? UpdatedBy { get; set; }
    }
}