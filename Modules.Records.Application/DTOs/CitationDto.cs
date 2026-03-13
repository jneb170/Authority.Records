namespace Modules.Records.Application.DTOs;

public sealed record CitationDto(
    Guid Id,
    long RecordNumber,
    Guid JurisdictionId,
    Guid AgencyId,
    string Description,
    DateTime IssueDate,
    bool IsIssued,
    bool IsLocked,
    Guid? LockedByUserId,
    Guid CreatedBy,
    Guid? ModifiedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    Guid? CourtId = null,
    string CitationNum = "",
    Guid? LocationId = null);
