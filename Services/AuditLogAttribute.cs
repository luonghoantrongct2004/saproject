using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SAProject.Services;
using System.Security.Claims;

public class AuditLogAttribute : ActionFilterAttribute
{
    private readonly ILogger<AuditLogAttribute> _logger;

    public AuditLogAttribute(ILogger<AuditLogAttribute> logger)
    {
        _logger = logger;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var auditService = context.HttpContext.RequestServices.GetRequiredService<IAuditService>();

        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = context.HttpContext.User.Identity?.Name;

        var controller = context.Controller.GetType().Name.Replace("Controller", "");
        var action = context.ActionDescriptor.RouteValues["action"];
        var auditAction = $"{context.HttpContext.Request.Method} {controller}/{action}";

        await auditService.LogAsync(
            userId: userId ?? "Unknown",
            userName: userName ?? "Unknown",
            action: auditAction,
            controllerName: controller,
            actionName: action,
            requestPath: context.HttpContext.Request.Path,
            ipAddress: context.HttpContext.Connection.RemoteIpAddress?.ToString()
        );

        await next();
    }
}