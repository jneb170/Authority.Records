using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.ReadModels;

public sealed class CitationReadModel
{
    public Guid Id { get; private set; }
    public long RecordNumber { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public bool IsIssued { get; private set; }
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public Guid   CreatedBy     { get; private set; }
    public Guid?  ModifiedBy    { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? CourtId { get; private set; }
    public string CitationNum { get; private set; } = string.Empty;
    public Guid? LocationId { get; private set; }

    private CitationReadModel() { } // EF

    public static CitationReadModel Create(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        Guid agencyId,
        string description,
        DateTime issueDate,
        DateTime createdAtUtc,
        Guid createdBy,
        string citationNum = "")
    {
        return new CitationReadModel
        {
            Id = id,
            RecordNumber = recordNumber,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Description = description,
            IssueDate = issueDate,
            IsIssued = false,
            IsLocked = false,
            LockedByUserId = null,
            CreatedBy = createdBy,
            ModifiedBy = null,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            CitationNum = citationNum
        };
    }

    public void ApplyModifiedAudit(Guid? modifiedBy) { ModifiedBy = modifiedBy; }
    public void ApplyLocationChanged(Guid? locationId) { LocationId = locationId; UpdatedAtUtc = DateTime.UtcNow; }

    public CitationDto ToDto() => new(
        Id, RecordNumber, JurisdictionId, AgencyId,
        Description, IssueDate, IsIssued, IsLocked, LockedByUserId,
        CreatedBy, ModifiedBy, CreatedAtUtc, UpdatedAtUtc, CourtId, CitationNum, LocationId);

    public void ApplyDetailsChanged(string description, DateTime issueDate, Guid? courtId, string citationNum)
    {
        Description  = description;
        IssueDate    = issueDate;
        CourtId      = courtId;
        CitationNum  = citationNum;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ApplyIssued() { IsIssued = true; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockAcquired(Guid userId) { IsLocked = true; LockedByUserId = userId; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockReleased() { IsLocked = false; LockedByUserId = null; UpdatedAtUtc = DateTime.UtcNow; }
}
