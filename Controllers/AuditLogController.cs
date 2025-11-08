using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SAProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs()
        {
            try
            {
                var auditLogs = await _context.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Select(a => new
                    {
                        id = a.Id,
                        userId = a.UserId,
                        userName = a.UserName ?? "N/A",
                        action = a.Action,
                        controllerName = a.ControllerName ?? "N/A",
                        actionName = a.ActionName ?? "N/A",
                        requestPath = a.RequestPath ?? "N/A",
                        ipAddress = a.IpAddress ?? "N/A",
                        timestamp = a.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")
                    })
                    .ToListAsync();

                return Json(new { data = auditLogs });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auditLog == null)
            {
                return NotFound();
            }

            return View(auditLog);
        }
    }
}