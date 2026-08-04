using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models.Accounting
{
    public enum ExpenseCategory
    {
        [Display(Name = "وقود")]
        Fuel = 1,

        [Display(Name = "صيانة")]
        Maintenance = 2,

        [Display(Name = "قطع غيار")]
        SpareParts = 3,

        [Display(Name = "رواتب")]
        Salaries = 4,

        [Display(Name = "إيجار")]
        Rent = 5,

        [Display(Name = "كهرباء رسمية")]
        Electricity = 6,

        [Display(Name = "زيوت وفلاتر")]
        OilsAndFilters = 7,

        [Display(Name = "معدات")]
        Equipment = 8,

        [Display(Name = "نقل ومواصلات")]
        Transportation = 9,

        [Display(Name = "اتصالات")]
        Communications = 10,

        [Display(Name = "مصاريف إدارية")]
        Administrative = 11,

        [Display(Name = "ضرائب ورسوم")]
        Taxes = 12,

        [Display(Name = "أخرى")]
        Other = 99
    }

    public class Expense
    {
        public int Id { get; set; }

        [Display(Name = "رقم المصروف")]
        [StringLength(30)]
        public string ExpenseNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "الفئة")]
        public ExpenseCategory Category { get; set; }

        [Required(ErrorMessage = "البيان مطلوب")]
        [StringLength(200)]
        [Display(Name = "البيان")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        [Display(Name = "المبلغ")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "تاريخ المصروف")]
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        [Display(Name = "المولد المرتبط")]
        public int? GeneratorId { get; set; }
        public Generator? Generator { get; set; }

        [Display(Name = "طريقة الدفع")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [StringLength(50)]
        [Display(Name = "المرجع (رقم فاتورة/شيك)")]
        public string? Reference { get; set; }

        [StringLength(100)]
        [Display(Name = "المستفيد / المورد")]
        public string? Beneficiary { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [StringLength(200)]
        [Display(Name = "المرفق")]
        public string? AttachmentPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // ═══ Computed ═══
        [NotMapped]
        public string CategoryText => Category switch
        {
            ExpenseCategory.Fuel => "وقود",
            ExpenseCategory.Maintenance => "صيانة",
            ExpenseCategory.SpareParts => "قطع غيار",
            ExpenseCategory.Salaries => "رواتب",
            ExpenseCategory.Rent => "إيجار",
            ExpenseCategory.Electricity => "كهرباء رسمية",
            ExpenseCategory.OilsAndFilters => "زيوت وفلاتر",
            ExpenseCategory.Equipment => "معدات",
            ExpenseCategory.Transportation => "نقل ومواصلات",
            ExpenseCategory.Communications => "اتصالات",
            ExpenseCategory.Administrative => "مصاريف إدارية",
            ExpenseCategory.Taxes => "ضرائب ورسوم",
            _ => "أخرى"
        };

        [NotMapped]
        public string CategoryIcon => Category switch
        {
            ExpenseCategory.Fuel => "fa-gas-pump",
            ExpenseCategory.Maintenance => "fa-wrench",
            ExpenseCategory.SpareParts => "fa-cogs",
            ExpenseCategory.Salaries => "fa-users",
            ExpenseCategory.Rent => "fa-building",
            ExpenseCategory.Electricity => "fa-bolt",
            ExpenseCategory.OilsAndFilters => "fa-oil-can",
            ExpenseCategory.Equipment => "fa-tools",
            ExpenseCategory.Transportation => "fa-truck",
            ExpenseCategory.Communications => "fa-phone",
            ExpenseCategory.Administrative => "fa-briefcase",
            ExpenseCategory.Taxes => "fa-file-invoice",
            _ => "fa-money-bill"
        };

        [NotMapped]
        public string CategoryColor => Category switch
        {
            ExpenseCategory.Fuel => "#E53E3E",
            ExpenseCategory.Maintenance => "#5A67D8",
            ExpenseCategory.SpareParts => "#805AD5",
            ExpenseCategory.Salaries => "#38A169",
            ExpenseCategory.Rent => "#D69E2E",
            ExpenseCategory.Electricity => "#3182CE",
            ExpenseCategory.OilsAndFilters => "#F6AD55",
            ExpenseCategory.Equipment => "#718096",
            ExpenseCategory.Transportation => "#4299E1",
            ExpenseCategory.Communications => "#48BB78",
            ExpenseCategory.Administrative => "#9F7AEA",
            ExpenseCategory.Taxes => "#ED8936",
            _ => "#4A5568"
        };

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