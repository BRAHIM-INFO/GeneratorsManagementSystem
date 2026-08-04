using GeneratorsManagementSystem.Models.Geography;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratorsManagementSystem.Models
{
    public class Subscriber
    {
        public int Id { get; set; }

        [Display(Name = "رقم المشترك")]
        [StringLength(20)]
        public string SubscriberNumber { get; set; } = string.Empty;

        // ═══ البيانات الشخصية ═══

        [Required(ErrorMessage = "اسم المشترك مطلوب")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "اسم المشترك")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "رقم الهوية")]
        public string? IdNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الهاتف")]
        public string? Phone { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الهاتف 2")]
        public string? Phone2 { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(100)]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        // ═══ العنوان الجديد (Cascade) ═══

        [Display(Name = "المحافظة")]
        public int? GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }

        [Display(Name = "القضاء")]
        public int? DistrictId { get; set; }
        public District? District { get; set; }

        [Display(Name = "الحي")]
        public int? NeighborhoodId { get; set; }
        public Neighborhood? Neighborhood { get; set; }

        [Display(Name = "الزقاق")]
        public int? AlleyId { get; set; }
        public Alley? Alley { get; set; }

        [StringLength(100)]
        [Display(Name = "الشارع")]
        public string? Street { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم البناية")]
        public string? BuildingNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "أقرب نقطة دالة")]
        public string? NearestLandmark { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات العنوان")]
        public string? AddressNotes { get; set; }

        // ═══ الحقول القديمة (نحتفظ بها للتوافق) ═══

        [StringLength(100)]
        [Display(Name = "المنطقة (قديم)")]
        public string? Area { get; set; }

        [StringLength(20)]
        [Display(Name = "الطابق")]
        public string? Floor { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الشقة")]
        public string? ApartmentNumber { get; set; }

        // ═══ ملاحظات عامة ═══

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ═══ بيانات التطبيق (Mobile App) - لاحقاً ═══

        [StringLength(200)]
        [Display(Name = "رمز التفعيل OTP")]
        public string? MobileOtp { get; set; }

        [Display(Name = "تاريخ التفعيل")]
        public DateTime? MobileActivatedAt { get; set; }

        [Display(Name = "التطبيق مربوط")]
        public bool IsMobileLinked { get; set; } = false;

        [StringLength(100)]
        [Display(Name = "معرف الجهاز")]
        public string? DeviceId { get; set; }

        [StringLength(50)]
        [Display(Name = "نظام التشغيل")]
        public string? MobileOS { get; set; }

        [StringLength(20)]
        [Display(Name = "إصدار التطبيق")]
        public string? AppVersion { get; set; }

        [Display(Name = "آخر اتصال")]
        public DateTime? LastMobileContact { get; set; }

        [Display(Name = "الإشعارات مفعّلة")]
        public bool NotificationsEnabled { get; set; } = true;

        // ═══ الحالة ═══

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        // ═══ بيانات النظام ═══

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // ═══ العلاقات ═══

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        // ═══ Computed Properties ═══

        [NotMapped]
        public string FullAddress
        {
            get
            {
                var parts = new List<string>();
                if (Governorate != null) parts.Add(Governorate.Name);
                if (District != null) parts.Add(District.Name);
                if (Neighborhood != null) parts.Add(Neighborhood.Name);
                if (Alley != null) parts.Add("زقاق " + Alley.Name);
                if (!string.IsNullOrEmpty(Street)) parts.Add("شارع " + Street);
                if (!string.IsNullOrEmpty(BuildingNumber)) parts.Add("بناية " + BuildingNumber);

                // إذا لم يوجد شيء، جرّب الحقول القديمة
                if (!parts.Any())
                {
                    if (!string.IsNullOrEmpty(Area)) parts.Add(Area);
                    if (!string.IsNullOrEmpty(Street)) parts.Add(Street);
                    if (!string.IsNullOrEmpty(BuildingNumber)) parts.Add("بناية " + BuildingNumber);
                    if (!string.IsNullOrEmpty(Floor)) parts.Add("ط" + Floor);
                    if (!string.IsNullOrEmpty(ApartmentNumber)) parts.Add("شقة " + ApartmentNumber);
                }

                return parts.Count > 0 ? string.Join(" - ", parts) : "—";
            }
        }

        [NotMapped]
        public string LandmarkText =>
            !string.IsNullOrEmpty(NearestLandmark) ? $"مقابل: {NearestLandmark}" : "";

        [NotMapped]
        public int ActiveSubscriptionsCount =>
            Subscriptions?.Count(s => s.Status == SubscriptionStatus.Active) ?? 0;

        [NotMapped]
        public decimal TotalMonthlyFees =>
            Subscriptions?
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Sum(s => s.MonthlyAmount) ?? 0;

        [NotMapped]
        public decimal TotalPaid => Payments?.Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal TotalInvoiced =>
            Invoices?.Where(i => i.Status != InvoiceStatus.Cancelled)
                    .Sum(i => i.Amount) ?? 0;

        [NotMapped]
        public decimal Balance => TotalPaid - TotalInvoiced;

        [NotMapped]
        public string MobileStatusText => IsMobileLinked ? "مرتبط" : "غير مرتبط";

        [NotMapped]
        public string MobileStatusBadge => IsMobileLinked ? "bg-success" : "bg-secondary";
    }
}