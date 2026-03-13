using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LocationCreatedDomainEvent(
    Guid LocationId,
    Guid JurisdictionId,
    string? CommonPlaceName,
    string StreetAddress,
    string City,
    Guid? StateId) : DomainEvent;
