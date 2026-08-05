using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models
{
    public enum GeneratorOperatingMode
    {
        [Display(Name = "يدوي")]
        Manual = 1,

        [Display(Name = "تلقائي (IoT)")]
        Automatic = 2
    }
}