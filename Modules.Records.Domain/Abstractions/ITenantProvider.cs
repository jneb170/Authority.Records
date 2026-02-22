namespace Modules.Records.Domain.Abstractions;

public interface ITenantProvider
{
    Guid GetJurisdictionId();
    Guid GetAgencyId();
    Guid GetUserId();

    //required for mutable tenant provider for non-Http execution. 
    void SetJurisdictionId(Guid jurisdictionId);
}
