namespace Modules.Records.Domain.DomainEvents;

public sealed record ChargeDeletedDomainEvent(Guid ChargeId, Guid DeletedByUserId) : DomainEvent;
