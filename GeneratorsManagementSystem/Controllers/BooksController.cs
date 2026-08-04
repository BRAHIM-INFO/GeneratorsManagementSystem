using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly IBookService _service;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public BooksController(
            IBookService service,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _service = service;
            _auditService = auditService;
            _userManager = userManager;
            _env = env;
        }

        // ══════════════════════════════════════
        //  INDEX - قائمة الكتب
        // ══════════════════════════════════════
        public async Task<IActionResult> Index(string? filter = null, string? searchTerm = null)
        {
            ViewData["PageTitle"] = "كتب المولدة";

            List<GeneratorBook> books;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                books = await _service.SearchAsync(searchTerm);
            }
            else
            {
                books = filter switch
                {
                    "expiring" => await _service.GetExpiringSoonAsync(30),
                    "expired" => await _service.GetExpiredAsync(),
                    "noexpiry" => await _service.GetByStatusAsync(BookStatus.NoExpiry),
                    "archived" => await _service.GetAllAsync(includeArchived: true),
                    _ => await _service.GetAllAsync()
                };

                if (filter == "archived")
                    books = books.Where(b => b.IsArchived).ToList();
            }

            var stats = await _service.GetStatsAsync();

            ViewBag.Stats = stats;
            ViewBag.CurrentFilter = filter;
            ViewBag.SearchTerm = searchTerm;

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.System,
                $"عرض قائمة كتب المولدة ({books.Count} كتاب)");

            return View(books);
        }

        // ══════════════════════════════════════
        //  DETAILS - تفاصيل الكتاب
        // ══════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            var book = await _service.GetByIdAsync(id);
            if (book == null) return NotFound();

            ViewData["PageTitle"] = $"تفاصيل الكتاب - {book.BookName}";

            await _auditService.LogAsync(
                AuditActionType.View,
                AuditModule.System,
                $"عرض تفاصيل الكتاب: {book.BookName}",
                "كتاب",
                id,
                book.InternalNumber);

            return View(book);
        }

        // ══════════════════════════════════════
        //  CREATE - نافذة إضافة
        // ══════════════════════════════════════
        public async Task<IActionResult> Create()
        {
            var book = new GeneratorBook
            {
                InternalNumber = await _service.GenerateInternalNumberAsync(),
                BookDate = DateTime.Today,
                Category = BookCategory.Other,
                HasExpiry = false
            };
            return PartialView("_CreateModal", book);
        }

        // ══════════════════════════════════════
        //  CREATE POST
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(15 * 1024 * 1024)] // 15 MB للاحتياط
        public async Task<IActionResult> Create(GeneratorBook book, IFormFile? attachment)
        {
            try
            {
                ModelState.Remove(nameof(book.InternalNumber));
                ModelState.Remove(nameof(book.CreatedBy));
                ModelState.Remove(nameof(book.RenewedFromBook));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                // التحقق من تاريخ الانتهاء
                if (book.HasExpiry && !book.ExpiryDate.HasValue)
                {
                    return Json(new
                    {
                        success = false,
                        message = "يرجى إدخال تاريخ الانتهاء",
                        errors = new[] { "تاريخ الانتهاء مطلوب" }
                    });
                }

                if (book.HasExpiry && book.ExpiryDate.HasValue && book.ExpiryDate.Value < book.BookDate)
                {
                    return Json(new
                    {
                        success = false,
                        message = "تاريخ الانتهاء يجب أن يكون بعد تاريخ الكتاب",
                        errors = new[] { "تاريخ الانتهاء غير صحيح" }
                    });
                }

                // التحقق من الملف
                if (attachment != null && attachment.Length > 0)
                {
                    if (attachment.Length > 10 * 1024 * 1024)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "حجم الملف يتجاوز 10 MB"
                        });
                    }

                    var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(attachment.FileName).ToLower();
                    if (!allowedExt.Contains(ext))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "نوع الملف غير مسموح. المسموح: PDF, JPG, PNG"
                        });
                    }
                }

                var user = await _userManager.GetUserAsync(User);

                // تنظيف الحقول
                if (string.IsNullOrWhiteSpace(book.Notes)) book.Notes = null;
                if (book.Amount == 0) book.Amount = null;

                var created = await _service.CreateAsync(book, attachment, user?.FullName ?? "النظام");

                // تسجيل النشاط
                await _auditService.LogCreateAsync(
                    AuditModule.System,
                    "كتاب مولدة",
                    created.Id,
                    $"{created.BookName} - {created.IssuingAuthority}",
                    new
                    {
                        created.InternalNumber,
                        created.BookName,
                        created.IssuingAuthority,
                        created.BookNumber,
                        Amount = created.Amount ?? 0
                    });

                var message = $"تم إضافة الكتاب ({created.InternalNumber}) بنجاح";
                if (created.Amount.HasValue && created.Amount.Value > 0)
                    message += $" وتم تسجيل مصروف تلقائي بمبلغ {created.Amount.Value:N0} د.ع";

                return Json(new
                {
                    success = true,
                    message,
                    bookId = created.Id,
                    internalNumber = created.InternalNumber
                });
            }
            catch (Exception ex)
            {
                var fullMessage = "حدث خطأ: " + ex.Message;
                if (ex.InnerException != null)
                    fullMessage += " | التفاصيل: " + ex.InnerException.Message;

                await _auditService.LogAsync(
                    AuditActionType.Create,
                    AuditModule.System,
                    "فشل إضافة كتاب",
                    isSuccess: false,
                    errorMessage: fullMessage);

                return Json(new { success = false, message = fullMessage });
            }
        }

        // ══════════════════════════════════════
        //  EDIT - نافذة تعديل
        // ══════════════════════════════════════
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _service.GetByIdAsync(id);
            if (book == null) return NotFound();
            return PartialView("_EditModal", book);
        }

        // ══════════════════════════════════════
        //  EDIT POST
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> Edit(GeneratorBook book, IFormFile? attachment)
        {
            try
            {
                ModelState.Remove(nameof(book.CreatedBy));
                ModelState.Remove(nameof(book.RenewedFromBook));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                if (book.HasExpiry && !book.ExpiryDate.HasValue)
                {
                    return Json(new
                    {
                        success = false,
                        message = "يرجى إدخال تاريخ الانتهاء"
                    });
                }

                if (attachment != null && attachment.Length > 0)
                {
                    if (attachment.Length > 10 * 1024 * 1024)
                        return Json(new { success = false, message = "حجم الملف يتجاوز 10 MB" });

                    var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(attachment.FileName).ToLower();
                    if (!allowedExt.Contains(ext))
                        return Json(new { success = false, message = "نوع الملف غير مسموح" });
                }

                var user = await _userManager.GetUserAsync(User);
                var updated = await _service.UpdateAsync(book, attachment, user?.FullName ?? "النظام");

                await _auditService.LogUpdateAsync(
                    AuditModule.System,
                    "كتاب مولدة",
                    updated.Id,
                    $"{updated.BookName} ({updated.InternalNumber})");

                return Json(new
                {
                    success = true,
                    message = "تم تحديث بيانات الكتاب بنجاح"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
            }
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
                var book = await _service.GetByIdAsync(id);
                if (book == null)
                    return Json(new { success = false, message = "الكتاب غير موجود" });

                var bookName = book.BookName;
                var internalNumber = book.InternalNumber;

                var result = await _service.DeleteAsync(id);

                if (result)
                {
                    await _auditService.LogDeleteAsync(
                        AuditModule.System,
                        "كتاب مولدة",
                        id,
                        $"{bookName} ({internalNumber})");
                }

                return Json(new
                {
                    success = result,
                    message = result ? "تم حذف الكتاب بنجاح" : "فشل الحذف"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  ARCHIVE / UNARCHIVE
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                var result = await _service.ArchiveAsync(id);

                if (result)
                {
                    await _auditService.LogAsync(
                        AuditActionType.ToggleStatus,
                        AuditModule.System,
                        $"أرشفة كتاب رقم {id}",
                        "كتاب",
                        id);
                }

                return Json(new
                {
                    success = result,
                    message = result ? "تمت الأرشفة بنجاح" : "فشل"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unarchive(int id)
        {
            try
            {
                var result = await _service.UnarchiveAsync(id);
                return Json(new { success = result, message = result ? "تمت الاستعادة" : "فشل" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ══════════════════════════════════════
        //  DOWNLOAD ATTACHMENT
        // ══════════════════════════════════════
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var book = await _service.GetByIdAsync(id);
            if (book == null || string.IsNullOrEmpty(book.AttachmentPath))
                return NotFound();

            var fullPath = Path.Combine(_env.WebRootPath, book.AttachmentPath);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            await _auditService.LogAsync(
                AuditActionType.Export,
                AuditModule.System,
                $"تحميل مرفق الكتاب: {book.BookName}",
                "كتاب",
                id);

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var contentType = book.AttachmentType ?? "application/octet-stream";
            var fileName = book.AttachmentName ?? Path.GetFileName(fullPath);

            return File(bytes, contentType, fileName);
        }

        // ══════════════════════════════════════
        //  VIEW ATTACHMENT (في المتصفح)
        // ══════════════════════════════════════
        public async Task<IActionResult> ViewAttachment(int id)
        {
            var book = await _service.GetByIdAsync(id);
            if (book == null || string.IsNullOrEmpty(book.AttachmentPath))
                return NotFound();

            var fullPath = Path.Combine(_env.WebRootPath, book.AttachmentPath);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var contentType = book.AttachmentType ?? "application/octet-stream";

            Response.Headers.Add("Content-Disposition",
                $"inline; filename=\"{book.AttachmentName}\"");

            return File(bytes, contentType);
        }

        // ══════════════════════════════════════
        //  ALERTS PAGE - صفحة التنبيهات
        // ══════════════════════════════════════
        public async Task<IActionResult> AlertsPage()
        {
            ViewData["PageTitle"] = "تنبيهات كتب المولدة";
            var alerts = await _service.GetAlertsAsync(60);
            return View(alerts);
        }

        // ══════════════════════════════════════
        //  ALERTS API - للـ Topbar
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Alerts(int daysAhead = 30)
        {
            var alerts = await _service.GetAlertsAsync(daysAhead);
            return Json(new
            {
                success = true,
                count = alerts.Count,
                expiredCount = alerts.Count(a => a.DaysUntilExpiry < 0),
                urgentCount = alerts.Count(a => a.DaysUntilExpiry >= 0 && a.DaysUntilExpiry <= 7),
                warningCount = alerts.Count(a => a.DaysUntilExpiry > 7 && a.DaysUntilExpiry <= 15),
                infoCount = alerts.Count(a => a.DaysUntilExpiry > 15),
                alerts
            });
        }

        // ══════════════════════════════════════
        //  RENEW - تجديد كتاب
        // ══════════════════════════════════════
        public async Task<IActionResult> Renew(int id)
        {
            var oldBook = await _service.GetByIdAsync(id);
            if (oldBook == null) return NotFound();

            // إنشاء كتاب جديد بناءً على القديم
            var newBook = new GeneratorBook
            {
                InternalNumber = await _service.GenerateInternalNumberAsync(),
                BookName = oldBook.BookName,
                IssuingAuthority = oldBook.IssuingAuthority,
                BookNumber = "", // فارغ - سيدخله المستخدم
                Category = oldBook.Category,
                BookDate = DateTime.Today,
                HasExpiry = true,
                ExpiryDate = DateTime.Today.AddYears(1),
                RenewedFromBookId = oldBook.Id,
                Notes = $"تجديد للكتاب: {oldBook.InternalNumber}"
            };

            ViewBag.OldBook = oldBook;
            return PartialView("_RenewModal", newBook);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> Renew(int oldBookId, GeneratorBook newBook, IFormFile? attachment)
        {
            try
            {
                ModelState.Remove(nameof(newBook.CreatedBy));
                ModelState.Remove(nameof(newBook.RenewedFromBook));
                ModelState.Remove(nameof(newBook.InternalNumber));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                        .Distinct().ToList();
                    return Json(new { success = false, message = "يرجى تصحيح الأخطاء", errors });
                }

                var user = await _userManager.GetUserAsync(User);
                var renewed = await _service.RenewAsync(oldBookId, newBook, attachment, user?.FullName ?? "النظام");

                await _auditService.LogAsync(
                    AuditActionType.Create,
                    AuditModule.System,
                    $"تجديد كتاب: {renewed.BookName}",
                    "كتاب",
                    renewed.Id,
                    renewed.InternalNumber);

                return Json(new
                {
                    success = true,
                    message = $"تم تجديد الكتاب بنجاح ({renewed.InternalNumber})"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}