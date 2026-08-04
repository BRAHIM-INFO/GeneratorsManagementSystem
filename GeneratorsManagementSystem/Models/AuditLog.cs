using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    // ══════════════════════════════════════
    //  أنواع العمليات
    // ══════════════════════════════════════
    public enum AuditActionType
    {
        [Display(Name = "إنشاء")]
        Create = 1,

        [Display(Name = "تعديل")]
        Update = 2,

        [Display(Name = "حذف")]
        Delete = 3,

        [Display(Name = "عرض")]
        View = 4,

        [Display(Name = "تسجيل دخول")]
        Login = 5,

        [Display(Name = "تسجيل خروج")]
        Logout = 6,

        [Display(Name = "فشل دخول")]
        LoginFailed = 7,

        [Display(Name = "تغيير كلمة مرور")]
        ChangePassword = 8,

        [Display(Name = "تفعيل/تعطيل")]
        ToggleStatus = 9,

        [Display(Name = "طباعة")]
        Print = 10,

        [Display(Name = "تصدير")]
        Export = 11,

        [Display(Name = "استيراد")]
        Import = 12,

        [Display(Name = "دفعة")]
        Payment = 13,

        [Display(Name = "إلغاء")]
        Cancel = 14,

        [Display(Name = "أخرى")]
        Other = 99
    }

    // ══════════════════════════════════════
    //  الوحدات
    // ══════════════════════════════════════
    public enum AuditModule
    {
        [Display(Name = "النظام")]
        System = 0,

        [Display(Name = "المستخدمون")]
        Users = 1,

        [Display(Name = "المولدات")]
        Generators = 2,

        [Display(Name = "المشتركون")]
        Subscribers = 3,

        [Display(Name = "الاشتراكات")]
        Subscriptions = 4,

        [Display(Name = "الفواتير")]
        Invoices = 5,

        [Display(Name = "المدفوعات")]
        Payments = 6,

        [Display(Name = "الوقود")]
        Fuel = 7,

        [Display(Name = "الصيانة")]
        Maintenance = 8,

        [Display(Name = "المحاسبة")]
        Accounting = 9,

        [Display(Name = "الإعدادات")]
        Settings = 10,

        [Display(Name = "التقارير")]
        Reports = 11
    }

    public class AuditLog
    {
        public int Id { get; set; }

        // ═══ المستخدم ═══
        [StringLength(450)]
        [Display(Name = "معرف المستخدم")]
        public string? UserId { get; set; }

        [StringLength(100)]
        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; } = "System";

        [StringLength(100)]
        [Display(Name = "الاسم الكامل")]
        public string? UserFullName { get; set; }

        // ═══ العملية ═══
        [Required]
        [Display(Name = "نوع العملية")]
        public AuditActionType ActionType { get; set; }

        [Required]
        [Display(Name = "الوحدة")]
        public AuditModule Module { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "الوصف")]
        public string Description { get; set; } = string.Empty;

        // ═══ الكيان المتأثر ═══
        [StringLength(100)]
        [Display(Name = "نوع الكيان")]
        public string? EntityType { get; set; }

        [Display(Name = "معرف الكيان")]
        public int? EntityId { get; set; }

        [StringLength(200)]
        [Display(Name = "اسم/رقم الكيان")]
        public string? EntityName { get; set; }

        // ═══ البيانات ═══
        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "البيانات القديمة")]
        public string? OldValues { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "البيانات الجديدة")]
        public string? NewValues { get; set; }

        [StringLength(500)]
        [Display(Name = "التغييرات")]
        public string? Changes { get; set; }

        // ═══ معلومات إضافية ═══
        [StringLength(50)]
        [Display(Name = "عنوان IP")]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        [Display(Name = "متصفح")]
        public string? UserAgent { get; set; }

        [StringLength(300)]
        [Display(Name = "الرابط")]
        public string? Url { get; set; }

        [StringLength(10)]
        [Display(Name = "طريقة الطلب")]
        public string? HttpMethod { get; set; }

        // ═══ الحالة ═══
        [Display(Name = "نجحت")]
        public bool IsSuccess { get; set; } = true;

        [StringLength(1000)]
        [Display(Name = "رسالة الخطأ")]
        public string? ErrorMessage { get; set; }

        // ═══ التاريخ ═══
        [Display(Name = "التاريخ والوقت")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // ═══ Computed Properties ═══

        [NotMapped]
        public string ActionTypeText => ActionType switch
        {
            AuditActionType.Create => "إنشاء",
            AuditActionType.Update => "تعديل",
            AuditActionType.Delete => "حذف",
            AuditActionType.View => "عرض",
            AuditActionType.Login => "تسجيل دخول",
            AuditActionType.Logout => "تسجيل خروج",
            AuditActionType.LoginFailed => "فشل دخول",
            AuditActionType.ChangePassword => "تغيير كلمة مرور",
            AuditActionType.ToggleStatus => "تفعيل/تعطيل",
            AuditActionType.Print => "طباعة",
            AuditActionType.Export => "تصدير",
            AuditActionType.Import => "استيراد",
            AuditActionType.Payment => "دفعة",
            AuditActionType.Cancel => "إلغاء",
            _ => "أخرى"
        };

        [NotMapped]
        public string ModuleText => Module switch
        {
            AuditModule.System => "النظام",
            AuditModule.Users => "المستخدمون",
            AuditModule.Generators => "المولدات",
            AuditModule.Subscribers => "المشتركون",
            AuditModule.Subscriptions => "الاشتراكات",
            AuditModule.Invoices => "الفواتير",
            AuditModule.Payments => "المدفوعات",
            AuditModule.Fuel => "الوقود",
            AuditModule.Maintenance => "الصيانة",
            AuditModule.Accounting => "المحاسبة",
            AuditModule.Settings => "الإعدادات",
            AuditModule.Reports => "التقارير",
            _ => "غير محدد"
        };

        [NotMapped]
        public string ActionColor => ActionType switch
        {
            AuditActionType.Create => "success",
            AuditActionType.Update => "info",
            AuditActionType.Delete => "danger",
            AuditActionType.Login => "primary",
            AuditActionType.LoginFailed => "danger",
            AuditActionType.Logout => "secondary",
            AuditActionType.Payment => "success",
            AuditActionType.Cancel => "warning",
            _ => "secondary"
        };

        [NotMapped]
        public string ActionIcon => ActionType switch
        {
            AuditActionType.Create => "fa-plus-circle",
            AuditActionType.Update => "fa-edit",
            AuditActionType.Delete => "fa-trash-alt",
            AuditActionType.View => "fa-eye",
            AuditActionType.Login => "fa-sign-in-alt",
            AuditActionType.Logout => "fa-sign-out-alt",
            AuditActionType.LoginFailed => "fa-times-circle",
            AuditActionType.ChangePassword => "fa-key",
            AuditActionType.ToggleStatus => "fa-toggle-on",
            AuditActionType.Print => "fa-print",
            AuditActionType.Export => "fa-file-export",
            AuditActionType.Import => "fa-file-import",
            AuditActionType.Payment => "fa-money-bill-wave",
            AuditActionType.Cancel => "fa-ban",
            _ => "fa-info-circle"
        };

        [NotMapped]
        public string ModuleIcon => Module switch
        {
            AuditModule.System => "fa-server",
            AuditModule.Users => "fa-users-cog",
            AuditModule.Generators => "fa-bolt",
            AuditModule.Subscribers => "fa-users",
            AuditModule.Subscriptions => "fa-file-contract",
            AuditModule.Invoices => "fa-file-invoice-dollar",
            AuditModule.Payments => "fa-money-check-alt",
            AuditModule.Fuel => "fa-gas-pump",
            AuditModule.Maintenance => "fa-wrench",
            AuditModule.Accounting => "fa-calculator",
            AuditModule.Settings => "fa-cogs",
            AuditModule.Reports => "fa-chart-bar",
            _ => "fa-folder"
        };

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var diff = DateTime.Now - Timestamp;
                if (diff.TotalMinutes < 1) return "الآن";
                if (diff.TotalMinutes < 60) return $"منذ {(int)diff.TotalMinutes} دقيقة";
                if (diff.TotalHours < 24) return $"منذ {(int)diff.TotalHours} ساعة";
                if (diff.TotalDays < 30) return $"منذ {(int)diff.TotalDays} يوم";
                return Timestamp.ToString("yyyy/MM/dd HH:mm");
            }
        }
    }
}