namespace Modules.Records.Domain.DomainEvents;

public sealed record NarrativeCreatedDomainEvent(
    Guid NarrativeId,
    Guid JurisdictionId,
    string Title) : DomainEvent;
