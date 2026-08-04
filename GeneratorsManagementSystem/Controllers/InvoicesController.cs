using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceService _service;
        private readonly IAuditService _auditService; // 🆕
        private readonly IPaymentService _paymentService;  // 🆕 
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoicesController(
            IInvoiceService service,
            IAuditService auditService, // 🆕
            IPaymentService paymentService,  // 🆕
            UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _auditService = auditService; // 🆕
            _paymentService = paymentService;  // 🆕
            _userManager = userManager;
        }

        // ══════════════════════════════════════
        //  INDEX
        // ══════════════════════════════════════
        public async Task<IActionResult> Index(string filter = "all")
        {
            ViewData["PageTitle"] = "إدارة الفواتير";

            List<Invoice> invoices = filter switch
            {
                "overdue" => await _service.GetOverdueInvoicesAsync(),
                "unpaid" => await _service.GetUnpaidInvoicesAsync(),
                "upcoming" => await _service.GetUpcomingInvoicesAsync(7),
                _ => await _service.GetAllAsync()
            };

            ViewBag.Stats = await _service.GetStatsAsync();
            ViewBag.CurrentFilter = filter;
            return View(invoices);
        }

        // ══════════════════════════════════════
        //  DETAILS
        // ══════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null) return NotFound();

            ViewData["PageTitle"] = $"فاتورة {invoice.InvoiceNumber}";
            return View(invoice);
        }

        // ══════════════════════════════════════
        //  🎯 PRINT - طباعة الفاتورة
        // ══════════════════════════════════════
        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null) return NotFound();

            return View(invoice);  // View بلا Layout
        }

        // ══════════════════════════════════════
        //  DELETE
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                return Json(new { success = result, message = result ? "تم الحذف" : "غير موجود" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  CANCEL
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            try
            {
                var result = await _service.CancelAsync(id, reason ?? "");
                return Json(new { success = result, message = result ? "تم الإلغاء" : "فشل" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  🎯 GENERATE MONTHLY - توليد فواتير الشهر
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMonthly()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var invoices = await _service.GenerateMonthlyInvoicesAsync(user?.FullName ?? "النظام");

                 


                return Json(new
                {
                    success = true,
                    count = invoices.Count,
                    message = invoices.Count > 0
                        ? $"تم توليد {invoices.Count} فاتورة بنجاح"
                        : "لا توجد فواتير مستحقة اليوم"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  🎯 ALERTS API - التنبيهات
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Alerts(int daysAhead = 7)
        {
            var alerts = await _service.GetAlertsAsync(daysAhead);
            return Json(new
            {
                success = true,
                count = alerts.Count,
                overdueCount = alerts.Count(a => a.AlertType == "overdue"),
                todayCount = alerts.Count(a => a.AlertType == "today"),
                upcomingCount = alerts.Count(a => a.AlertType == "upcoming"),
                alerts
            });
        }

        // ══════════════════════════════════════
        //  ALERTS PAGE
        // ══════════════════════════════════════
        public async Task<IActionResult> AlertsPage()
        {
            ViewData["PageTitle"] = "تنبيهات الفواتير";
            var alerts = await _service.GetAlertsAsync(30);
            return View(alerts);
        }


        // ══════════════════════════════════════
        //  💰 CREATE PAYMENT - تسجيل دفعة
        // ══════════════════════════════════════
        public async Task<IActionResult> CreatePayment(int invoiceId)
        {
            var invoice = await _service.GetByIdAsync(invoiceId);
            if (invoice == null) return NotFound();

            if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
                return Json(new { success = false, message = "لا يمكن الدفع على هذه الفاتورة" });

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                SubscriberId = invoice.SubscriberId,
                Amount = invoice.RemainingAmount,
                PaymentDate = DateTime.Today,
                PaymentMethod = PaymentMethod.Cash,
                ReceiptNumber = await _paymentService.GenerateReceiptNumberAsync()
            };

            ViewBag.Invoice = invoice;
            return PartialView("_CreatePaymentModal", payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment(Payment payment)
        {
            try
            {
                ModelState.Remove(nameof(payment.ReceiptNumber));
                ModelState.Remove(nameof(payment.CreatedBy));
                ModelState.Remove(nameof(payment.Subscriber));
                ModelState.Remove(nameof(payment.Invoice));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);
                var created = await _paymentService.CreateAsync(payment, user?.FullName ?? "النظام");

                // تسجيل النشاط
                var invoice = await _service.GetByIdAsync(created.InvoiceId);
                try
                {
                    await _auditService.LogPaymentAsync(
                        created.Id,
                        invoice?.Subscriber?.FullName ?? "غير معروف",
                        created.Amount);
                }
                catch { }

                return Json(new
                {
                    success = true,
                    message = $"تم تسجيل الدفعة ({created.ReceiptNumber}) بنجاح - {created.Amount:N0} د.ع",
                    receiptNumber = created.ReceiptNumber,
                    paymentId = created.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                    return Json(new { success = false, message = "الدفعة غير موجودة" });

                var receiptNumber = payment.ReceiptNumber;
                var result = await _paymentService.DeleteAsync(id);

                return Json(new
                {
                    success = result,
                    message = result ? "تم حذف الدفعة" : "فشل الحذف"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}