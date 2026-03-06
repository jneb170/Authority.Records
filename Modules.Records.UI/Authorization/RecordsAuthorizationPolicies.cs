using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Records.UI.Authorization;

public static class RecordsAuthorizationPolicies
{
    public const string RecordsRead = "RecordsRead";
    public const string RecordsWrite = "RecordsWrite";
    public const string RecordsAdmin = "RecordsAdmin";

    public static void RegisterPolicies(IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(RecordsRead, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(RecordsWrite, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(RecordsAdmin, policy =>
                policy.RequireAuthenticatedUser());
        });
    }
}
