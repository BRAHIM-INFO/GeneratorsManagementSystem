using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class BillingSettingsViewModel
    {
        public int Id { get; set; }

        // ترقيم الفواتير
        [Display(Name = "بادئة رقم الفاتورة")]
        [Required]
        [MaxLength(20)]
        public string InvoiceNumberPrefix { get; set; } = "INV";

        [Display(Name = "طول الرقم")]
        [Range(1, 10)]
        public int InvoiceNumberLength { get; set; } = 6;

        [Display(Name = "تضمين السنة")]
        public bool IncludeYearInInvoice { get; set; } = true;

        [Display(Name = "تضمين الشهر")]
        public bool IncludeMonthInInvoice { get; set; } = false;

        [Display(Name = "إعادة ترقيم سنوياً")]
        public bool ResetInvoiceNumberYearly { get; set; } = true;

        // ترقيم الإيصالات
        [Display(Name = "بادئة رقم الإيصال")]
        [Required]
        [MaxLength(20)]
        public string ReceiptNumberPrefix { get; set; } = "REC";

        [Display(Name = "طول رقم الإيصال")]
        [Range(1, 10)]
        public int ReceiptNumberLength { get; set; } = 6;

        // الضرائب
        [Display(Name = "تفعيل الضريبة")]
        public bool EnableTax { get; set; } = false;

        [Display(Name = "اسم الضريبة")]
        [MaxLength(100)]
        public string TaxName { get; set; } = "ضريبة القيمة المضافة";

        [Display(Name = "نسبة الضريبة %")]
        [Range(0, 100)]
        public decimal TaxPercentage { get; set; } = 15;

        [Display(Name = "السعر شامل الضريبة")]
        public bool TaxIncludedInPrice { get; set; } = false;

        // الخصومات
        [Display(Name = "السماح بالخصومات")]
        public bool EnableDiscounts { get; set; } = true;

        [Display(Name = "السماح بخصم نسبة")]
        public bool AllowPercentageDiscount { get; set; } = true;

        [Display(Name = "السماح بخصم مبلغ ثابت")]
        public bool AllowFixedDiscount { get; set; } = true;

        [Display(Name = "الحد الأقصى للخصم %")]
        [Range(0, 100)]
        public decimal MaxDiscountPercentage { get; set; } = 50;

        // طرق الدفع
        [Display(Name = "الدفع النقدي")]
        public bool AllowCashPayment { get; set; } = true;

        [Display(Name = "التحويل البنكي")]
        public bool AllowBankTransfer { get; set; } = true;

        [Display(Name = "بطاقة الائتمان")]
        public bool AllowCreditCard { get; set; } = false;

        [Display(Name = "الشيكات")]
        public bool AllowCheque { get; set; } = false;

        [Display(Name = "الدفع الإلكتروني")]
        public bool AllowOnlinePayment { get; set; } = false;

        [Display(Name = "المحفظة الإلكترونية")]
        public bool AllowMobilePayment { get; set; } = false;

        // إعدادات الفاتورة
        [Display(Name = "رأس الفاتورة")]
        [MaxLength(500)]
        public string? InvoiceHeader { get; set; }

        [Display(Name = "تذييل الفاتورة")]
        [MaxLength(500)]
        public string? InvoiceFooter { get; set; } = "شكراً لاختياركم خدماتنا";

        [Display(Name = "شروط الدفع")]
        [MaxLength(1000)]
        public string? PaymentTerms { get; set; } = "الدفع خلال 30 يوماً من تاريخ الفاتورة";

        [Display(Name = "بيانات البنك")]
        [MaxLength(1000)]
        public string? BankDetails { get; set; }

        [Display(Name = "عرض الشعار في الفاتورة")]
        public bool ShowLogoOnInvoice { get; set; } = true;

        [Display(Name = "عرض التوقيع في الفاتورة")]
        public bool ShowSignatureOnInvoice { get; set; } = false;

        [Display(Name = "عرض الختم في الفاتورة")]
        public bool ShowStampOnInvoice { get; set; } = false;

        // العملة
        [Display(Name = "العملة")]
        public string Currency { get; set; } = "LYD";

        [Display(Name = "رمز العملة")]
        public string CurrencySymbol { get; set; } = "د.ل";

        // التقريب
        [Display(Name = "طريقة التقريب")]
        public string RoundingMethod { get; set; } = "Normal";

        [Display(Name = "عدد المنازل العشرية")]
        [Range(0, 4)]
        public int RoundingDecimals { get; set; } = 2;

        // البريد
        [Display(Name = "إرسال الفاتورة بالبريد تلقائياً")]
        public bool AutoSendInvoiceEmail { get; set; } = false;

        [Display(Name = "إرسال الإيصال بالبريد تلقائياً")]
        public bool AutoSendReceiptEmail { get; set; } = false;
    }
}