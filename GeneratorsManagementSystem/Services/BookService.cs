using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Accounting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        // ═══ إعدادات الملفات ═══
        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB
        private const string UploadFolder = "uploads/books";

        public BookService(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ═══ توليد رقم داخلي: BOOK-26-00001 ═══
        public async Task<string> GenerateInternalNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"BOOK-{year}-";

            var lastNumber = await _db.GeneratorBooks
                .Where(b => b.InternalNumber.StartsWith(prefix))
                .OrderByDescending(b => b.InternalNumber)
                .Select(b => b.InternalNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            var newNumber = $"{prefix}{nextSeq:D5}";
            while (await _db.GeneratorBooks.AnyAsync(b => b.InternalNumber == newNumber))
            {
                nextSeq++;
                newNumber = $"{prefix}{nextSeq:D5}";
            }

            return newNumber;
        }

        // ═══ الكل ═══
        public async Task<List<GeneratorBook>> GetAllAsync(bool includeArchived = false)
        {
            var query = _db.GeneratorBooks.AsQueryable();

            if (!includeArchived)
                query = query.Where(b => !b.IsArchived);

            return await query
                .OrderByDescending(b => b.BookDate)
                .ThenByDescending(b => b.Id)
                .ToListAsync();
        }

        // ═══ بالمعرّف ═══
        public async Task<GeneratorBook?> GetByIdAsync(int id)
        {
            return await _db.GeneratorBooks
                .Include(b => b.RenewedFromBook)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // ═══ إنشاء ═══
        public async Task<GeneratorBook> CreateAsync(GeneratorBook book, IFormFile? attachment, string createdBy)
        {
            book.InternalNumber = await GenerateInternalNumberAsync();
            book.CreatedAt = DateTime.Now;
            book.CreatedBy = createdBy;

            // معالجة تاريخ الانتهاء
            if (!book.HasExpiry)
                book.ExpiryDate = null;

            // رفع المرفق
            if (attachment != null && attachment.Length > 0)
            {
                var (path, name, size, type) = await UploadFileAsync(attachment);
                book.AttachmentPath = path;
                book.AttachmentName = name;
                book.AttachmentSize = size;
                book.AttachmentType = type;
            }

            _db.GeneratorBooks.Add(book);
            await _db.SaveChangesAsync();

            // إنشاء مصروف تلقائي إذا كان هناك مبلغ
            if (book.Amount.HasValue && book.Amount.Value > 0)
            {
                await CreateAutoExpenseAsync(book, createdBy);
                await _db.SaveChangesAsync();
            }

            return book;
        }

        // ═══ تحديث ═══
        public async Task<GeneratorBook> UpdateAsync(GeneratorBook book, IFormFile? attachment, string updatedBy)
        {
            var existing = await _db.GeneratorBooks.FindAsync(book.Id)
                ?? throw new Exception("الكتاب غير موجود");

            existing.BookName = book.BookName;
            existing.IssuingAuthority = book.IssuingAuthority;
            existing.BookNumber = book.BookNumber;
            existing.Category = book.Category;
            existing.BookDate = book.BookDate;
            existing.HasExpiry = book.HasExpiry;
            existing.ExpiryDate = book.HasExpiry ? book.ExpiryDate : null;
            existing.Amount = book.Amount;
            existing.Notes = book.Notes;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = updatedBy;

            // رفع مرفق جديد (يحل محل القديم)
            if (attachment != null && attachment.Length > 0)
            {
                // حذف الملف القديم
                if (!string.IsNullOrEmpty(existing.AttachmentPath))
                    DeleteFile(existing.AttachmentPath);

                var (path, name, size, type) = await UploadFileAsync(attachment);
                existing.AttachmentPath = path;
                existing.AttachmentName = name;
                existing.AttachmentSize = size;
                existing.AttachmentType = type;
            }

            await _db.SaveChangesAsync();
            return existing;
        }

        // ═══ حذف ═══
        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _db.GeneratorBooks.FindAsync(id);
            if (book == null) return false;

            // حذف الملف المرفق
            if (!string.IsNullOrEmpty(book.AttachmentPath))
                DeleteFile(book.AttachmentPath);

            // حذف المصروف المرتبط إن وُجد
            if (book.ExpenseId.HasValue)
            {
                var expense = await _db.Expenses.FindAsync(book.ExpenseId.Value);
                if (expense != null)
                    _db.Expenses.Remove(expense);
            }

            _db.GeneratorBooks.Remove(book);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ أرشفة ═══
        public async Task<bool> ArchiveAsync(int id)
        {
            var book = await _db.GeneratorBooks.FindAsync(id);
            if (book == null) return false;

            book.IsArchived = true;
            book.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnarchiveAsync(int id)
        {
            var book = await _db.GeneratorBooks.FindAsync(id);
            if (book == null) return false;

            book.IsArchived = false;
            book.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ تجديد كتاب ═══
        public async Task<GeneratorBook> RenewAsync(int oldBookId, GeneratorBook newBook, IFormFile? attachment, string createdBy)
        {
            var oldBook = await _db.GeneratorBooks.FindAsync(oldBookId)
                ?? throw new Exception("الكتاب القديم غير موجود");

            // ربط الكتاب الجديد بالقديم
            newBook.RenewedFromBookId = oldBookId;

            // إنشاء الكتاب الجديد
            var created = await CreateAsync(newBook, attachment, createdBy);

            // تحديث الكتاب القديم
            oldBook.IsRenewed = true;
            oldBook.IsArchived = true;
            oldBook.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return created;
        }

        // ═══ الفلترة ═══
        public async Task<List<GeneratorBook>> GetByStatusAsync(BookStatus status)
        {
            var books = await GetAllAsync(includeArchived: status == BookStatus.Archived);
            return books.Where(b => b.Status == status).ToList();
        }

        public async Task<List<GeneratorBook>> GetExpiringSoonAsync(int daysAhead = 30)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(daysAhead);

            return await _db.GeneratorBooks
                .Where(b => !b.IsArchived
                         && b.HasExpiry
                         && b.ExpiryDate.HasValue
                         && b.ExpiryDate.Value.Date >= today
                         && b.ExpiryDate.Value.Date <= futureDate)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();
        }

        public async Task<List<GeneratorBook>> GetExpiredAsync()
        {
            var today = DateTime.Today;

            return await _db.GeneratorBooks
                .Where(b => !b.IsArchived
                         && b.HasExpiry
                         && b.ExpiryDate.HasValue
                         && b.ExpiryDate.Value.Date < today)
                .OrderByDescending(b => b.ExpiryDate)
                .ToListAsync();
        }

        public async Task<List<GeneratorBook>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await GetAllAsync();

            term = term.Trim().ToLower();

            return await _db.GeneratorBooks
                .Where(b => !b.IsArchived
                         && (b.BookName.ToLower().Contains(term)
                          || b.BookNumber.ToLower().Contains(term)
                          || b.InternalNumber.ToLower().Contains(term)
                          || b.IssuingAuthority.ToLower().Contains(term)))
                .OrderByDescending(b => b.BookDate)
                .ToListAsync();
        }

        // ═══ الإحصائيات ═══
        public async Task<BookStats> GetStatsAsync()
        {
            var books = await _db.GeneratorBooks.ToListAsync();
            var today = DateTime.Today;

            return new BookStats
            {
                Total = books.Count(b => !b.IsArchived),
                Active = books.Count(b => !b.IsArchived && b.Status == BookStatus.Active),
                ExpiringSoon = books.Count(b => !b.IsArchived && b.Status == BookStatus.ExpiringSoon),
                Expired = books.Count(b => !b.IsArchived && b.Status == BookStatus.Expired),
                NoExpiry = books.Count(b => !b.IsArchived && b.Status == BookStatus.NoExpiry),
                Archived = books.Count(b => b.IsArchived),
                WithAmount = books.Count(b => !b.IsArchived && b.Amount.HasValue && b.Amount.Value > 0),
                TotalAmount = books.Where(b => !b.IsArchived && b.Amount.HasValue).Sum(b => b.Amount ?? 0),
                ByCategory = books
                    .Where(b => !b.IsArchived)
                    .GroupBy(b => b.CategoryText)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        // ═══ التنبيهات ═══
        public async Task<List<BookAlert>> GetAlertsAsync(int daysAhead = 30)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(daysAhead);

            var books = await _db.GeneratorBooks
                .Where(b => !b.IsArchived
                         && b.HasExpiry
                         && b.ExpiryDate.HasValue
                         && b.ExpiryDate.Value.Date <= futureDate)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();

            return books.Select(b =>
            {
                var days = (b.ExpiryDate!.Value.Date - today).Days;
                var alertLevel = days < 0 ? "danger"
                              : days <= 3 ? "danger"
                              : days <= 7 ? "warning"
                              : "info";

                var alertMessage = days < 0 ? $"منتهي منذ {Math.Abs(days)} يوم"
                                : days == 0 ? "ينتهي اليوم"
                                : days == 1 ? "ينتهي غداً"
                                : $"باقي {days} يوم";

                var badgeClass = days < 0 ? "bg-danger"
                              : days <= 3 ? "bg-danger"
                              : days <= 7 ? "bg-warning"
                              : "bg-info";

                return new BookAlert
                {
                    BookId = b.Id,
                    InternalNumber = b.InternalNumber,
                    BookName = b.BookName,
                    IssuingAuthority = b.IssuingAuthority,
                    ExpiryDate = b.ExpiryDate,
                    DaysUntilExpiry = days,
                    AlertLevel = alertLevel,
                    AlertMessage = alertMessage,
                    BadgeClass = badgeClass,
                    CategoryText = b.CategoryText,
                    CategoryIcon = b.CategoryIcon
                };
            }).ToList();
        }

        // ═══ الملفات ═══
        public async Task<byte[]?> GetAttachmentAsync(int bookId)
        {
            var book = await _db.GeneratorBooks.FindAsync(bookId);
            if (book == null || string.IsNullOrEmpty(book.AttachmentPath))
                return null;

            var fullPath = Path.Combine(_env.WebRootPath, book.AttachmentPath);
            if (!File.Exists(fullPath)) return null;

            return await File.ReadAllBytesAsync(fullPath);
        }

        public string? GetAttachmentPath(int bookId)
        {
            var book = _db.GeneratorBooks.Find(bookId);
            return book?.AttachmentPath;
        }

        // ═══════════════════════════════════════
        //  Private Helpers
        // ═══════════════════════════════════════

        private async Task<(string path, string name, long size, string type)> UploadFileAsync(IFormFile file)
        {
            // التحقق من الحجم
            if (file.Length > _maxFileSize)
                throw new Exception($"حجم الملف يتجاوز الحد الأقصى (10 MB)");

            // التحقق من نوع الملف
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                throw new Exception("نوع الملف غير مسموح. المسموح: PDF, JPG, PNG");

            // إنشاء المجلد إن لم يكن موجوداً
            var uploadsPath = Path.Combine(_env.WebRootPath, UploadFolder);
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // اسم فريد للملف
            var uniqueName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsPath, uniqueName);

            // حفظ الملف
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"{UploadFolder}/{uniqueName}";
            return (relativePath, file.FileName, file.Length, file.ContentType);
        }

        private void DeleteFile(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch { /* تجاهل خطأ الحذف */ }
        }

        private async Task CreateAutoExpenseAsync(GeneratorBook book, string createdBy)
        {
            // توليد رقم المصروف
            var year = DateTime.Now.ToString("yy");
            var prefix = $"EXP-{year}-";
            var lastExpense = await _db.Expenses
                .Where(e => e.ExpenseNumber.StartsWith(prefix))
                .OrderByDescending(e => e.ExpenseNumber)
                .Select(e => e.ExpenseNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastExpense))
            {
                var lastSeq = lastExpense.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            var expense = new Expense
            {
                ExpenseNumber = $"{prefix}{nextSeq:D5}",
                Category = ExpenseCategory.Administrative,
                Description = $"كتاب: {book.BookName} - {book.IssuingAuthority}",
                Amount = book.Amount ?? 0,
                ExpenseDate = book.BookDate,
                PaymentMethod = PaymentMethod.Cash,
                Reference = book.BookNumber,
                Beneficiary = book.IssuingAuthority,
                Notes = $"مصروف تلقائي من كتاب ({book.InternalNumber})",
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy
            };

            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();

            // ربط الكتاب بالمصروف
            book.ExpenseId = expense.Id;
        }
    }
}