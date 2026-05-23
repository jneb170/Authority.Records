using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.UI.Services;

/// <summary>
/// Blazor Server-aware current user. HttpContext is null after the SignalR
/// handshake, so fall back to AuthenticationStateProvider — same pattern as
/// BlazorTenantProvider.
/// </summary>
public sealed class BlazorCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authStateProvider;

    public BlazorCurrentUserContext(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authStateProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
    }

    public bool IsInRole(string roleName)
    {
        var httpUser = _httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
            return httpUser.IsInRole(roleName);

        var state = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        return state.User.IsInRole(roleName);
    }
}
