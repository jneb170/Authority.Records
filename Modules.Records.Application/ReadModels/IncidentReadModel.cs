using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.ReadModels;

public sealed class IncidentReadModel
{
    public Guid    Id             { get; private set; }
    public long    RecordNumber   { get; private set; }
    public Guid    JurisdictionId { get; private set; }
    public Guid    AgencyId       { get; private set; }

    // Flat EF columns — kept for SQL filtering/sorting. Update these when adding a new field.
    public string IncidentNum { get; private set; } = string.Empty;
    public string LocalNum    { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CFSNum      { get; private set; } = string.Empty;

    public string Status          { get; private set; } = string.Empty;
    public bool   IsDeleted       { get; private set; }
    public bool   IsLocked        { get; private set; }
    public Guid?  LockedByUserId  { get; private set; }
    public int    ArrestCount     { get; private set; }
    public int    CitationCount   { get; private set; }
    public Guid   CreatedBy       { get; private set; }
    public Guid?  ModifiedBy      { get; private set; }
    public DateTime CreatedAtUtc  { get; private set; }
    public DateTime UpdatedAtUtc  { get; private set; }
    public Guid?  LocationId      { get; private set; }

    private IncidentReadModel() { } // EF

    public static IncidentReadModel Create(
        Guid id, long recordNumber, Guid jurisdictionId, Guid agencyId,
        IncidentDetails details,
        RecordStatus status, DateTime createdAtUtc, Guid createdBy)
    {
        return new IncidentReadModel
        {
            Id             = id,
            RecordNumber   = recordNumber,
            JurisdictionId = jurisdictionId,
            AgencyId       = agencyId,
            IncidentNum    = details.IncidentNum,
            LocalNum       = details.LocalNum,
            Description    = details.Description,
            CFSNum         = details.CFSNum,
            Status         = status.ToString(),
            IsDeleted      = false,
            IsLocked       = false,
            ArrestCount    = 0,
            CitationCount  = 0,
            CreatedBy      = createdBy,
            ModifiedBy     = null,
            CreatedAtUtc   = createdAtUtc,
            UpdatedAtUtc   = createdAtUtc
        };
    }

    /// <summary>
    /// Maps this read model to a DTO. Only update here when adding/removing fields.
    /// Both query handlers call this — they never need changing for new fields.
    /// </summary>
    public IncidentDto ToDto() => new(
        Id, RecordNumber, JurisdictionId, AgencyId,
        new IncidentDetails { 
            IncidentNum = IncidentNum, 
            LocalNum    = LocalNum,
            Description = Description, 
            CFSNum = CFSNum },
        Status, IsDeleted, IsLocked, LockedByUserId, ArrestCount, CitationCount,
        CreatedBy, ModifiedBy, CreatedAtUtc, UpdatedAtUtc, LocationId);

    public void ApplyDetailsChanged(IncidentDetails d) { 
        IncidentNum = d.IncidentNum; 
        LocalNum = d.LocalNum; 
        Description = d.Description; 
        CFSNum = d.CFSNum; 
        UpdatedAtUtc = DateTime.UtcNow; }

    public void ApplyModifiedAudit(Guid? modifiedBy) { ModifiedBy = modifiedBy; }

    public void ApplyStatusChange(string status)           { Status = status; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyDeleted()                             { IsDeleted = true;  UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyRestored()                            { IsDeleted = false; UpdatedAtUtc = DateTime.UtcNow; }
    public void IncrementArrestCount()                     { ArrestCount++; UpdatedAtUtc = DateTime.UtcNow; }
    public void DecrementArrestCount()                     { if (ArrestCount > 0) ArrestCount--; UpdatedAtUtc = DateTime.UtcNow; }
    public void IncrementCitationCount()                   { CitationCount++; UpdatedAtUtc = DateTime.UtcNow; }
    public void DecrementCitationCount()                   { if (CitationCount > 0) CitationCount--; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockAcquired(Guid userId)             { IsLocked = true;  LockedByUserId = userId; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockReleased()                        { IsLocked = false; LockedByUserId = null;   UpdatedAtUtc = DateTime.UtcNow; }
}
