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

public sealed class Incident 
    : LockableAggregateRoot<Incident>, IMultiTenant
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

    protected override void ValidateTransition(
        RecordStatus current,
        RecordStatus target,
        bool isForced)
    {
        if (current == RecordStatus.Draft && target == RecordStatus.Open)
            return;

        if (current == RecordStatus.Open && target == RecordStatus.Closed)
        {
            if (!isForced)
                ValidateForClose();
            return;
        }

        if (current == RecordStatus.Closed && target == RecordStatus.Archived)
            return;

        throw new DomainException(
            "incident.invalid.transition",
            $"Invalid transition from {current} to {target}");
    }

    private void ValidateForClose()
    {
        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("incident.invalid", "Description required before closing.");
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

    public override void AcquireLock(Guid userId, TimeSpan lockTimeout, bool isSupervisor = false)
    {
        base.AcquireLock(userId, lockTimeout, isSupervisor);

        // Additional checks can be added here if needed
    }

    protected override void EnsureCanModify(Guid userId, bool isSupervisor = false)
    {
        base.EnsureCanModify(userId, isSupervisor);

        // Additional checks can be added here if needed
    }

    protected override void EnsureCanLock(Guid userId)
    {
        base.EnsureCanLock(userId);

        // Additional checks can be added here if needed
    }

    
}


