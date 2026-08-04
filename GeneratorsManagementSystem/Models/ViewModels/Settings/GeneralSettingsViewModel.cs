using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.ViewModels.Settings
{
    public class GeneralSettingsViewModel
    {
        // معلومات النظام
        [Display(Name = "اسم النظام")]
        [Required(ErrorMessage = "اسم النظام مطلوب")]
        [MaxLength(200)]
        public string SystemName { get; set; } = "نظام إدارة المولدات الكهربائية";

        [Display(Name = "الاسم المختصر")]
        [MaxLength(50)]
        public string SystemShortName { get; set; } = "GMS";

        [Display(Name = "إصدار النظام")]
        [MaxLength(20)]
        public string Version { get; set; } = "1.0.0";

        [Display(Name = "بريد الدعم الفني")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
        [MaxLength(100)]
        public string? SupportEmail { get; set; }

        [Display(Name = "هاتف الدعم الفني")]
        [MaxLength(20)]
        public string? SupportPhone { get; set; }

        // العملة والتنسيقات
        [Display(Name = "العملة الافتراضية")]
        [Required]
        public string Currency { get; set; } = "LYD";

        [Display(Name = "رمز العملة")]
        [MaxLength(10)]
        public string CurrencySymbol { get; set; } = "د.ل";

        [Display(Name = "موضع رمز العملة")]
        public string CurrencyPosition { get; set; } = "after"; // before, after

        [Display(Name = "عدد المنازل العشرية")]
        [Range(0, 4)]
        public int DecimalPlaces { get; set; } = 2;

        [Display(Name = "فاصل الآلاف")]
        public string ThousandSeparator { get; set; } = ",";

        [Display(Name = "الفاصل العشري")]
        public string DecimalSeparator { get; set; } = ".";

        // اللغة والمنطقة الزمنية
        [Display(Name = "اللغة الافتراضية")]
        public string DefaultLanguage { get; set; } = "ar";

        [Display(Name = "اتجاه النص")]
        public string TextDirection { get; set; } = "rtl";

        [Display(Name = "المنطقة الزمنية")]
        public string TimeZone { get; set; } = "Africa/Tripoli";

        [Display(Name = "صيغة التاريخ")]
        public string DateFormat { get; set; } = "yyyy/MM/dd";

        [Display(Name = "صيغة الوقت")]
        public string TimeFormat { get; set; } = "HH:mm";

        [Display(Name = "التقويم")]
        public string Calendar { get; set; } = "gregorian"; // gregorian, hijri

        // إعدادات الجلسة
        [Display(Name = "مدة الجلسة (بالدقائق)")]
        [Range(5, 1440)]
        public int SessionTimeout { get; set; } = 480; // 8 ساعات

        [Display(Name = "تسجيل خروج تلقائي عند عدم النشاط")]
        public bool AutoLogout { get; set; } = true;

        // إعدادات التسجيل
        [Display(Name = "السماح بالتسجيل الذاتي للمشتركين")]
        public bool AllowSelfRegistration { get; set; } = false;

        [Display(Name = "تفعيل الحسابات تلقائياً")]
        public bool AutoActivateAccounts { get; set; } = false;

        [Display(Name = "الحد الأدنى لطول كلمة المرور")]
        [Range(4, 20)]
        public int MinPasswordLength { get; set; } = 6;

        // إعدادات الصفحات
        [Display(Name = "عدد العناصر في الصفحة")]
        [Range(5, 100)]
        public int PageSize { get; set; } = 15;
    }
}