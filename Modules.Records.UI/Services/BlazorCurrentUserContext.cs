using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;

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
    private readonly string _demoEmail;

    public BlazorCurrentUserContext(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authStateProvider,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
        _demoEmail = DemoUserMatching.ResolveDemoEmail(configuration[DemoUserMatching.LoginEmailConfigKey]);
    }

    private ClaimsPrincipal? GetUser()
    {
        var httpUser = _httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
            return httpUser;

        var state = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        return state.User;
    }

    public bool IsInRole(string roleName) => GetUser()?.IsInRole(roleName) ?? false;

    public bool IsDemoUser => DemoUserMatching.IsDemoPrincipal(GetUser(), _demoEmail);
}
