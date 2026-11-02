using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Shared.Infrastructure.Identity;

namespace Modules.Records.UI.Services;

/// <summary>
/// Periodically re-checks the user behind an open Blazor Server circuit.
/// </summary>
/// <remarks>
/// A circuit keeps the principal from the HTTP request that started it, so cookie
/// validation alone would not end a live session. Every
/// <see cref="ActiveUserSecurityStampValidator.ValidationInterval"/> this verifies the
/// security stamp is unchanged and the user can still sign in (i.e. is active). On failure
/// the circuit becomes anonymous and <c>AuthorizeRouteView</c> sends the user to login.
/// </remarks>
public sealed class RevalidatingIdentityAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => ActiveUserSecurityStampValidator.ValidationInterval;

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        // Runs on a timer, concurrently with the circuit's own work, so use a fresh scope
        // (and therefore a fresh AuthDbContext) rather than the circuit's.
        await using var scope = scopeFactory.CreateAsyncScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        var user = await signInManager.ValidateSecurityStampAsync(authenticationState.User);
        return user is not null && await signInManager.CanSignInAsync(user);
    }
}
