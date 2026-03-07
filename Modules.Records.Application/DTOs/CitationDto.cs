namespace Modules.Records.Application.DTOs;

public sealed record CitationDto(
    Guid Id,
    Guid JurisdictionId,
    Guid AgencyId,
    Guid IncidentId,
    string Description,
    DateTime IssueDate,
    bool IsIssued,
    bool IsLocked,
    Guid? LockedByUserId,
    Guid CreatedBy,
    Guid? ModifiedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
