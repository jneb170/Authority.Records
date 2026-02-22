using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Modules.Records.Domain.Abstractions;

namespace Shared.Infrastructure.Identity;

public sealed class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _backgroundTenantId;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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
        if (_backgroundTenantId.HasValue)
            return _backgroundTenantId.Value;
        else
            return GetRequiredGuidClaim("jurisdiction");
    }

    public void SetJurisdictionId(Guid jurisdictionId)
    {
        _backgroundTenantId = jurisdictionId;
    }

    public Guid? GetAgencyId() => GetOptionalGuidClaim("agency");

    public Guid? GetUserId() => GetOptionalGuidClaim(ClaimTypes.NameIdentifier);

    Guid ITenantProvider.GetAgencyId()
    {
        throw new NotImplementedException();
    }

    Guid ITenantProvider.GetUserId()
    {
        throw new NotImplementedException();
    }
}
