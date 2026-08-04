using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Settings
{
    public class BillingSettings
    {
        [Key]
        public int Id { get; set; }

        // إعدادات ترقيم الفواتير
        [MaxLength(20)]
        public string InvoiceNumberPrefix { get; set; } = "INV";

        public int InvoiceNumberLength { get; set; } = 6;
        public bool IncludeYearInInvoice { get; set; } = true;
        public bool IncludeMonthInInvoice { get; set; } = false;
        public bool ResetInvoiceNumberYearly { get; set; } = true;

        // إعدادات ترقيم إيصالات الدفع
        [MaxLength(20)]
        public string ReceiptNumberPrefix { get; set; } = "REC";

        public int ReceiptNumberLength { get; set; } = 6;

        // الضرائب
        public bool EnableTax { get; set; } = false;

        [MaxLength(100)]
        public string TaxName { get; set; } = "ضريبة القيمة المضافة";

        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxPercentage { get; set; } = 15;

        public bool TaxIncludedInPrice { get; set; } = false; // شامل الضريبة أم مضاف عليها

        // الخصومات
        public bool EnableDiscounts { get; set; } = true;
        public bool AllowPercentageDiscount { get; set; } = true;
        public bool AllowFixedDiscount { get; set; } = true;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxDiscountPercentage { get; set; } = 50;

        // طرق الدفع المسموحة
        public bool AllowCashPayment { get; set; } = true;
        public bool AllowBankTransfer { get; set; } = true;
        public bool AllowCreditCard { get; set; } = false;
        public bool AllowCheque { get; set; } = false;
        public bool AllowOnlinePayment { get; set; } = false;
        public bool AllowMobilePayment { get; set; } = false;

        // إعدادات الفاتورة
        [MaxLength(500)]
        public string? InvoiceHeader { get; set; }

        [MaxLength(500)]
        public string? InvoiceFooter { get; set; } = "شكراً لاختياركم خدماتنا";

        [MaxLength(1000)]
        public string? PaymentTerms { get; set; } = "الدفع خلال 30 يوماً من تاريخ الفاتورة";

        [MaxLength(1000)]
        public string? BankDetails { get; set; }

        public bool ShowLogoOnInvoice { get; set; } = true;
        public bool ShowSignatureOnInvoice { get; set; } = false;
        public bool ShowStampOnInvoice { get; set; } = false;

        // العملة
        [MaxLength(10)]
        public string Currency { get; set; } = "LYD";

        [MaxLength(10)]
        public string CurrencySymbol { get; set; } = "د.ل";

        // إعدادات التقريب
        public string RoundingMethod { get; set; } = "Normal"; // Normal, Up, Down
        public int RoundingDecimals { get; set; } = 2;

        // التذكيرات
        public bool AutoSendInvoiceEmail { get; set; } = false;
        public bool AutoSendReceiptEmail { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(450)]
        public string? UpdatedBy { get; set; }
    }
}