namespace Modules.Records.Domain.DomainEvents;

public sealed record ChargeDeactivatedDomainEvent(Guid ChargeId) : DomainEvent;
