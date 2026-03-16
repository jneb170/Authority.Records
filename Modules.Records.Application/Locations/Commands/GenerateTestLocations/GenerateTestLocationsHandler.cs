using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Locations.Commands.CreateLocation;
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

        var directionDict  = await LoadPicklistAsync(PicklistTypes.Direction,  jurisdictionId, cancellationToken);
        var streetTypeDict = await LoadPicklistAsync(PicklistTypes.StreetType, jurisdictionId, cancellationToken);
        var stateDict      = await LoadPicklistAsync(PicklistTypes.State,      jurisdictionId, cancellationToken);
        var countryDict    = await LoadPicklistAsync(PicklistTypes.Country,    jurisdictionId, cancellationToken);

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
                var (preDir, streetName, postDir, streetType) = ParseStreet(
                    place.StreetAddress, directionDict, streetTypeDict);

                var stateId   = Lookup(stateDict,   place.StateAbbreviation);
                var countryId = Lookup(countryDict,  place.CountryCode);

                string? coordinates = place.Lat.HasValue && place.Lng.HasValue
                    ? $"{place.Lat.Value},{place.Lng.Value}"
                    : null;

                await _sender.Send(new CreateLocationCommand(
                    StreetAddress:   streetName ?? place.StreetAddress ?? place.FormattedAddress,
                    City:            place.City           ?? "Unknown",
                    StreetNumber:    place.StreetNumber,
                    AptSuite:        place.AptSuite,
                    PreDirectionId:  preDir,
                    StreetTypeId:    streetType,
                    PostDirectionId: postDir,
                    StateId:         stateId,
                    CountryId:       countryId,
                    Zip:             place.Zip,
                    Coordinates:     coordinates,
                    CommonPlaceName: place.PlaceName,
                    Address:         place.FormattedAddress),
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

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Dictionary<string, Guid>> LoadPicklistAsync(
        string            picklistType,
        Guid              jurisdictionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.PicklistItems
            .AsNoTracking()
            .Where(p => p.JurisdictionId == jurisdictionId
                     && p.PicklistType   == picklistType
                     && p.IsActive)
            .ToDictionaryAsync(
                p => p.Value,
                p => p.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);
    }

    private static Guid? Lookup(Dictionary<string, Guid> dict, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return dict.TryGetValue(value, out var id) ? id : null;
    }

    /// <summary>
    /// Splits a Google Maps route string into its pre-direction, base street name,
    /// post-direction, and street-type components, resolving each to picklist IDs.
    /// Delegates to <see cref="StreetParser.Parse"/>.
    /// </summary>
    private static (Guid? preDir, string? streetName, Guid? postDir, Guid? streetType)
        ParseStreet(
            string?                    route,
            Dictionary<string, Guid>   directionDict,
            Dictionary<string, Guid>   streetTypeDict)
        => StreetParser.Parse(route, directionDict, streetTypeDict);
}
