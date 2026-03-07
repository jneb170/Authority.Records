namespace Modules.Records.Domain.DomainEvents;

public sealed record PicklistItemActivatedDomainEvent(Guid PicklistItemId) : DomainEvent;
