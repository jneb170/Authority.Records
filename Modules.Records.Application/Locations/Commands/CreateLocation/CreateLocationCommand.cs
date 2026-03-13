using MediatR;

namespace Modules.Records.Application.Locations.Commands.CreateLocation;

public sealed record CreateLocationCommand(
    string StreetAddress,
    string City,
    string? StreetNumber = null,
    Guid? PreDirectionId = null,
    Guid? StreetTypeId = null,
    Guid? PostDirectionId = null,
    Guid? StateId = null,
    Guid? CountryId = null,
    string? Zip = null,
    string? AptSuite = null,
    string? Coordinates = null,
    string? CommonPlaceName = null,
    string? Comments = null) : IRequest<long>;
