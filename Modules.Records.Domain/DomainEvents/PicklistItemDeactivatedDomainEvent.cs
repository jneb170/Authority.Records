namespace Modules.Records.Domain.DomainEvents;

public sealed record PicklistItemDeactivatedDomainEvent(Guid PicklistItemId) : DomainEvent;
