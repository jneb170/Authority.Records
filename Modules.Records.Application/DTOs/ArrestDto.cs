namespace Modules.Records.Application.DTOs;

public sealed record ArrestDto(
    Guid Id,
    Guid JurisdictionId,
    Guid AgencyId,
    Guid IncidentId,
    string SuspectName,
    DateTime ArrestedAt,
    string Status,
    bool IsLocked,
    Guid? LockedByUserId,
    Guid CreatedBy,
    Guid? ModifiedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
