using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    [Authorize]
    public class AuditLogController : Controller
    {
        private readonly IAuditService _service;

        public AuditLogController(IAuditService service)
        {
            _service = service;
        }

        // ══════════════════════════════════════
        //  INDEX
        // ══════════════════════════════════════
        public async Task<IActionResult> Index(
            string? searchTerm = null,
            AuditModule? module = null,
            AuditActionType? actionType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? isSuccess = null,
            int page = 1)
        {
            ViewData["PageTitle"] = "سجل النشاطات";

            var filter = new AuditFilter
            {
                SearchTerm = searchTerm,
                Module = module,
                ActionType = actionType,
                StartDate = startDate,
                EndDate = endDate,
                IsSuccess = isSuccess,
                Page = page,
                PageSize = 50
            };

            var (logs, totalCount) = await _service.SearchAsync(filter);
            var stats = await _service.GetStatsAsync();

            ViewBag.Stats = stats;
            ViewBag.Filter = filter;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return View(logs);
        }

        // ══════════════════════════════════════
        //  DETAILS
        // ══════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            var log = await _service.GetByIdAsync(id);
            if (log == null) return NotFound();
            return PartialView("_DetailsModal", log);
        }

        // ══════════════════════════════════════
        //  RECENT (API)
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Recent(int limit = 10)
        {
            var logs = await _service.GetRecentAsync(limit);
            return Json(logs.Select(l => new
            {
                l.Id,
                actionType = l.ActionTypeText,
                actionColor = l.ActionColor,
                actionIcon = l.ActionIcon,
                module = l.ModuleText,
                moduleIcon = l.ModuleIcon,
                l.Description,
                userName = l.UserFullName ?? l.UserName,
                timeAgo = l.TimeAgo,
                timestamp = l.Timestamp.ToString("yyyy/MM/dd HH:mm:ss"),
                l.IsSuccess
            }));
        }

        // ══════════════════════════════════════
        //  DELETE OLD
        // ══════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOld(int daysToKeep = 90)
        {
            try
            {
                var count = await _service.DeleteOldLogsAsync(daysToKeep);
                await _service.LogAsync(
                    AuditActionType.Delete,
                    AuditModule.System,
                    $"حذف {count} سجل قديم (أقدم من {daysToKeep} يوم)");

                return Json(new { success = true, count, message = $"تم حذف {count} سجل بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}