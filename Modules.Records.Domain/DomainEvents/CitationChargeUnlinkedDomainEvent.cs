namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationChargeUnlinkedDomainEvent(
    Guid LinkId,
    Guid CitationId,
    Guid ChargeId,
    Guid JurisdictionId,
    Guid UnlinkedByUserId) : DomainEvent;
