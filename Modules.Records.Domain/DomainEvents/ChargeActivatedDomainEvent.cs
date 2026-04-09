namespace Modules.Records.Domain.DomainEvents;

public sealed record ChargeActivatedDomainEvent(Guid ChargeId) : DomainEvent;
