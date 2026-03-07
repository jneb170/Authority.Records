namespace Modules.Records.Application.DTOs;

public sealed record PicklistItemDto(
    Guid   Id,
    Guid   JurisdictionId,
    Guid   AgencyId,
    string PicklistType,
    string Value,
    string Label,
    int    SortOrder,
    bool   IsActive,
    bool   IsSystemDefault);
