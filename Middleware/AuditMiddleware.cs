using System.Security.Claims;
using SAProject.Services;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditService _auditService;

    public AuditMiddleware(RequestDelegate next, IAuditService auditService)
    {
        _next = next;
        _auditService = auditService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = context.User.Identity?.Name;
        var controller = context.GetRouteData().Values["controller"]?.ToString();
        var action = context.GetRouteData().Values["action"]?.ToString();

        string auditAction = $"{context.Request.Method} {controller}/{action}";
        if (context.Request.Method == "POST" && action == "Login")
        {
            auditAction = "Login Success";
        }
        else if (context.Request.Path.Value?.EndsWith("Logout", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            auditAction = "Logout";
        }

        await _auditService.LogAsync(
            userId: userId ?? "Unknown",
            userName: userName ?? "Unknown",
            action: auditAction,
            controllerName: controller,
            actionName: action,
            requestPath: context.Request.Path,
            ipAddress: context.Connection.RemoteIpAddress?.ToString()
        );
        await _next(context);
    }
}