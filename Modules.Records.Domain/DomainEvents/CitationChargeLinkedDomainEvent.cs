namespace Modules.Records.Domain.DomainEvents;

public sealed record CitationChargeLinkedDomainEvent(
    Guid LinkId,
    Guid CitationId,
    Guid ChargeId,
    Guid JurisdictionId,
    Guid LinkedByUserId) : DomainEvent;
