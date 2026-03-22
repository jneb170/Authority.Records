using Shared.Infrastructure.Maintenance;
using System.Security.Claims;

namespace Modules.Records.UI.Middleware;

public sealed class ApplicationMaintenanceMiddleware
{
    private readonly RequestDelegate _next;

    public ApplicationMaintenanceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ApplicationMaintenanceCoordinator maintenanceCoordinator)
    {
        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var jurisdictionId = GetGuidClaim(context.User, "jurisdiction");
                if (jurisdictionId != Guid.Empty)
                {
                    await maintenanceCoordinator.RunRequestMaintenanceAsync(
                        context.RequestServices,
                        jurisdictionId,
                        context.RequestAborted);
                }
            }
        }

        await _next(context);
    }

    private static Guid GetGuidClaim(ClaimsPrincipal user, string claimType)
    {
        var rawValue = user.FindFirst(claimType)?.Value;
        return Guid.TryParse(rawValue, out var parsed) ? parsed : Guid.Empty;
    }
}
