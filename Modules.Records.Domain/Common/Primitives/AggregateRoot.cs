using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Common.Primitives;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event)
        => _domainEvents.Add(@event);
    public void ClearDomainEvents()
        => _domainEvents.Clear();

    // Soft delete support
    public bool IsDeleted { get; protected set; }

    // Concurrency token for optimistic locking
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();


    public virtual void SoftDelete()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            // Optionally: add a domain event here
            // AddDomainEvent(new EntitySoftDeletedDomainEvent(this));
        }
    }

    public virtual void Restore()
    {
        if (IsDeleted)
        {
            IsDeleted = false;
            // Optionally: add a domain event here
            // AddDomainEvent(new EntityRestoredDomainEvent(this));
        }
    }
}
