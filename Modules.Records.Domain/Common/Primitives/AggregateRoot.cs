using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Common.Primitives;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    public long Version { get; private set; }

    // Audit fields — set automatically by AuditInterceptor; never set directly by domain code.
    public DateTime CreatedAt    { get; private set; }
    public Guid     CreatedBy    { get; private set; }
    public DateTime? ModifiedAt  { get; private set; }
    public Guid?    ModifiedBy   { get; private set; }

    // Called by AuditInterceptor only.
    internal void SetCreatedAudit(Guid userId, DateTime utcNow)
    {
        CreatedAt  = utcNow;
        CreatedBy  = userId;
        ModifiedAt = utcNow;
        ModifiedBy = userId;
    }

    internal void SetModifiedAudit(Guid userId, DateTime utcNow)
    {
        ModifiedAt = utcNow;
        ModifiedBy = userId;
    }

    protected void AddDomainEvent(IDomainEvent @event)
    {
        Version++;
        if (@event is DomainEvent domainEvent)
        {
            domainEvent.AggregateId = Id;
            domainEvent.AggregateVersion = Version;
        }
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    // Soft delete support
    public bool IsDeleted { get; protected set; }

    // Concurrency token for optimistic locking
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();


    public virtual void SoftDelete(Guid userId)
    {
        if (!IsDeleted)
            IsDeleted = true;
    }

    public virtual void Restore(Guid userId)
    {
        if (IsDeleted)
            IsDeleted = false;
    }
}
