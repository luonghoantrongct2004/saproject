namespace SAProject.Middleware;

using Microsoft.AspNetCore.Mvc.Filters;
using SAProject.Services;
using System.Security.Claims;

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditService _auditService;

    public AuditActionFilter(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Thực thi action trước
        var result = await next();

        // Audit sau khi action hoàn thành
        await AuditAction(context, result);
    }

    private async Task AuditAction(ActionExecutingContext context, ActionExecutedContext result)
    {
        try
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = user.Identity?.Name;
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            string auditAction = $"{httpContext.Request.Method} {controller}/{action}";

            if (httpContext.Request.Method == "POST" && action == "Login")
            {
                auditAction = "Login Success";
            }
            else if (httpContext.Request.Path.Value?.EndsWith("Logout", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                auditAction = "Logout";
            }

            await _auditService.LogAsync(
                userId: userId ?? "Unknown",
                userName: userName ?? "Unknown",
                action: auditAction,
                controllerName: controller,
                actionName: action,
                requestPath: httpContext.Request.Path,
                ipAddress: httpContext.Connection.RemoteIpAddress?.ToString()
            );
        }
        catch (Exception ex)
        {
            // Log lỗi nhưng không làm gián đoạn request
            var logger = context.HttpContext.RequestServices.GetService<ILogger<AuditActionFilter>>();
            logger?.LogError(ex, "Error occurred while auditing action");
        }
    }
}
