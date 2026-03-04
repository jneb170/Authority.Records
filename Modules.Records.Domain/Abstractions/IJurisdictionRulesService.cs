namespace Modules.Records.Domain.Abstractions;

public interface IJurisdictionRulesService
{
    bool MustCloseAllArrests(Guid jurisdictionId);
    bool MustCloseAllCitations(Guid jurisdictionId);

    // Add more jurisdiction-level rules here as needed
}