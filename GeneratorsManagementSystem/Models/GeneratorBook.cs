using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    // ══════════════════════════════════════
    //  تصنيف الكتاب
    // ══════════════════════════════════════
    public enum BookCategory
    {
        [Display(Name = "رخصة")]
        License = 1,

        [Display(Name = "شهادة")]
        Certificate = 2,

        [Display(Name = "عقد")]
        Contract = 3,

        [Display(Name = "ترخيص")]
        Permit = 4,

        [Display(Name = "تأمين")]
        Insurance = 5,

        [Display(Name = "فاتورة رسمية")]
        OfficialInvoice = 6,

        [Display(Name = "مراسلة")]
        Correspondence = 7,

        [Display(Name = "قرار")]
        Decision = 8,

        [Display(Name = "أخرى")]
        Other = 99
    }

    // ══════════════════════════════════════
    //  حالة الكتاب
    // ══════════════════════════════════════
    public enum BookStatus
    {
        [Display(Name = "نشط")]
        Active = 1,

        [Display(Name = "قريب الانتهاء")]
        ExpiringSoon = 2,

        [Display(Name = "منتهي")]
        Expired = 3,

        [Display(Name = "بدون تاريخ انتهاء")]
        NoExpiry = 4,

        [Display(Name = "مؤرشف")]
        Archived = 5
    }

    // ══════════════════════════════════════
    //  كتاب المولدة
    // ══════════════════════════════════════
    public class GeneratorBook
    {
        public int Id { get; set; }

        // ═══ الأرقام ═══

        [Display(Name = "الرقم الداخلي")]
        [StringLength(30)]
        public string InternalNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الكتاب مطلوب")]
        [StringLength(200)]
        [Display(Name = "اسم الكتاب")]
        public string BookName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الجهة المصدرة مطلوبة")]
        [StringLength(200)]
        [Display(Name = "الجهة المصدرة")]
        public string IssuingAuthority { get; set; } = string.Empty;

        [Required(ErrorMessage = "العدد/الرقم مطلوب")]
        [StringLength(50)]
        [Display(Name = "العدد (رقم الكتاب الرسمي)")]
        public string BookNumber { get; set; } = string.Empty;

        [Display(Name = "التصنيف")]
        public BookCategory Category { get; set; } = BookCategory.Other;

        // ═══ التواريخ ═══

        [Required(ErrorMessage = "تاريخ الكتاب مطلوب")]
        [Display(Name = "تاريخ الكتاب")]
        public DateTime BookDate { get; set; } = DateTime.Today;

        [Display(Name = "له تاريخ انتهاء")]
        public bool HasExpiry { get; set; } = false;

        [Display(Name = "تاريخ الانتهاء")]
        public DateTime? ExpiryDate { get; set; }

        // ═══ المبلغ المالي ═══

        [Display(Name = "المبلغ المالي")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? Amount { get; set; }

        [Display(Name = "معرف المصروف المرتبط")]
        public int? ExpenseId { get; set; }

        // ═══ الملاحظات والمرفق ═══

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [StringLength(300)]
        [Display(Name = "مسار المرفق")]
        public string? AttachmentPath { get; set; }

        [StringLength(200)]
        [Display(Name = "اسم الملف الأصلي")]
        public string? AttachmentName { get; set; }

        [Display(Name = "حجم الملف (بايت)")]
        public long? AttachmentSize { get; set; }

        [StringLength(50)]
        [Display(Name = "نوع الملف")]
        public string? AttachmentType { get; set; }

        // ═══ التجديد ═══

        [Display(Name = "تم تجديده")]
        public bool IsRenewed { get; set; } = false;

        [Display(Name = "الكتاب القديم")]
        public int? RenewedFromBookId { get; set; }
        public GeneratorBook? RenewedFromBook { get; set; }

        // ═══ الحالة ═══

        [Display(Name = "مؤرشف")]
        public bool IsArchived { get; set; } = false;

        // ═══ بيانات النظام ═══

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // ═══ Computed Properties ═══

        [NotMapped]
        public int? DaysUntilExpiry
        {
            get
            {
                if (!HasExpiry || !ExpiryDate.HasValue) return null;
                return (ExpiryDate.Value.Date - DateTime.Today).Days;
            }
        }

        [NotMapped]
        public BookStatus Status
        {
            get
            {
                if (IsArchived) return BookStatus.Archived;
                if (!HasExpiry || !ExpiryDate.HasValue) return BookStatus.NoExpiry;

                var days = DaysUntilExpiry ?? 0;
                if (days < 0) return BookStatus.Expired;
                if (days <= 30) return BookStatus.ExpiringSoon;
                return BookStatus.Active;
            }
        }

        [NotMapped]
        public string StatusText => Status switch
        {
            BookStatus.Active => "نشط",
            BookStatus.ExpiringSoon => "قريب الانتهاء",
            BookStatus.Expired => "منتهي",
            BookStatus.NoExpiry => "بدون تاريخ انتهاء",
            BookStatus.Archived => "مؤرشف",
            _ => "—"
        };

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            BookStatus.Active => "bg-success",
            BookStatus.ExpiringSoon => GetExpiringBadge(),
            BookStatus.Expired => "bg-danger",
            BookStatus.NoExpiry => "bg-info",
            BookStatus.Archived => "bg-secondary",
            _ => "bg-light"
        };

        private string GetExpiringBadge()
        {
            var days = DaysUntilExpiry ?? 0;
            if (days <= 3) return "bg-danger";
            if (days <= 7) return "bg-warning";
            return "bg-warning";
        }

        [NotMapped]
        public string StatusColor => Status switch
        {
            BookStatus.Active => "#48BB78",
            BookStatus.ExpiringSoon => GetExpiringColor(),
            BookStatus.Expired => "#E53E3E",
            BookStatus.NoExpiry => "#4299E1",
            BookStatus.Archived => "#718096",
            _ => "#A0AEC0"
        };

        private string GetExpiringColor()
        {
            var days = DaysUntilExpiry ?? 0;
            if (days <= 3) return "#E53E3E";
            if (days <= 7) return "#F6AD55";
            return "#ECC94B";
        }

        [NotMapped]
        public string StatusIcon => Status switch
        {
            BookStatus.Active => "fa-check-circle",
            BookStatus.ExpiringSoon => "fa-clock",
            BookStatus.Expired => "fa-times-circle",
            BookStatus.NoExpiry => "fa-infinity",
            BookStatus.Archived => "fa-archive",
            _ => "fa-file"
        };

        [NotMapped]
        public string CategoryText => Category switch
        {
            BookCategory.License => "رخصة",
            BookCategory.Certificate => "شهادة",
            BookCategory.Contract => "عقد",
            BookCategory.Permit => "ترخيص",
            BookCategory.Insurance => "تأمين",
            BookCategory.OfficialInvoice => "فاتورة رسمية",
            BookCategory.Correspondence => "مراسلة",
            BookCategory.Decision => "قرار",
            _ => "أخرى"
        };

        [NotMapped]
        public string CategoryIcon => Category switch
        {
            BookCategory.License => "fa-id-card",
            BookCategory.Certificate => "fa-certificate",
            BookCategory.Contract => "fa-file-signature",
            BookCategory.Permit => "fa-stamp",
            BookCategory.Insurance => "fa-shield-alt",
            BookCategory.OfficialInvoice => "fa-file-invoice",
            BookCategory.Correspondence => "fa-envelope",
            BookCategory.Decision => "fa-gavel",
            _ => "fa-file-alt"
        };

        [NotMapped]
        public string CategoryColor => Category switch
        {
            BookCategory.License => "#5A67D8",
            BookCategory.Certificate => "#48BB78",
            BookCategory.Contract => "#805AD5",
            BookCategory.Permit => "#ED8936",
            BookCategory.Insurance => "#4299E1",
            BookCategory.OfficialInvoice => "#38A169",
            BookCategory.Correspondence => "#D69E2E",
            BookCategory.Decision => "#C53030",
            _ => "#718096"
        };

        [NotMapped]
        public string ExpiryMessage
        {
            get
            {
                if (!HasExpiry || !ExpiryDate.HasValue) return "بدون تاريخ انتهاء";
                var days = DaysUntilExpiry ?? 0;
                if (days < 0) return $"منتهي منذ {Math.Abs(days)} يوم";
                if (days == 0) return "ينتهي اليوم";
                if (days == 1) return "ينتهي غداً";
                return $"باقي {days} يوم";
            }
        }

        [NotMapped]
        public bool HasAttachment => !string.IsNullOrEmpty(AttachmentPath);

        [NotMapped]
        public string AttachmentSizeText
        {
            get
            {
                if (!AttachmentSize.HasValue) return "—";
                var size = AttachmentSize.Value;
                if (size < 1024) return $"{size} B";
                if (size < 1024 * 1024) return $"{(size / 1024.0):F1} KB";
                return $"{(size / (1024.0 * 1024.0)):F2} MB";
            }
        }

        [NotMapped]
        public string AttachmentIcon
        {
            get
            {
                if (string.IsNullOrEmpty(AttachmentType)) return "fa-file";
                if (AttachmentType.Contains("pdf")) return "fa-file-pdf";
                if (AttachmentType.Contains("image")) return "fa-file-image";
                return "fa-file";
            }
        }
    }
}