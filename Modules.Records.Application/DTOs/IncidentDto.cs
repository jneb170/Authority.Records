using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.DTOs;

/// <summary>
/// Read-side projection of an Incident.
/// Editable fields live in <see cref="IncidentDetails"/> — this record never changes for new fields.
/// </summary>
public sealed record IncidentDto(
    Guid            Id,
    long            RecordNumber,
    Guid            JurisdictionId,
    Guid            AgencyId,
    IncidentDetails Details,
    string          Status,
    bool            IsDeleted,
    bool            IsLocked,
    Guid?           LockedByUserId,
    int             ArrestCount,
    int             CitationCount,
    Guid            CreatedBy,
    Guid?           ModifiedBy,
    DateTime        CreatedAtUtc,
    DateTime        UpdatedAtUtc);
