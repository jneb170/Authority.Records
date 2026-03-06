using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

public sealed class Incident
    : LockableAggregateRoot<Incident>, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string Description { get; private set; }

    // EF Core requires a parameterless constructor for materialization
    private Incident() { } // EF

    // -------------------------------------------------------
    // Aggregate construction via factory
    // -------------------------------------------------------
    internal Incident(Guid jurisdictionId, Guid agencyId, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("incident.description.empty", "Description cannot be empty.");

        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        Description = description;

        Status = RecordStatus.Draft;

        AddDomainEvent(new IncidentCreatedDomainEvent(Id, Guid.Empty));
    }

    // -------------------------------------------------------
    // State transition methods
    // -------------------------------------------------------
    public void Open(
        IModificationContext context,
        ILifecyclePolicy<Incident> lifecyclePolicy)
        => ChangeStatus(RecordStatus.Open, context, lifecyclePolicy);
    public void Close(
        IModificationContext context,
        ILifecyclePolicy<Incident> lifecyclePolicy,
        bool force = false)
        => ChangeStatus(RecordStatus.Closed, context, lifecyclePolicy, force);
    public void Archive(
        IModificationContext context,
        ILifecyclePolicy<Incident> lifecyclePolicy)
        => ChangeStatus(RecordStatus.Archived, context, lifecyclePolicy);

    // -------------------------------------------------------
    // Behavior methods
    // -------------------------------------------------------
    public void UpdateDescription(string description, IModificationContext context)
    {
        EnsureCanModify(context);

        if (Status == RecordStatus.Draft)
        {
            // Draft mode, no lock required
        }
        else
        {
            EnsureUserOwnsLock(context.UserId);
        }

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("incident.description.empty", "Description cannot be empty.");

        Description = description;
    }

    // -------------------------------------------------------
    // Soft delete overrides
    // -------------------------------------------------------
    public override void SoftDelete(Guid userId)
    {
        base.SoftDelete(userId);
        AddDomainEvent(new IncidentSoftDeletedDomainEvent(Id, userId));
    }

    public override void Restore(Guid userId)
    {
        base.Restore(userId);
        AddDomainEvent(new IncidentRestoredDomainEvent(Id, userId));
    }

    // -------------------------------------------------------
    // Authorization and locking strategies
    // -------------------------------------------------------
    private static readonly IncidentAuthorizationPolicy _authorizationPolicy
        = new();
    protected override IAuthorizationPolicy<Incident> AuthorizationPolicy
        => _authorizationPolicy;

    private static readonly TimeoutLockExpirationStrategy<Incident> _lockExpirationStrategy
        = new();
    protected override ILockExpirationStrategy<Incident> LockExpirationStrategy
        => _lockExpirationStrategy;

    private static readonly SystemClock _clock
        = new();
    protected override IClock Clock
        => _clock;

    // -------------------------------------------------------
    // Child collections  
    // -------------------------------------------------------
    private readonly List<Arrest> _arrests = new();
    public IReadOnlyCollection<Arrest> Arrests => _arrests.AsReadOnly();

    private readonly List<Citation> _citations = new();
    public IReadOnlyCollection<Citation> Citations => _citations.AsReadOnly();


    // -------------------------------------------------------
    // Locking Overrides to enforce additional rules if needed
    // -------------------------------------------------------
    public override void AcquireLock(IModificationContext context, TimeSpan lockTimeout)
    {
        // Override lock aquire to enforce additional rules if needed
        base.AcquireLock(context, lockTimeout);
        // Additional checks can be added here if needed
    }
    
    protected override void EnsureCanModify(IModificationContext context)
    {
        // Override lock release to enforce additional rules if needed
        base.EnsureCanModify(context);

        // Additional checks can be added here if needed
    }

    protected override void EnsureCanLock(IModificationContext context)
    {
        // Override lock to enforce additional rules if needed
        base.EnsureCanLock(context);

        // Additional checks can be added here if needed
    }


    // -------------------------------------------------------
    // Methods to manage child entities
    // -------------------------------------------------------
    public void AddArrest(Arrest arrest, IModificationContext context)
    {
        EnsureCanModify(context);
        _arrests.Add(arrest);
    }

    public void AddCitation(Citation citation, IModificationContext context)
    {
        EnsureCanModify(context);
        _citations.Add(citation);
    }
}


