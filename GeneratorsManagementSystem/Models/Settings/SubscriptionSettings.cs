using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Settings
{
    public class SubscriptionSettings
    {
        [Key]
        public int Id { get; set; }

        // إعدادات ترقيم المشتركين
        [MaxLength(20)]
        public string SubscriberNumberPrefix { get; set; } = "SUB";

        public int SubscriberNumberLength { get; set; } = 4;
        public bool IncludeYearInNumber { get; set; } = true;

        // الرسوم الافتراضية
        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultInstallationFee { get; set; } = 150;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultMonthlyFee { get; set; } = 50;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultPricePerAmpere { get; set; } = 25;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultPricePerKwh { get; set; } = 0.5m;

        // نوع الاشتراك الافتراضي
        [MaxLength(20)]
        public string DefaultSubscriptionType { get; set; } = "Monthly"; // Monthly, Yearly, PayAsYouGo

        // دورة الفوترة
        public int BillingDayOfMonth { get; set; } = 1; // يوم الاستحقاق من الشهر
        public int GracePeriodDays { get; set; } = 7; // فترة السماح قبل التعليق

        // إعدادات التعليق التلقائي
        public bool AutoSuspendOverdue { get; set; } = false;
        public int SuspendAfterDays { get; set; } = 15; // تعليق بعد X يوم من التأخر

        // إعدادات الغرامات
        public bool ApplyLateFees { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal LateFeeAmount { get; set; } = 10;

        [Column(TypeName = "decimal(5,2)")]
        public decimal LateFeePercentage { get; set; } = 0; // نسبة من الفاتورة

        // إعدادات الخصومات
        public bool AllowEarlyPaymentDiscount { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        public decimal EarlyPaymentDiscountPercentage { get; set; } = 5;

        public int EarlyPaymentDaysBeforeDue { get; set; } = 10;

        // إعدادات العقود
        public int DefaultContractDurationMonths { get; set; } = 12;
        public bool RequireDeposit { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultDepositAmount { get; set; } = 100;

        // تنبيهات المشتركين
        public bool EnablePaymentReminders { get; set; } = true;
        public int ReminderBeforeDueDays { get; set; } = 3;
        public int ReminderAfterDueDays { get; set; } = 3;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(450)]
        public string? UpdatedBy { get; set; }
    }
}