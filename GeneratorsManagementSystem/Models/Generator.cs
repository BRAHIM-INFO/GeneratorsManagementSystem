using GeneratorsManagementSystem.Models.Fuel;
using GeneratorsManagementSystem.Models.IoT;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    public class Generator
    {
        [Key]
        public int Id { get; set; }

        // ─── رقم المولد (تلقائي) ───
        [Display(Name = "رقم المولد")]
        public string GeneratorNumber { get; set; } = string.Empty;

        // ─── البيانات الأساسية ───
        [Required(ErrorMessage = "اسم المولد مطلوب")]
        [Display(Name = "اسم المولد")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الماركة")]
        [MaxLength(100)]
        public string? Brand { get; set; }

        [Display(Name = "الموديل")]
        [MaxLength(100)]
        public string? ModelNumber { get; set; }


        [Display(Name = "الرقم التسلسلي")]
        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [Display(Name = "سنة الصنع")]
        public int? ManufactureYear { get; set; }

        // ─── القدرة والمواصفات ───
        [Display(Name = "القدرة (KVA)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PowerKVA { get; set; }

        [Display(Name = "القدرة (KW)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PowerKW { get; set; }

        [Display(Name = "الجهد الكهربائي (V)")]
        public int? Voltage { get; set; }

        [Display(Name = "التيار (Hz)")]
        public int? Frequency { get; set; }

        [Display(Name = "الحد الأقصى للأمبير")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxAmpere { get; set; }

        [Display(Name = "نوع الوقود")]
        public FuelType FuelType { get; set; } = FuelType.Diesel;

        [Display(Name = "سعة خزان الوقود (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelTankCapacity { get; set; }

        [Display(Name = "معدل استهلاك الوقود (لتر/ساعة)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelConsumptionRate { get; set; }

        // ─── الموقع ───
        [Display(Name = "المنطقة")]
        [MaxLength(100)]
        public string? Area { get; set; }

        [Display(Name = "الموقع التفصيلي")]
        [MaxLength(300)]
        public string? Location { get; set; }

        [Display(Name = "خط الطول")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal? Longitude { get; set; }

        [Display(Name = "خط العرض")]
        [Column(TypeName = "decimal(18,6)")]
        public decimal? Latitude { get; set; }

        // ─── التشغيل ───
        [Display(Name = "تاريخ بدء التشغيل")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "إجمالي ساعات التشغيل")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalRunningHours { get; set; } = 0;

        [Display(Name = "ساعات التشغيل اليوم")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TodayRunningHours { get; set; } = 0;

        // ─── مستوى الوقود الحالي ───
        [Display(Name = "مستوى الوقود الحالي (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentFuelLevel { get; set; }

        [Display(Name = "آخر تعبئة وقود")]
        public DateTime? LastFuelRefill { get; set; }

        // ─── الصيانة ───
        [Display(Name = "آخر صيانة")]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "الصيانة القادمة")]
        public DateTime? NextMaintenanceDate { get; set; }

        [Display(Name = "ساعات الصيانة الدورية")]
        public int? MaintenanceIntervalHours { get; set; }

        // ─── الحالة ───
        [Display(Name = "حالة المولد")]
        public GeneratorStatus Status { get; set; } = GeneratorStatus.Active;

        [Display(Name = "سبب التوقف")]
        [MaxLength(300)]
        public string? StopReason { get; set; }

        // ─── بيانات مباشرة (Real-time) ───
        [Display(Name = "الحمل الحالي (A)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentLoad { get; set; }

        [Display(Name = "درجة الحرارة (°C)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Temperature { get; set; }

        [Display(Name = "ضغط الزيت")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? OilPressure { get; set; }

        [Display(Name = "آخر تحديث للبيانات")]
        public DateTime? LastDataUpdate { get; set; }

        // ─── الملاحظات ───
        [Display(Name = "ملاحظات")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ─── بيانات النظام ───
        [Display(Name = "تاريخ الإضافة")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التعديل")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "أضيف بواسطة")]
        public string? CreatedBy { get; set; }

        // ─── Navigation ───
        //public virtual ICollection<Subscriber> Subscribers { get; set; }
        //    = new List<Subscriber>();

        // ═══ سجلات استهلاك الوقود ═══
        public ICollection<Models.Fuel.FuelConsumption> FuelConsumptions { get; set; }
            = new List<Models.Fuel.FuelConsumption>();

        // ═══ Computed: إجمالي الوقود المستهلك ═══
        [NotMapped]
        public decimal TotalFuelConsumed =>
            FuelConsumptions?.Sum(c => c.Quantity) ?? 0;


        public virtual ICollection<GeneratorLog> Logs { get; set; }
            = new List<GeneratorLog>();
        public virtual ICollection<FuelRecord> FuelRecords { get; set; }
            = new List<FuelRecord>();

        

        [NotMapped]
        public decimal UsedAmpere =>
            Subscriptions?
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Sum(s => s.Ampere) ?? 0;


        [NotMapped]
        public decimal LoadPercentage =>
            MaxAmpere.HasValue && MaxAmpere > 0
                ? Math.Round((UsedAmpere / MaxAmpere.Value) * 100, 1)
                : 0;

       

        [NotMapped]
        public decimal? FuelLevelPercentage =>
            FuelTankCapacity.HasValue && FuelTankCapacity > 0 && CurrentFuelLevel.HasValue
                ? Math.Round((CurrentFuelLevel.Value / FuelTankCapacity.Value) * 100, 1)
                : null;

        [NotMapped]
        public bool NeedsMaintenanceSoon =>
            NextMaintenanceDate.HasValue &&
            NextMaintenanceDate.Value <= DateTime.Now.AddDays(7);


        

        // ═══ علاقة مع الاشتراكات ═══
        public ICollection<Subscription> Subscriptions { get; set; }
            = new List<Subscription>();

        // ═══ علاقة مع المصاريف ═══
        public ICollection<Models.Accounting.Expense> Expenses { get; set; }
            = new List<Models.Accounting.Expense>();

        // ═══ Computed: عدد المشتركين النشطين ═══
        [NotMapped]
        public int ActiveSubscribersCount =>
            Subscriptions?.Count(s => s.Status == SubscriptionStatus.Active) ?? 0;

        // ═══ Computed: إجمالي الإيرادات ═══
        [NotMapped]
        public decimal TotalRevenue =>
          Subscriptions?
         .SelectMany(s => s.Invoices ?? new List<Invoice>())
         .SelectMany(i => i.Payments ?? new List<Payment>())
         .Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal TotalExpenses =>
            Expenses?.Sum(e => e.Amount) ?? 0;

        [NotMapped]
        public decimal NetProfit => TotalRevenue - TotalExpenses;

        //-***********************************************

        // ═══════════════════════════════════════════════════
        //  🆕 نوع التشغيل وإعدادات الخزان
        // ═══════════════════════════════════════════════════

        [Display(Name = "نوع التشغيل")]
        public GeneratorOperatingMode OperatingMode { get; set; } = GeneratorOperatingMode.Manual;

        [Display(Name = "ارتفاع الخزان (سم)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? TankHeightCM { get; set; }

        [Display(Name = "كل سم = كم لتر")]
        [Column(TypeName = "decimal(10,4)")]
        public decimal? LitersPerCM { get; set; }

        [Display(Name = "معدل الاستهلاك (لتر/ساعة)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelConsumptionRatePerHour { get; set; }

        [Display(Name = "سعر اللتر الحالي")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentFuelPricePerLiter { get; set; }

        [Display(Name = "مستوى الوقود الحالي (سم)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentFuelLevelCM { get; set; }

        [Display(Name = "الكمية الحالية (لتر)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentFuelLiters { get; set; }

        [Display(Name = "يعمل الآن")]
        public bool IsRunning { get; set; } = false;

        [Display(Name = "وقت آخر تشغيل")]
        public DateTime? LastStartTime { get; set; }

        [Display(Name = "وقت آخر إيقاف")]
        public DateTime? LastStopTime { get; set; }

       

        // Computed Properties
        [NotMapped]
        public string OperatingModeText => OperatingMode switch
        {
            GeneratorOperatingMode.Automatic => "تلقائي (IoT)",
            GeneratorOperatingMode.Manual => "يدوي",
            _ => "غير محدد"
        };

        [NotMapped]
        public string OperatingModeBadgeClass => OperatingMode switch
        {
            GeneratorOperatingMode.Automatic => "bg-primary",
            GeneratorOperatingMode.Manual => "bg-secondary",
            _ => "bg-light"
        };

        [NotMapped]
        public string OperatingModeIcon => OperatingMode switch
        {
            GeneratorOperatingMode.Automatic => "fa-microchip",
            GeneratorOperatingMode.Manual => "fa-user-cog",
            _ => "fa-question"
        };

        [NotMapped]
        public decimal? EstimatedRunningHoursRemaining
        {
            get
            {
                if (!CurrentFuelLiters.HasValue || !FuelConsumptionRatePerHour.HasValue || FuelConsumptionRatePerHour.Value == 0)
                    return null;
                return Math.Round(CurrentFuelLiters.Value / FuelConsumptionRatePerHour.Value, 2);
            }
        }

        [NotMapped]
        public decimal? HourlyOperatingCost
        {
            get
            {
                if (!FuelConsumptionRatePerHour.HasValue || !CurrentFuelPricePerLiter.HasValue)
                    return null;
                return Math.Round(FuelConsumptionRatePerHour.Value * CurrentFuelPricePerLiter.Value, 2);
            }
        }

        // Navigation Properties
        public ICollection<OperatingSession> OperatingSessions { get; set; } = new List<OperatingSession>();
        public ICollection<FuelRefill> FuelRefills { get; set; } = new List<FuelRefill>();
        public ICollection<IoTDevice> IoTDevices { get; set; } = new List<IoTDevice>();

    }

    // ─── Generator Log ───
    public class GeneratorLog
    {
        public int Id { get; set; }
        public int GeneratorId { get; set; }
        public DateTime LogTime { get; set; } = DateTime.Now;
        public GeneratorLogType LogType { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentLoad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelLevel { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Temperature { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OilPressure { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Voltage { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public virtual Generator? Generator { get; set; }
    }

    // ─── Fuel Record ───
    public class FuelRecord
    {
        public int Id { get; set; }
        public int GeneratorId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerLiter { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; }

        [MaxLength(200)]
        public string? Supplier { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public string? AddedBy { get; set; }
        public virtual Generator? Generator { get; set; }
    }

    // ─── Enums ───
    public enum GeneratorStatus
    {
        [Display(Name = "يعمل")] Active = 1,
        [Display(Name = "متوقف")] Stopped = 2,
        [Display(Name = "صيانة")] Maintenance = 3,
        [Display(Name = "عطل")] Fault = 4,
        [Display(Name = "احتياط")] Standby = 5
    }

    public enum FuelType
    {
        [Display(Name = "ديزل")] Diesel = 1,
        [Display(Name = "بنزين")] Gasoline = 2,
        [Display(Name = "غاز")] Gas = 3,
        [Display(Name = "ثنائي الوقود")] Dual = 4
    }

    public enum GeneratorLogType
    {
        [Display(Name = "تشغيل عادي")] Normal = 1,
        [Display(Name = "تحذير")] Warning = 2,
        [Display(Name = "عطل")] Fault = 3,
        [Display(Name = "صيانة")] Maintenance = 4,
        [Display(Name = "إيقاف")] Shutdown = 5,
        [Display(Name = "IoT")] IoT = 6
    }
}