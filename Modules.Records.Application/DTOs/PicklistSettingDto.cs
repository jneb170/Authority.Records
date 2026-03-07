namespace Modules.Records.Application.DTOs;

public sealed record PicklistSettingDto(
    Guid   Id,
    Guid   JurisdictionId,
    Guid   AgencyId,
    string PicklistType,
    bool   IsRequired);
