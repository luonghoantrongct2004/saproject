using SAProject.Data;
using SAProject.Models;

namespace SAProject.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string userId, string userName, string action, string? controllerName = null, string? actionName = null, string? requestPath = null, string? ipAddress = null)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                ControllerName = controllerName,
                ActionName = actionName,
                RequestPath = requestPath,
                IpAddress = ipAddress ?? context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
