using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Records.UI.Authorization;

public static class RecordsAuthorizationPolicies
{
    public const string RecordsRead = "RecordsRead";
    public const string RecordsWrite = "RecordsWrite";
    public const string RecordsAdmin = "RecordsAdmin";
    public const string SuperOnly = "SuperOnly";
    public const string AdminOnly = "AdminOnly";

    private static readonly string[] RecordsRoles = ["Admin", "Supervisor", "Officer", "Dispatcher"];

    public static void RegisterPolicies(IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(RecordsRead, policy =>
                policy.RequireRole(RecordsRoles));

            options.AddPolicy(RecordsWrite, policy =>
                policy.RequireRole(RecordsRoles));

            options.AddPolicy(RecordsAdmin, policy =>
                policy.RequireRole("Admin", "Supervisor"));

            options.AddPolicy(SuperOnly, policy =>
                policy.RequireRole("Super"));

            options.AddPolicy(AdminOnly, policy =>
                policy.RequireRole("Admin"));
        });
    }
}
