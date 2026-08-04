using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    public enum InvoiceStatus
    {
        [Display(Name = "غير مدفوعة")]
        Unpaid = 1,

        [Display(Name = "مدفوعة جزئياً")]
        PartiallyPaid = 2,

        [Display(Name = "مدفوعة")]
        Paid = 3,

        [Display(Name = "متأخرة")]
        Overdue = 4,

        [Display(Name = "ملغاة")]
        Cancelled = 5
    }

    public class Invoice
    {
        public int Id { get; set; }

        [Display(Name = "رقم الفاتورة")]
        [StringLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        // ═══ العلاقات ═══

        [Display(Name = "الاشتراك")]
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = null!;

        [Display(Name = "المشترك")]
        public int SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; } = null!;

        // ═══ الفترة ═══

        [Display(Name = "من تاريخ")]
        public DateTime PeriodStart { get; set; }

        [Display(Name = "إلى تاريخ")]
        public DateTime PeriodEnd { get; set; }

        [Display(Name = "تاريخ الإصدار")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Display(Name = "تاريخ الاستحقاق")]
        public DateTime DueDate { get; set; }

        // ═══ المبالغ ═══

        [Display(Name = "المبلغ")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "الخصم")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; } = 0;

        [Display(Name = "الضريبة")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Tax { get; set; } = 0;

        [Display(Name = "رسوم إضافية")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AdditionalCharges { get; set; } = 0;

        [Display(Name = "الحالة")]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ═══ بيانات النظام ═══

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ═══ العلاقات ═══

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // ═══ Computed ═══

        [NotMapped]
        public decimal TotalAmount => Amount - Discount + Tax + AdditionalCharges;

        [NotMapped]
        public decimal PaidAmount => Payments?.Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        [NotMapped]
        public bool IsPaid => Status == InvoiceStatus.Paid;

        [NotMapped]
        public bool IsOverdue =>
            (Status == InvoiceStatus.Unpaid || Status == InvoiceStatus.PartiallyPaid)
            && DueDate < DateTime.Today;

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            InvoiceStatus.Paid => "bg-success",
            InvoiceStatus.PartiallyPaid => "bg-info",
            InvoiceStatus.Unpaid => IsOverdue ? "bg-danger" : "bg-warning",
            InvoiceStatus.Overdue => "bg-danger",
            InvoiceStatus.Cancelled => "bg-secondary",
            _ => "bg-secondary"
        };

        [NotMapped]
        public string StatusText => Status switch
        {
            InvoiceStatus.Paid => "مدفوعة",
            InvoiceStatus.PartiallyPaid => "مدفوعة جزئياً",
            InvoiceStatus.Unpaid => IsOverdue ? "متأخرة" : "غير مدفوعة",
            InvoiceStatus.Overdue => "متأخرة",
            InvoiceStatus.Cancelled => "ملغاة",
            _ => "—"
        };
    }
}