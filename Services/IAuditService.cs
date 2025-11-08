namespace SAProject.Services
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string userName, string action, string? controllerName = null, string? actionName = null, string? requestPath = null, string? ipAddress = null);
    }
}
