using MediatR;


namespace Modules.Records.Domain.DomainEvents
{
    public interface IDomainEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredOnUtc { get; }
        Guid AggregateId { get; }
        long AggregateVersion { get; }
    }
}
