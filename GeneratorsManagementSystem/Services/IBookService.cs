using GeneratorsManagementSystem.Models;
using Microsoft.AspNetCore.Http;

namespace GeneratorsManagementSystem.Services
{
    public interface IBookService
    {
        // ═══ توليد الأرقام ═══
        Task<string> GenerateInternalNumberAsync();

        // ═══ CRUD ═══
        Task<List<GeneratorBook>> GetAllAsync(bool includeArchived = false);
        Task<GeneratorBook?> GetByIdAsync(int id);
        Task<GeneratorBook> CreateAsync(GeneratorBook book, IFormFile? attachment, string createdBy);
        Task<GeneratorBook> UpdateAsync(GeneratorBook book, IFormFile? attachment, string updatedBy);
        Task<bool> DeleteAsync(int id);
        Task<bool> ArchiveAsync(int id);
        Task<bool> UnarchiveAsync(int id);
        Task<GeneratorBook> RenewAsync(int oldBookId, GeneratorBook newBook, IFormFile? attachment, string createdBy);

        // ═══ الفلترة ═══
        Task<List<GeneratorBook>> GetByStatusAsync(BookStatus status);
        Task<List<GeneratorBook>> GetExpiringSoonAsync(int daysAhead = 30);
        Task<List<GeneratorBook>> GetExpiredAsync();
        Task<List<GeneratorBook>> SearchAsync(string term);

        // ═══ الإحصائيات ═══
        Task<BookStats> GetStatsAsync();

        // ═══ التنبيهات ═══
        Task<List<BookAlert>> GetAlertsAsync(int daysAhead = 30);

        // ═══ الملفات ═══
        Task<byte[]?> GetAttachmentAsync(int bookId);
        string? GetAttachmentPath(int bookId);
    }

    public class BookStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int ExpiringSoon { get; set; }
        public int Expired { get; set; }
        public int NoExpiry { get; set; }
        public int Archived { get; set; }
        public int WithAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<string, int> ByCategory { get; set; } = new();
    }

    public class BookAlert
    {
        public int BookId { get; set; }
        public string InternalNumber { get; set; } = string.Empty;
        public string BookName { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string AlertLevel { get; set; } = string.Empty; // info, warning, danger
        public string AlertMessage { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = string.Empty;
        public string CategoryText { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
    }
}