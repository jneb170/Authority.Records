using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LocationDetailsUpdatedDomainEvent(
    Guid LocationId,
    string? StreetNumber,
    Guid? PreDirectionId,
    string StreetAddress,
    Guid? StreetTypeId,
    Guid? PostDirectionId,
    string City,
    Guid? StateId,
    Guid? CountryId,
    string? Zip,
    string? AptSuite,
    string? Coordinates,
    string? CommonPlaceName,
    string? Comments,
    string? Address) : DomainEvent;
