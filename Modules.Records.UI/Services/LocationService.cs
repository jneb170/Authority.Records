using MediatR;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Locations.Commands.AcquireLocationLock;
using Modules.Records.Application.Locations.Commands.CreateLocation;
using Modules.Records.Application.Locations.Commands.ReleaseLocationLock;
using Modules.Records.Application.Locations.Commands.RenewLocationLock;
using Modules.Records.Application.Locations.Commands.RestoreLocation;
using Modules.Records.Application.Locations.Commands.SoftDeleteLocation;
using Modules.Records.Application.Locations.Commands.UpdateLocationDetails;
using Modules.Records.Application.Locations.Queries.GetLocationById;
using Modules.Records.Application.Locations.Queries.GetLocationByRecordNumber;
using Modules.Records.Application.Locations.Queries.GetLocationsByJurisdiction;
using Modules.Records.Application.Locations.Queries.SearchLocations;

namespace Modules.Records.UI.Services;

public sealed class LocationService : ILocationService
{
    private readonly ISender _sender;

    public LocationService(ISender sender) => _sender = sender;

    public Task<LocationDto?> GetByIdAsync(Guid id) =>
        _sender.Send(new GetLocationByIdQuery(id));

    public Task<LocationDto?> GetByRecordNumberAsync(long recordNumber) =>
        _sender.Send(new GetLocationByRecordNumberQuery(recordNumber));

    public Task<IReadOnlyList<LocationDto>> GetByJurisdictionAsync() =>
        _sender.Send(new GetLocationsByJurisdictionQuery());

    public Task<IReadOnlyList<LocationDto>> SearchAsync(
        string? addressContains = null,
        string? city            = null,
        Guid?   stateId         = null,
        string? zip             = null,
        string? commonPlaceName = null) =>
        _sender.Send(new SearchLocationsQuery(addressContains, city, stateId, zip, commonPlaceName));

    public Task<long> CreateAsync(
        string  streetAddress,
        string  city,
        string? streetNumber    = null,
        Guid?   preDirectionId  = null,
        Guid?   streetTypeId    = null,
        Guid?   postDirectionId = null,
        Guid?   stateId         = null,
        Guid?   countryId       = null,
        string? zip             = null,
        string? aptSuite        = null,
        string? coordinates     = null,
        string? commonPlaceName = null,
        string? comments        = null,
        string? address         = null) =>
        _sender.Send(new CreateLocationCommand(
            streetAddress, city, streetNumber,
            preDirectionId, streetTypeId, postDirectionId,
            stateId, countryId, zip, aptSuite,
            coordinates, commonPlaceName, comments, address));

    public Task UpdateDetailsAsync(
        Guid    locationId,
        string  streetAddress,
        string  city,
        string? streetNumber    = null,
        Guid?   preDirectionId  = null,
        Guid?   streetTypeId    = null,
        Guid?   postDirectionId = null,
        Guid?   stateId         = null,
        Guid?   countryId       = null,
        string? zip             = null,
        string? aptSuite        = null,
        string? coordinates     = null,
        string? commonPlaceName = null,
        string? comments        = null,
        string? address         = null) =>
        _sender.Send(new UpdateLocationDetailsCommand(
            locationId, streetAddress, city, streetNumber,
            preDirectionId, streetTypeId, postDirectionId,
            stateId, countryId, zip, aptSuite,
            coordinates, commonPlaceName, comments, address));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireLocationLockCommand(id));

    public Task RenewLockAsync(Guid id) =>
        _sender.Send(new RenewLocationLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseLocationLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteLocationCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreLocationCommand(id));
}
