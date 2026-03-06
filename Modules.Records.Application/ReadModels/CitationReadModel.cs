namespace Modules.Records.Application.ReadModels;

public sealed class CitationReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid IncidentId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public bool IsIssued { get; private set; }
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private CitationReadModel() { } // EF

    public static CitationReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid agencyId,
        Guid incidentId,
        string description,
        DateTime issueDate,
        DateTime createdAtUtc)
    {
        return new CitationReadModel
        {
            Id = id,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            IncidentId = incidentId,
            Description = description,
            IssueDate = issueDate,
            IsIssued = false,
            IsLocked = false,
            LockedByUserId = null,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public void ApplyIssued() { IsIssued = true; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockAcquired(Guid userId) { IsLocked = true; LockedByUserId = userId; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyLockReleased() { IsLocked = false; LockedByUserId = null; UpdatedAtUtc = DateTime.UtcNow; }
}
