using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    public enum PaymentMethod
    {
        [Display(Name = "نقدي")]
        Cash = 1,

        [Display(Name = "تحويل بنكي")]
        BankTransfer = 2,

        [Display(Name = "بطاقة")]
        Card = 3,

        [Display(Name = "محفظة إلكترونية")]
        EWallet = 4,

        [Display(Name = "شيك")]
        Cheque = 5,

        [Display(Name = "أخرى")]
        Other = 99
    }

    public class Payment
    {
        public int Id { get; set; }

        [Display(Name = "رقم الإيصال")]
        [StringLength(30)]
        public string ReceiptNumber { get; set; } = string.Empty;

        // ═══ العلاقات ═══

        [Required]
        [Display(Name = "الفاتورة")]
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        [Display(Name = "المشترك")]
        public int SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; } = null!;

        // ═══ بيانات الدفع ═══

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        [Display(Name = "المبلغ")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "طريقة الدفع")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "تاريخ الدفع")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [StringLength(50)]
        [Display(Name = "المرجع")]
        public string? Reference { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [StringLength(100)]
        [Display(Name = "المُستلم")]
        public string? ReceivedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        [NotMapped]
        public string PaymentMethodText => PaymentMethod switch
        {
            PaymentMethod.Cash => "نقدي",
            PaymentMethod.BankTransfer => "تحويل بنكي",
            PaymentMethod.Card => "بطاقة",
            PaymentMethod.EWallet => "محفظة إلكترونية",
            PaymentMethod.Cheque => "شيك",
            _ => "أخرى"
        };
    }
}