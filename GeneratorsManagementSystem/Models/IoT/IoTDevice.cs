using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.IoT
{
    public class IoTDevice
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الجهاز مطلوب")]
        [StringLength(50)]
        [Display(Name = "اسم الجهاز")]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "المولد المرتبط")]
        public int GeneratorId { get; set; }
        public Generator Generator { get; set; } = null!;

        [Required]
        [Display(Name = "نوع الجهاز")]
        public IoTDeviceType DeviceType { get; set; } = IoTDeviceType.ESP32;

        // ═══ API Authentication ═══

        [Required]
        [StringLength(64)]
        [Display(Name = "API Key")]
        public string ApiKey { get; set; } = string.Empty;

        [StringLength(64)]
        [Display(Name = "API Secret")]
        public string? ApiSecret { get; set; }

        // ═══ معلومات الجهاز ═══

        [StringLength(50)]
        [Display(Name = "MAC Address")]
        public string? MacAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        [StringLength(20)]
        [Display(Name = "إصدار Firmware")]
        public string? FirmwareVersion { get; set; }

        // ═══ الحالة والاتصال ═══

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "الحالة")]
        public IoTDeviceStatus Status { get; set; } = IoTDeviceStatus.Offline;

        [Display(Name = "آخر ظهور")]
        public DateTime? LastSeenAt { get; set; }

        [Display(Name = "آخر قراءة")]
        public DateTime? LastReadingAt { get; set; }

        [Display(Name = "عدد القراءات")]
        public long ReadingsCount { get; set; } = 0;

        // ═══ الإعدادات ═══

        [Display(Name = "فترة الإرسال (ثواني)")]
        public int ReportingIntervalSeconds { get; set; } = 60;

        [Display(Name = "الحساسات المرفقة")]
        [StringLength(500)]
        public string? AttachedSensors { get; set; }

        // ═══ ملاحظات ═══

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ═══ Timestamps ═══

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ═══ Computed Properties ═══

        [NotMapped]
        public string StatusText => Status switch
        {
            IoTDeviceStatus.Online => "متصل",
            IoTDeviceStatus.Offline => "غير متصل",
            IoTDeviceStatus.Fault => "عطل",
            _ => "غير محدد"
        };

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            IoTDeviceStatus.Online => "bg-success",
            IoTDeviceStatus.Offline => "bg-secondary",
            IoTDeviceStatus.Fault => "bg-danger",
            _ => "bg-light"
        };

        [NotMapped]
        public string StatusIcon => Status switch
        {
            IoTDeviceStatus.Online => "fa-circle text-success",
            IoTDeviceStatus.Offline => "fa-circle text-secondary",
            IoTDeviceStatus.Fault => "fa-exclamation-triangle text-danger",
            _ => "fa-question-circle"
        };

        [NotMapped]
        public string DeviceTypeText => DeviceType switch
        {
            IoTDeviceType.ESP32 => "ESP32",
            IoTDeviceType.ESP8266 => "ESP8266",
            IoTDeviceType.Arduino => "Arduino",
            IoTDeviceType.Other => "أخرى",
            _ => "غير محدد"
        };

        [NotMapped]
        public string LastSeenText
        {
            get
            {
                if (!LastSeenAt.HasValue) return "لم يتصل بعد";
                var diff = DateTime.Now - LastSeenAt.Value;
                if (diff.TotalMinutes < 1) return "الآن";
                if (diff.TotalMinutes < 60) return $"قبل {(int)diff.TotalMinutes} دقيقة";
                if (diff.TotalHours < 24) return $"قبل {(int)diff.TotalHours} ساعة";
                return $"قبل {(int)diff.TotalDays} يوم";
            }
        }

        [NotMapped]
        public bool IsConnected =>
            LastSeenAt.HasValue &&
            (DateTime.Now - LastSeenAt.Value).TotalMinutes < 5;

        // Navigation
        public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    }

    public enum IoTDeviceType
    {
        [Display(Name = "ESP32")]
        ESP32 = 1,

        [Display(Name = "ESP8266")]
        ESP8266 = 2,

        [Display(Name = "Arduino")]
        Arduino = 3,

        [Display(Name = "أخرى")]
        Other = 99
    }

    public enum IoTDeviceStatus
    {
        [Display(Name = "غير متصل")]
        Offline = 1,

        [Display(Name = "متصل")]
        Online = 2,

        [Display(Name = "عطل")]
        Fault = 3
    }
}