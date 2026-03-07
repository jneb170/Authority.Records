namespace Modules.Records.Domain.DomainEvents;

public sealed record PicklistItemUpdatedDomainEvent(
    Guid PicklistItemId,
    string Label,
    int SortOrder)
    : DomainEvent;
