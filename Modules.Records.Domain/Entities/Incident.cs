using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modules.Records.Domain.Entities;

public sealed class Incident
    : LockableAggregateRoot<Incident>, IMultiTenant, IAgencyOwned
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }

    /// <summary>DB-generated auto-increment number. Use this in URLs and display; the GUID is for internal identity.</summary>
    public long RecordNumber { get; private set; }

    /// <summary>Optional reference to a Master Location Index record for the incident location.</summary>
    public Guid? LocationId { get; private set; }

    /// <summary>Date and time the incident occurred (distinct from when the record was created).</summary>
    public DateTime? OccurredOn { get; private set; }

    // Flat EF-mapped columns— kept separate so no migration is needed
    public string IncidentNum { get; private set; } = string.Empty;
    public string LocalNum { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CFSNum      { get; private set; } = string.Empty;

    /// <summary>Computed VO that groups editable fields. Not stored in DB.</summary>
    [NotMapped]
    public IncidentDetails Details => new() {
        IncidentNum = IncidentNum, 
        LocalNum = LocalNum,
        Description = Description, 
        CFSNum = CFSNum };

    // EF Core requires a parameterless constructor for materialization
    private Incident() { }

    // -------------------------------------------------------
    // Aggregate construction via factory
    // -------------------------------------------------------
    internal Incident(CreateIncidentRequest request)
    {
        request.Details.Validate();

        Id             = Guid.NewGuid();
        JurisdictionId = request.JurisdictionId;
        AgencyId       = request.AgencyId;
        IncidentNum    = request.Details.IncidentNum;
        LocalNum       = request.Details.LocalNum;
        Description    = request.Details.Description;
        CFSNum         = request.Details.CFSNum;

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

    /// <summary>
    /// Updates all editable details in one call.
    /// Add new fields to <see cref="IncidentDetails"/> — this method never changes signature.
    /// </summary>
    public void UpdateDetails(IncidentDetails details, DateTime? occurredOn, IModificationContext context)
    {
        details.Validate();
        EnsureCanModify(context);

        if (Status != RecordStatus.Draft)
            EnsureUserOwnsLock(context.UserId);

        Description = details.Description;
        CFSNum      = details.CFSNum;
        IncidentNum = details.IncidentNum;
        LocalNum    = details.LocalNum;
        OccurredOn  = occurredOn;

        AddDomainEvent(new IncidentDetailsUpdatedDomainEvent(Id, Details, OccurredOn, LocationId, context.UserId));
    }

    /// <summary>Sets or clears the linked Master Location Index record for this incident.</summary>
    public void SetLocation(Guid? locationId, IModificationContext context)
    {
        EnsureCanModify(context);

        if (Status != RecordStatus.Draft)
            EnsureUserOwnsLock(context.UserId);

        LocationId = locationId;
    }

    // -------------------------------------------------------
    // Delete (issue #122)
    // -------------------------------------------------------

    /// <summary>
    /// Deletes a draft incident. Once an incident is opened it is part of the record and can't
    /// be deleted. Deleting also respects another user's active lock, as editing does, so a
    /// record can't disappear from under someone who is working on it.
    /// </summary>
    public void Delete(IModificationContext context, TimeSpan lockTimeout)
    {
        if (IsDeleted)
            return;

        if (Status != RecordStatus.Draft)
        {
            throw new DomainException(
                "incident.delete.not-draft",
                "Only draft incidents can be deleted. Open, closed and archived incidents are part of the record.");
        }

        if (IsLocked &&
            LockedByUserId != context.UserId &&
            !context.CanOverrideLocks &&
            LockExpirationStrategy.IsLockActive(this, lockTimeout, Clock))
        {
            throw new DomainException(
                "record.locked",
                "This incident is being edited by another user. It can't be deleted until they release it.");
        }

        // Don't leave the deleter's own lock behind on the deleted record.
        if (IsLocked && LockedByUserId == context.UserId)
            ReleaseLock(context);

        SoftDelete(context.UserId);
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
    // Locking Overrides to enforce additional rules if needed
    // -------------------------------------------------------
    public override void AcquireLock(IModificationContext context, TimeSpan lockTimeout)
    {
        base.AcquireLock(context, lockTimeout);
    }
    
    protected override void EnsureCanModify(IModificationContext context)
    {
        base.EnsureCanModify(context);
    }

    protected override void EnsureCanLock(IModificationContext context)
    {
        base.EnsureCanLock(context);
    }


}
