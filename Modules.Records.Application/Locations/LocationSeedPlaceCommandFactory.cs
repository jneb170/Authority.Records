using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Locations.Commands.CreateLocation;

namespace Modules.Records.Application.Locations;

internal static class LocationSeedPlaceCommandFactory
{
    public static async Task<Dictionary<string, Guid>> LoadPicklistAsync(
        IApplicationDbContext dbContext,
        string picklistType,
        Guid jurisdictionId,
        Guid agencyId,
        CancellationToken cancellationToken)
    {
        return await dbContext.PicklistItems
            .AsNoTracking()
            .Where(p => p.JurisdictionId == jurisdictionId
                     && p.AgencyId == agencyId
                     && p.PicklistType == picklistType
                     && p.IsActive)
            .ToDictionaryAsync(
                p => p.Value,
                p => p.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);
    }

    public static CreateLocationCommand BuildCreateLocationCommand(
        GooglePlaceResult place,
        Dictionary<string, Guid> directionDict,
        Dictionary<string, Guid> streetTypeDict,
        Dictionary<string, Guid> stateDict,
        Dictionary<string, Guid> countryDict)
    {
        var (preDir, streetName, postDir, streetType) = StreetParser.Parse(
            place.StreetAddress,
            directionDict,
            streetTypeDict);

        var stateId = Lookup(stateDict, place.StateAbbreviation);
        var countryId = Lookup(countryDict, place.CountryCode);

        string? coordinates = place.Lat.HasValue && place.Lng.HasValue
            ? $"{place.Lat.Value},{place.Lng.Value}"
            : null;

        return new CreateLocationCommand(
            StreetAddress: streetName ?? place.StreetAddress ?? place.FormattedAddress,
            City: place.City ?? "Unknown",
            StreetNumber: place.StreetNumber,
            AptSuite: place.AptSuite,
            PreDirectionId: preDir,
            StreetTypeId: streetType,
            PostDirectionId: postDir,
            StateId: stateId,
            CountryId: countryId,
            Zip: place.Zip,
            Coordinates: coordinates,
            CommonPlaceName: place.PlaceName,
            Address: place.FormattedAddress);
    }

    private static Guid? Lookup(Dictionary<string, Guid> dict, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return dict.TryGetValue(value, out var id) ? id : null;
    }
}
