using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.UI.Services;

/// <summary>
/// Blazor Server-aware tenant provider.
/// IHttpContextAccessor.HttpContext is null after the SignalR handshake,
/// so we fall back to AuthenticationStateProvider which holds the
/// auth state for the entire Blazor circuit.
/// </summary>
public sealed class BlazorTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IActiveAgencyContext _activeAgencyContext;
    private Guid? _backgroundTenantId;

    public BlazorTenantProvider(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authStateProvider,
        IActiveAgencyContext activeAgencyContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
        _activeAgencyContext = activeAgencyContext;
    }

    private ClaimsPrincipal GetUser()
    {
        // During prerender / regular HTTP: use HttpContext
        var httpUser = _httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
            return httpUser;

        // During Blazor Server SignalR phase: use AuthenticationStateProvider.
        // ServerAuthenticationStateProvider returns a completed Task (backed by
        // the initial HTTP request's ClaimsPrincipal), so .GetResult() is safe.
        var state = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        return state.User;
    }

    private Guid GetRequiredGuidClaim(ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirst(claimType)?.Value;
        if (value is null)
            return Guid.Empty;
        return Guid.Parse(value);
    }

    public Guid GetJurisdictionId()
    {
        if (_backgroundTenantId.HasValue) return _backgroundTenantId.Value;
        return GetRequiredGuidClaim(GetUser(), "jurisdiction");
    }

    public Guid GetAgencyId()
    {
        if (_activeAgencyContext.HasLoaded)
            return _activeAgencyContext.ActiveAgencyId;

        return GetRequiredGuidClaim(GetUser(), "agency");
    }

    public Guid GetUserId() => GetRequiredGuidClaim(GetUser(), ClaimTypes.NameIdentifier);

    public void SetJurisdictionId(Guid jurisdictionId) => _backgroundTenantId = jurisdictionId;
}
