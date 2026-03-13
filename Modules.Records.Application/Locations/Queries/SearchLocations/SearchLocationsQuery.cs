using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Locations.Queries.SearchLocations;

/// <summary>
/// Flexible MLI search. All filter parameters are optional — provide any combination.
/// Address search uses case-insensitive contains on StreetAddress, City, and CommonPlaceName.
/// </summary>
public sealed record SearchLocationsQuery(
    string? AddressContains   = null,
    string? City              = null,
    Guid?   StateId           = null,
    string? Zip               = null,
    string? CommonPlaceName   = null) : IRequest<IReadOnlyList<LocationDto>>;
