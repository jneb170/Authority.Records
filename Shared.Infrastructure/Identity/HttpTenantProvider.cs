using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Modules.Records.Domain.Abstractions;

namespace Shared.Infrastructure.Identity;

public sealed class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid JurisdictionId =>
        GetRequiredGuidClaim("jurisdiction");

    public Guid? AgencyId =>
        GetOptionalGuidClaim("agency");

    public Guid? UserId =>
        GetOptionalGuidClaim(ClaimTypes.NameIdentifier);

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        if (value == null)
            throw new InvalidOperationException($"Missing required claim: {claimType}");

        return Guid.Parse(value);
    }

    private Guid? GetOptionalGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        return value != null ? Guid.Parse(value) : null;
    }

    public Guid GetJurisdictionId()
    {
        throw new NotImplementedException();
    }

    public Guid GetAgencyId()
    {
        throw new NotImplementedException();
    }

    public Guid GetUserId()
    {
        throw new NotImplementedException();
    }
}
