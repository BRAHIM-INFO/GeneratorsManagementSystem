using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.Fuel
{
    public enum FuelPaymentType
    {
        [Display(Name = "نقداً")]
        Cash = 1,

        [Display(Name = "آجل")]
        Credit = 2,

        [Display(Name = "مجاناً (حصة)")]
        Free = 3
    }
}