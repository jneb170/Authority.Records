using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Common.Primitives;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    public long Version { get; private set; }

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
