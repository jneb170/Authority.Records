using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.DomainEvents;


namespace Modules.Records.Domain.Entities;

public enum IncidentStatus
{
    Draft = 0,
    Open = 1,
    Closed = 2,
    Archived = 3
}

public sealed class Incident : LockableAggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string Description { get; private set; }

    private Incident() { } // EF

    public Incident(Guid jurisdictionId, Guid agencyId, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("incident.description.empty", "Description cannot be empty.");

        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        Description = description;
    }

    public void UpdateDescription(string description, Guid userId)
    {
        EnsureCanModify(userId);

        if (Status == RecordStatus.Draft && !IsLocked)
        {
            // Create mode allowed
        }
        else
        {
            EnsureUserOwnsLock(userId);
        }

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("incident.description.empty", "Description cannot be empty.");

        Description = description;
    }

    protected override void EnsureCanModify(Guid userId)
    {
        base.EnsureCanModify(userId);
    }

    protected override void EnsureCanLock(Guid userId)
    {
        base.EnsureCanLock(userId);
    }

    // ----------------------------------------------------
    // Lifecycle
    // ----------------------------------------------------
    public void OpenIncident(Guid userId)
    {
        EnsureUserOwnsLock(userId);
        Open(userId);
    }
    public void CloseIncident(Guid userId)
    {
        EnsureUserOwnsLock(userId);
        Close(userId);
    }
    public void ForceCloseIncident(Guid supervisorId)
    {
        Close(supervisorId, true);
    }
    public void ArchiveIncident(Guid userId)
    {
        Archive(userId);
    }

    protected override void ValidateForClose()
    {
        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("incident.invalid", "Description required before closing.");
    }

    // ----------------------------------------------------
    // Domain Event Factories
    // ----------------------------------------------------
    protected override IDomainEvent CreateLockAcquiredEvent(Guid userId)
            => new IncidentLockAcquiredDomainEvent(Id, userId);

    protected override IDomainEvent CreateLockReleasedEvent(Guid userId)
        => new IncidentLockReleasedDomainEvent(Id, userId);

    protected override IDomainEvent CreateOpenedEvent(Guid userId)
            => new IncidentOpenedDomainEvent(Id, userId);

    protected override IDomainEvent CreateClosedEvent(Guid userId, bool forced)
        => new IncidentClosedDomainEvent(Id, userId, forced);

    protected override IDomainEvent CreateArchivedEvent(Guid userId)
        => new IncidentArchivedDomainEvent(Id, userId);    
}


