namespace Modules.Records.Application.DTOs;

public sealed record NarrativeDto(
    Guid Id,
    long RecordNumber,
    Guid JurisdictionId,
    string Title,
    string Content,
    bool IsLocked,
    Guid? LockedByUserId,
    Guid CreatedBy,
    Guid? ModifiedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
