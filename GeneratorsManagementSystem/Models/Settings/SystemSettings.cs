using System.ComponentModel.DataAnnotations;

namespace GeneratorsManagementSystem.Models.Settings
{
    public class SystemSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? SettingValue { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string DataType { get; set; } = "string"; // string, int, decimal, bool, json

        public bool IsSystem { get; set; } = false; // إعدادات النظام الأساسية لا يمكن حذفها

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(450)]
        public string? UpdatedBy { get; set; }
    }
}