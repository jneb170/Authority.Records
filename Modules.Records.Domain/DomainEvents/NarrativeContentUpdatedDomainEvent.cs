namespace Modules.Records.Domain.DomainEvents;

/// <summary>
/// Raised when a narrative's title/content changes. Carries only the title (small);
/// the projection re-reads the full content from the aggregate so large narrative
/// bodies don't bloat the outbox/audit payload.
/// </summary>
public sealed record NarrativeContentUpdatedDomainEvent(
    Guid NarrativeId,
    string Title) : DomainEvent;
