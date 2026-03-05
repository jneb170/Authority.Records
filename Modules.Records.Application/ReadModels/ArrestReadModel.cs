using Modules.Records.Domain.Common;

namespace Modules.Records.Application.ReadModels;

public sealed class ArrestReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid IncidentId { get; private set; }
    public string SuspectName { get; private set; } = string.Empty;
    public DateTime ArrestedAt { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ArrestReadModel() { } // EF

    public static ArrestReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid agencyId,
        Guid incidentId,
        string suspectName,
        DateTime arrestedAt,
        DateTime createdAtUtc)
    {
        return new ArrestReadModel
        {
            Id = id,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            IncidentId = incidentId,
            SuspectName = suspectName,
            ArrestedAt = arrestedAt,
            Status = RecordStatus.Draft.ToString(),
            IsLocked = false,
            LockedByUserId = null,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public void ApplyStatusChange(string status) { Status = status; UpdatedAtUtc = DateTime.UtcNow; }
}
