namespace Modules.Records.Domain.Abstractions;

public interface ITenantProvider
{
    Guid GetJurisdictionId();
    Guid GetAgencyId();
    Guid GetUserId();
}
