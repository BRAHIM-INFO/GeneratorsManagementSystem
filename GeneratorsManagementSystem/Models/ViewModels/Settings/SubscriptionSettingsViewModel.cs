using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class SubscriptionSettingsViewModel
    {
        public int Id { get; set; }

        // ترقيم المشتركين
        [Display(Name = "بادئة رقم المشترك")]
        [Required]
        [MaxLength(20)]
        public string SubscriberNumberPrefix { get; set; } = "SUB";

        [Display(Name = "طول الرقم")]
        [Range(1, 10)]
        public int SubscriberNumberLength { get; set; } = 4;

        [Display(Name = "تضمين السنة في الرقم")]
        public bool IncludeYearInNumber { get; set; } = true;

        // الرسوم الافتراضية
        [Display(Name = "رسوم التركيب الافتراضية")]
        [Range(0, 100000)]
        public decimal DefaultInstallationFee { get; set; } = 150;

        [Display(Name = "الرسوم الشهرية الافتراضية")]
        [Range(0, 100000)]
        public decimal DefaultMonthlyFee { get; set; } = 50;

        [Display(Name = "سعر الأمبير")]
        [Range(0, 10000)]
        public decimal DefaultPricePerAmpere { get; set; } = 25;

        [Display(Name = "سعر الكيلوواط ساعة")]
        [Range(0, 100)]
        public decimal DefaultPricePerKwh { get; set; } = 0.5m;

        [Display(Name = "نوع الاشتراك الافتراضي")]
        public string DefaultSubscriptionType { get; set; } = "Monthly";

        // الفوترة
        [Display(Name = "يوم الاستحقاق من الشهر")]
        [Range(1, 28)]
        public int BillingDayOfMonth { get; set; } = 1;

        [Display(Name = "فترة السماح (يوم)")]
        [Range(0, 60)]
        public int GracePeriodDays { get; set; } = 7;

        // التعليق التلقائي
        [Display(Name = "تعليق تلقائي عند التأخر")]
        public bool AutoSuspendOverdue { get; set; } = false;

        [Display(Name = "تعليق بعد (يوم)")]
        [Range(1, 365)]
        public int SuspendAfterDays { get; set; } = 15;

        // الغرامات
        [Display(Name = "تطبيق غرامات التأخير")]
        public bool ApplyLateFees { get; set; } = false;

        [Display(Name = "قيمة الغرامة الثابتة")]
        [Range(0, 10000)]
        public decimal LateFeeAmount { get; set; } = 10;

        [Display(Name = "نسبة الغرامة %")]
        [Range(0, 100)]
        public decimal LateFeePercentage { get; set; } = 0;

        // الخصومات
        [Display(Name = "السماح بخصم الدفع المبكر")]
        public bool AllowEarlyPaymentDiscount { get; set; } = false;

        [Display(Name = "نسبة خصم الدفع المبكر %")]
        [Range(0, 100)]
        public decimal EarlyPaymentDiscountPercentage { get; set; } = 5;

        [Display(Name = "أيام الدفع المبكر قبل الاستحقاق")]
        [Range(1, 30)]
        public int EarlyPaymentDaysBeforeDue { get; set; } = 10;

        // العقود
        [Display(Name = "مدة العقد الافتراضية (شهر)")]
        [Range(1, 120)]
        public int DefaultContractDurationMonths { get; set; } = 12;

        [Display(Name = "يتطلب دفعة تأمين")]
        public bool RequireDeposit { get; set; } = false;

        [Display(Name = "قيمة التأمين الافتراضية")]
        [Range(0, 100000)]
        public decimal DefaultDepositAmount { get; set; } = 100;

        // التنبيهات
        [Display(Name = "تفعيل تذكيرات الدفع")]
        public bool EnablePaymentReminders { get; set; } = true;

        [Display(Name = "تذكير قبل الاستحقاق (يوم)")]
        [Range(0, 30)]
        public int ReminderBeforeDueDays { get; set; } = 3;

        [Display(Name = "تذكير بعد الاستحقاق (يوم)")]
        [Range(0, 30)]
        public int ReminderAfterDueDays { get; set; } = 3;
    }
}