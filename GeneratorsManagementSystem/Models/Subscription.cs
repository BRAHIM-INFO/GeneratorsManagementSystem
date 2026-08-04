using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    public enum SubscriptionType
    {
        [Display(Name = "شهري")]
        Monthly = 1,

        [Display(Name = "ربع سنوي")]
        Quarterly = 2,

        [Display(Name = "نصف سنوي")]
        SemiAnnual = 3,

        [Display(Name = "سنوي")]
        Annual = 4
    }

    public enum SubscriptionStatus
    {
        [Display(Name = "نشط")]
        Active = 1,

        [Display(Name = "متوقف مؤقتاً")]
        Suspended = 2,

        [Display(Name = "منتهي")]
        Expired = 3,

        [Display(Name = "ملغى")]
        Cancelled = 4
    }

    public class Subscription
    {
        public int Id { get; set; }

        [Display(Name = "رقم العقد")]
        [StringLength(30)]
        public string ContractNumber { get; set; } = string.Empty;

        // ═══ العلاقات ═══

        [Required]
        [Display(Name = "المشترك")]
        public int SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; } = null!;

        [Required]
        [Display(Name = "المولد")]
        public int GeneratorId { get; set; }
        public Generator Generator { get; set; } = null!;

        // ═══ نوع الجهاز (اختياري) ═══

        [Display(Name = "نوع الجهاز")]
        public int? DeviceTypeId { get; set; }
        public DeviceType? DeviceType { get; set; }

        [Display(Name = "عدد الأجهزة")]
        public int DeviceCount { get; set; } = 1;

        // ═══ بيانات الاشتراك ═══

        [Display(Name = "نوع الاشتراك")]
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Monthly;

        [Display(Name = "عدد الأمبير")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Ampere { get; set; }

        [Display(Name = "سعر الأمبير")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerAmpere { get; set; }

        [Display(Name = "رسوم ثابتة")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FixedFee { get; set; } = 0;

        [Display(Name = "رسوم التركيب")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal InstallationFee { get; set; } = 0;

        // ═══ 🆕 العمولة الإدارية ═══

        [Display(Name = "نسبة العمولة الإدارية (%)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal AdminCommissionPercentage { get; set; } = 0;

        [Display(Name = "قيمة العمولة")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AdminCommissionAmount { get; set; } = 0;

        // ═══ 🆕 الخصم ═══

        [Display(Name = "مبلغ الخصم")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "سبب الخصم")]
        public int? DiscountReasonId { get; set; }
        public DiscountReason? DiscountReason { get; set; }

        [StringLength(300)]
        [Display(Name = "تفاصيل الخصم")]
        public string? DiscountNotes { get; set; }

        // ═══ 🆕 الإعفاء الكامل ═══

        [Display(Name = "إعفاء كامل")]
        public bool IsFullExempt { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "سبب الإعفاء")]
        public string? ExemptReason { get; set; }

        [Display(Name = "تاريخ الإعفاء")]
        public DateTime? ExemptDate { get; set; }

        [StringLength(100)]
        [Display(Name = "أعفى بواسطة")]
        public string? ExemptBy { get; set; }

        // ═══ الحقل القديم (للتوافق) ═══

        [Display(Name = "الخصم الشهري (قديم)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MonthlyDiscount { get; set; } = 0;

        [Display(Name = "يوم الاستحقاق")]
        public int DueDay { get; set; } = 1;

        // ═══ الكابينة ═══

        [StringLength(30)]
        [Display(Name = "رقم الكابينة")]
        public string? CabinetNumber { get; set; }

        [StringLength(30)]
        [Display(Name = "رقم الدورة")]
        public string? CircuitNumber { get; set; }

        [StringLength(30)]
        [Display(Name = "رقم العداد")]
        public string? MeterNumber { get; set; }

        // ═══ الحالة والتواريخ ═══

        [Display(Name = "حالة الاشتراك")]
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        [Display(Name = "تاريخ البداية")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "تاريخ الانتهاء")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "تاريخ آخر فوترة")]
        public DateTime? LastBillingDate { get; set; }

        [Display(Name = "تاريخ الفوترة القادمة")]
        public DateTime? NextBillingDate { get; set; }

        [StringLength(500)]
        [Display(Name = "سبب الإيقاف")]
        public string? SuspensionReason { get; set; }

        [Display(Name = "تاريخ الإيقاف")]
        public DateTime? SuspensionDate { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ═══ بيانات النظام ═══

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // ═══ العلاقات ═══

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        // ═══ Computed Properties ═══

        [NotMapped]
        public decimal BaseAmount => Ampere * PricePerAmpere;

        [NotMapped]
        public decimal DeviceBasedAmount => DeviceCount * (DeviceType?.DefaultPrice ?? 0);

        [NotMapped]
        public decimal MonthlyAmount
        {
            get
            {
                if (IsFullExempt) return 0;

                var baseAmt = BaseAmount + FixedFee;
                var commission = baseAmt * (AdminCommissionPercentage / 100m);
                var total = baseAmt + commission - DiscountAmount;

                return Math.Max(0, total);
            }
        }

        [NotMapped]
        public decimal CommissionValue =>
            BaseAmount * (AdminCommissionPercentage / 100m);

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            SubscriptionStatus.Active => "bg-success",
            SubscriptionStatus.Suspended => "bg-warning",
            SubscriptionStatus.Expired => "bg-secondary",
            SubscriptionStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };

        [NotMapped]
        public string StatusText => Status switch
        {
            SubscriptionStatus.Active => "نشط",
            SubscriptionStatus.Suspended => "متوقف",
            SubscriptionStatus.Expired => "منتهي",
            SubscriptionStatus.Cancelled => "ملغى",
            _ => "—"
        };

        [NotMapped]
        public string SubscriptionTypeText => SubscriptionType switch
        {
            SubscriptionType.Monthly => "شهري",
            SubscriptionType.Quarterly => "ربع سنوي",
            SubscriptionType.SemiAnnual => "نصف سنوي",
            SubscriptionType.Annual => "سنوي",
            _ => "—"
        };

        [NotMapped]
        public int MonthsInPeriod => SubscriptionType switch
        {
            SubscriptionType.Monthly => 1,
            SubscriptionType.Quarterly => 3,
            SubscriptionType.SemiAnnual => 6,
            SubscriptionType.Annual => 12,
            _ => 1
        };
    }
}