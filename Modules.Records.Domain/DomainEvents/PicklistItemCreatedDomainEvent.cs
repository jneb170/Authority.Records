namespace Modules.Records.Domain.DomainEvents;

public sealed record PicklistItemCreatedDomainEvent(
    Guid PicklistItemId,
    Guid JurisdictionId,
    Guid AgencyId,
    string PicklistType,
    string Value,
    string Label)
    : DomainEvent;
