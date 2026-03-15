using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.ReadModels;

public sealed class ArrestReadModel
{
    public Guid Id { get; private set; }
    public long RecordNumber { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string SuspectName { get; private set; } = string.Empty;
    public DateTime ArrestedAt { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public Guid   CreatedBy     { get; private set; }
    public Guid?  ModifiedBy    { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? ArrestTypeId { get; private set; }
    public string ArrestNum { get; private set; } = string.Empty;
    public Guid? LocationId { get; private set; }
    public string? PrimaryMugshotUrl { get; private set; }

    private ArrestReadModel() { } // EF

    public static ArrestReadModel Create(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        Guid agencyId,
        string suspectName,
        DateTime arrestedAt,
        DateTime createdAtUtc,
        Guid createdBy,
        string arrestNum = "")
    {
        return new ArrestReadModel
        {
            Id = id,
            RecordNumber = recordNumber,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            SuspectName = suspectName,
            ArrestedAt = arrestedAt,
            Status = RecordStatus.Draft.ToString(),
            IsLocked = false,
            LockedByUserId = null,
            CreatedBy = createdBy,
            ModifiedBy = null,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            ArrestNum = arrestNum
        };
    }

    public void ApplyModifiedAudit(Guid? modifiedBy) { ModifiedBy = modifiedBy; }

    public ArrestDto ToDto() => new(
        Id, RecordNumber, JurisdictionId, AgencyId,
        SuspectName, ArrestedAt, Status, IsLocked, LockedByUserId,
        CreatedBy, ModifiedBy, CreatedAtUtc, UpdatedAtUtc, ArrestTypeId, ArrestNum, LocationId, PrimaryMugshotUrl);

    public void ApplyDetailsChanged(string suspectName, DateTime arrestedAt, Guid? arrestTypeId, string arrestNum)
    {
        SuspectName  = suspectName;
        ArrestedAt   = arrestedAt;
        ArrestTypeId = arrestTypeId;
        ArrestNum    = arrestNum;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ApplyLocationChanged(Guid? locationId) { LocationId = locationId; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyPrimaryMugshot(string? primaryMugshotUrl) { PrimaryMugshotUrl = primaryMugshotUrl; UpdatedAtUtc = DateTime.UtcNow; }

    public void ApplyStatusChange(string status) { Status = status; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockAcquired(Guid userId) { IsLocked = true; LockedByUserId = userId; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockReleased() { IsLocked = false; LockedByUserId = null; UpdatedAtUtc = DateTime.UtcNow; }
}
