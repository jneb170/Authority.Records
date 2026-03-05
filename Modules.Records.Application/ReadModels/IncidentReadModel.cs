using Modules.Records.Domain.Common;

namespace Modules.Records.Application.ReadModels;

public sealed class IncidentReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public int ArrestCount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private IncidentReadModel() { } // EF

    public static IncidentReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid agencyId,
        string description,
        RecordStatus status,
        DateTime createdAtUtc)
    {
        return new IncidentReadModel
        {
            Id = id,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Description = description,
            Status = status.ToString(),
            IsDeleted = false,
            IsLocked = false,
            LockedByUserId = null,
            ArrestCount = 0,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public void ApplyStatusChange(string status) { Status = status; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyDeleted() { IsDeleted = true; UpdatedAtUtc = DateTime.UtcNow; }
    public void ApplyRestored() { IsDeleted = false; UpdatedAtUtc = DateTime.UtcNow; }
    public void IncrementArrestCount() { ArrestCount++; UpdatedAtUtc = DateTime.UtcNow; }
}
