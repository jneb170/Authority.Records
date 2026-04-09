using MediatR;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Locations;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Locations.Commands.GenerateTestLocations;

public sealed class GenerateTestLocationsHandler
    : IRequestHandler<GenerateTestLocationsCommand, GenerateTestLocationsResult>
{
    private readonly IGoogleMapsPlacesClient _placesClient;
    private readonly ISender                 _sender;
    private readonly IApplicationDbContext   _dbContext;
    private readonly ITenantProvider         _tenantProvider;

    public GenerateTestLocationsHandler(
        IGoogleMapsPlacesClient placesClient,
        ISender                 sender,
        IApplicationDbContext   dbContext,
        ITenantProvider         tenantProvider)
    {
        _placesClient   = placesClient;
        _sender         = sender;
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<GenerateTestLocationsResult> Handle(
        GenerateTestLocationsCommand request,
        CancellationToken            cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var directionDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.Direction, jurisdictionId, agencyId, cancellationToken);
        var streetTypeDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.StreetType, jurisdictionId, agencyId, cancellationToken);
        var stateDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.State, jurisdictionId, agencyId, cancellationToken);
        var countryDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.Country, jurisdictionId, agencyId, cancellationToken);

        var places = await _placesClient.SearchAsync(
            request.Keyword,
            Math.Clamp(request.Count, 1, 60),
            request.ApiKey,
            cancellationToken);

        int created = 0;
        int failed  = 0;
        var errors  = new List<string>();

        foreach (var place in places)
        {
            try
            {
                await _sender.Send(
                    LocationSeedPlaceCommandFactory.BuildCreateLocationCommand(
                        place,
                        directionDict,
                        streetTypeDict,
                        stateDict,
                        countryDict),
                    cancellationToken);

                created++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"{place.PlaceName}: {ex.Message}");
            }
        }

        return new GenerateTestLocationsResult(created, failed, errors);
    }
}
