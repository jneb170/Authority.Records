namespace Modules.Records.Application.DTOs;

public sealed record ArrestDto(
    Guid Id,
    long RecordNumber,
    Guid JurisdictionId,
    Guid AgencyId,
    string SuspectName,
    DateTime ArrestedAt,
    string Status,
    bool IsLocked,
    Guid? LockedByUserId,
    Guid CreatedBy,
    Guid? ModifiedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    Guid? ArrestTypeId = null,
    string ArrestNum = "",
    Guid? LocationId = null);
