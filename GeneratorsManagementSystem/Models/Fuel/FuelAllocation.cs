using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Fuel
{
    // ══════════════════════════════════════
    //  نوع الوقود
    // ══════════════════════════════════════
    public enum FuelKind
    {
        [Display(Name = "ديزل")]
        Diesel = 1,

        [Display(Name = "بنزين")]
        Gasoline = 2,

        [Display(Name = "غاز")]
        Gas = 3,

        [Display(Name = "زيت أسود")]
        HeavyOil = 4
    }

    // ══════════════════════════════════════
    //  مصدر الوقود
    // ══════════════════════════════════════
    public enum FuelSource
    {
        [Display(Name = "حصة الدولة")]
        Government = 1,

        [Display(Name = "شراء من السوق")]
        MarketPurchase = 2,

        [Display(Name = "تبرع")]
        Donation = 3,

        [Display(Name = "أخرى")]
        Other = 99
    }

    // ══════════════════════════════════════
    //  حصة الوقود
    // ══════════════════════════════════════
    public class FuelAllocation
    {
        public int Id { get; set; }

        [Display(Name = "رقم الحصة")]
        [StringLength(30)]
        public string AllocationNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "نوع الوقود")]
        public FuelKind FuelKind { get; set; } = FuelKind.Diesel;

        [Required]
        [Display(Name = "مصدر الوقود")]
        public FuelSource Source { get; set; } = FuelSource.Government;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        [Display(Name = "الكمية (لتر)")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Quantity { get; set; }

        [Display(Name = "سعر اللتر")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerLiter { get; set; } = 0;

        [Display(Name = "التكلفة الإجمالية")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalCost { get; set; } = 0;

        [Required]
        [Display(Name = "تاريخ الاستلام")]
        public DateTime AllocationDate { get; set; } = DateTime.Today;

        [Display(Name = "الشهر المستحق عنه")]
        public int? AllocationMonth { get; set; }

        [Display(Name = "السنة المستحقة عنها")]
        public int? AllocationYear { get; set; }

        [StringLength(100)]
        [Display(Name = "المورد / الجهة")]
        public string? Supplier { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم الإذن / المرجع")]
        public string? ReferenceNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "المستلم")]
        public string? ReceivedBy { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [StringLength(200)]
        [Display(Name = "المرفق")]
        public string? AttachmentPath { get; set; }

        // ═══ بيانات النظام ═══
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // ═══ العلاقات ═══
        public ICollection<FuelConsumption> Consumptions { get; set; } = new List<FuelConsumption>();

        // ═══ Computed Properties ═══
        [NotMapped]
        public decimal ConsumedQuantity =>
            Consumptions?.Sum(c => c.Quantity) ?? 0;

        [NotMapped]
        public decimal RemainingQuantity => Quantity - ConsumedQuantity;

        [NotMapped]
        public decimal ConsumptionPercentage =>
            Quantity > 0 ? (ConsumedQuantity / Quantity * 100) : 0;

        [NotMapped]
        public bool IsFullyConsumed => RemainingQuantity <= 0;

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
        public string SourceText => Source switch
        {
            FuelSource.Government => "حصة الدولة",
            FuelSource.MarketPurchase => "شراء من السوق",
            FuelSource.Donation => "تبرع",
            _ => "أخرى"
        };

        [NotMapped]
        public string SourceBadgeClass => Source switch
        {
            FuelSource.Government => "bg-success",
            FuelSource.MarketPurchase => "bg-primary",
            FuelSource.Donation => "bg-info",
            _ => "bg-secondary"
        };

        [NotMapped]
        public string FuelKindIcon => FuelKind switch
        {
            FuelKind.Diesel => "fa-oil-can",
            FuelKind.Gasoline => "fa-gas-pump",
            FuelKind.Gas => "fa-fire",
            FuelKind.HeavyOil => "fa-tint",
            _ => "fa-gas-pump"
        };

        [NotMapped]
        public string FuelKindColor => FuelKind switch
        {
            FuelKind.Diesel => "#E53E3E",
            FuelKind.Gasoline => "#F6AD55",
            FuelKind.Gas => "#48BB78",
            FuelKind.HeavyOil => "#2D3748",
            _ => "#718096"
        };

        [NotMapped]
        public string AllocationPeriod
        {
            get
            {
                if (AllocationMonth.HasValue && AllocationYear.HasValue)
                {
                    var monthNames = new[] { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                                              "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
                    return $"{monthNames[AllocationMonth.Value]} {AllocationYear.Value}";
                }
                return "—";
            }
        }

        // ═══════════════════════════════════════════════════
        //  🆕 حقول التجهيز التجاري
        // ═══════════════════════════════════════════════════

        [Display(Name = "طريقة الدفع")]
        public FuelPaymentType PaymentType { get; set; } = FuelPaymentType.Cash;

        [Display(Name = "الكمية المدخلة (لتر)")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? EnteredQuantity { get; set; }

        [Display(Name = "الكمية الفعلية (لتر)")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? ActualQuantity { get; set; }

        [Display(Name = "المتبقي من حصة الشهر (لتر)")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? MonthlyRemaining { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم كتاب التجهيز")]
        public string? AllocationBookNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "الجهة الرسمية/الفرقة")]
        public string? OfficialAuthority { get; set; }

        [Display(Name = "تجاري")]
        public bool IsCommercial { get; set; } = false;

        [Display(Name = "معرف المصروف المُنشأ")]
        public int? GeneratedExpenseId { get; set; }

        [Display(Name = "المولد المرتبط")]
        public int? GeneratorId { get; set; }
        public Generator? Generator { get; set; }

        // Computed Properties
        [NotMapped]
        public string PaymentTypeText => PaymentType switch
        {
            FuelPaymentType.Cash => "نقداً",
            FuelPaymentType.Credit => "آجل",
            FuelPaymentType.Free => "مجاناً (حصة)",
            _ => "غير محدد"
        };

        [NotMapped]
        public string PaymentTypeBadgeClass => PaymentType switch
        {
            FuelPaymentType.Cash => "bg-success",
            FuelPaymentType.Credit => "bg-warning",
            FuelPaymentType.Free => "bg-info",
            _ => "bg-secondary"
        };

        [NotMapped]
        public string AllocationTypeText => IsCommercial ? "تجهيز تجاري" : "حصة شهرية";

        [NotMapped]
        public string AllocationTypeBadgeClass => IsCommercial ? "bg-warning" : "bg-success";
    }
}