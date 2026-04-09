using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Locations.Commands.CreateLocation;
using Modules.Records.Application.Locations;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.TestData;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.Commands.GenerateTestArrests;

public sealed class GenerateTestArrestsHandler : IRequestHandler<GenerateTestArrestsCommand, GenerateTestArrestsResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly ISender _sender;
    private readonly IGoogleMapsPlacesClient _placesClient;
    private readonly IModificationContext _modificationContext;

    public GenerateTestArrestsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        ISender sender,
        IGoogleMapsPlacesClient placesClient,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _sender = sender;
        _placesClient = placesClient;
        _modificationContext = modificationContext;
    }

    public async Task<GenerateTestArrestsResult> Handle(GenerateTestArrestsCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var count = Math.Clamp(request.Count, 1, 250);
        var errors = new List<string>();

        if (request.ArrestedTo < request.ArrestedFrom)
        {
            errors.Add("The arrest time frame end must be on or after the start.");
            return CreateFailureResult(count, errors);
        }

        if (request.NameMaxUses < 1)
        {
            errors.Add("Name max uses must be at least 1.");
            return CreateFailureResult(count, errors);
        }

        if (request.LocationMaxUses < 1)
        {
            errors.Add("Location max uses must be at least 1.");
            return CreateFailureResult(count, errors);
        }

        var existingNameIds = new List<Guid>();
        if (request.NameStrategy == TestDataRecordLinkStrategy.Existing)
        {
            existingNameIds = await _dbContext.Names
                .AsNoTracking()
                .Where(n => n.JurisdictionId == jurisdictionId)
                .Select(n => n.Id)
                .ToListAsync(cancellationToken);
        }

        var existingLocationIds = new List<Guid>();
        if (request.LocationStrategy == TestDataRecordLinkStrategy.Existing)
        {
            existingLocationIds = await _dbContext.Locations
                .AsNoTracking()
                .Where(l => l.JurisdictionId == jurisdictionId)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);
        }

        var stateIds = await LoadPicklistIdsAsync(PicklistTypes.State, jurisdictionId, agencyId, cancellationToken);
        var hairColorIds = await LoadPicklistIdsAsync(PicklistTypes.HairColor, jurisdictionId, agencyId, cancellationToken);
        var eyeColorIds = await LoadPicklistIdsAsync(PicklistTypes.EyeColor, jurisdictionId, agencyId, cancellationToken);
        var directionDict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var streetTypeDict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var stateDict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var countryDict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var searchedPlaces = Array.Empty<GooglePlaceResult>();

        if (request.NameStrategy == TestDataRecordLinkStrategy.Existing && existingNameIds.Count == 0)
        {
            errors.Add("No existing Name records were found for this jurisdiction.");
            return CreateFailureResult(count, errors);
        }

        if (request.LocationStrategy == TestDataRecordLinkStrategy.Existing && existingLocationIds.Count == 0)
        {
            errors.Add("No existing Location records were found for this jurisdiction.");
            return CreateFailureResult(count, errors);
        }

        if (request.LocationStrategy != TestDataRecordLinkStrategy.Existing)
        {
            if (string.IsNullOrWhiteSpace(request.LocationKeyword))
            {
                errors.Add("A location search keyword is required when arrest seeding needs to create new locations.");
                return CreateFailureResult(count, errors);
            }

            if (string.IsNullOrWhiteSpace(request.LocationApiKey))
            {
                errors.Add("A Google Maps API key is required when arrest seeding needs to create new locations.");
                return CreateFailureResult(count, errors);
            }

            directionDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.Direction, jurisdictionId, agencyId, cancellationToken);
            streetTypeDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.StreetType, jurisdictionId, agencyId, cancellationToken);
            stateDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.State, jurisdictionId, agencyId, cancellationToken);
            countryDict = await LocationSeedPlaceCommandFactory.LoadPicklistAsync(_dbContext, PicklistTypes.Country, jurisdictionId, agencyId, cancellationToken);

            searchedPlaces = (await _placesClient.SearchAsync(
                request.LocationKeyword,
                Math.Min(count, 60),
                request.LocationApiKey,
                cancellationToken)).ToArray();

            if (searchedPlaces.Length == 0)
            {
                errors.Add($"No Google Maps locations were found for keyword '{request.LocationKeyword}'.");
                return CreateFailureResult(count, errors);
            }
        }

        var currentRunNameIds = new List<Guid>();
        var currentRunLocationIds = new List<Guid>();
        var nameUsageCounts = new Dictionary<Guid, int>();
        var locationUsageCounts = new Dictionary<Guid, int>();

        var created = 0;
        var failed = 0;
        var namesCreated = 0;
        var namesReusedFromExisting = 0;
        var namesReusedFromCurrentRun = 0;
        var locationsCreated = 0;
        var locationsReusedFromExisting = 0;
        var locationsReusedFromCurrentRun = 0;

        for (var index = 0; index < count; index++)
        {
            try
            {
                var resolvedName = await ResolveNameAsync(
                    request.NameStrategy,
                    existingNameIds,
                    currentRunNameIds,
                    nameUsageCounts,
                    request.NameMaxUses,
                    stateIds,
                    hairColorIds,
                    eyeColorIds,
                    jurisdictionId,
                    agencyId,
                    cancellationToken);

                IncrementCounters(resolvedName, ref namesCreated, ref namesReusedFromExisting, ref namesReusedFromCurrentRun);

                var name = await _dbContext.Names
                    .FirstAsync(n => n.Id == resolvedName.Id && n.JurisdictionId == jurisdictionId, cancellationToken);

                var arrestLocation = await ResolveLocationAsync(
                    request.LocationStrategy,
                    existingLocationIds,
                    currentRunLocationIds,
                    locationUsageCounts,
                    request.LocationMaxUses,
                    searchedPlaces,
                    directionDict,
                    streetTypeDict,
                    stateDict,
                    countryDict,
                    jurisdictionId,
                    cancellationToken);

                IncrementCounters(arrestLocation, ref locationsCreated, ref locationsReusedFromExisting, ref locationsReusedFromCurrentRun);

                Guid? primaryLocationId = name.PrimaryLocationId;
                Guid? secondaryLocationId = name.SecondaryLocationId;

                if (!primaryLocationId.HasValue)
                {
                    var resolvedPrimaryLocation = await ResolveLocationAsync(
                        request.LocationStrategy,
                        existingLocationIds,
                        currentRunLocationIds,
                        locationUsageCounts,
                        request.LocationMaxUses,
                        searchedPlaces,
                        directionDict,
                        streetTypeDict,
                        stateDict,
                        countryDict,
                        jurisdictionId,
                        cancellationToken);

                    primaryLocationId = resolvedPrimaryLocation.Id;
                    IncrementCounters(resolvedPrimaryLocation, ref locationsCreated, ref locationsReusedFromExisting, ref locationsReusedFromCurrentRun);
                }

                if (!secondaryLocationId.HasValue)
                {
                    var resolvedSecondaryLocation = await ResolveLocationAsync(
                        request.LocationStrategy,
                        existingLocationIds,
                        currentRunLocationIds,
                        locationUsageCounts,
                        request.LocationMaxUses,
                        searchedPlaces,
                        directionDict,
                        streetTypeDict,
                        stateDict,
                        countryDict,
                        jurisdictionId,
                        cancellationToken);

                    secondaryLocationId = resolvedSecondaryLocation.Id;
                    IncrementCounters(resolvedSecondaryLocation, ref locationsCreated, ref locationsReusedFromExisting, ref locationsReusedFromCurrentRun);
                }

                await EnsureNameLocationsAsync(name, primaryLocationId, secondaryLocationId, cancellationToken);

                await _sender.Send(
                    new CreateArrestCommand(
                        NameId: resolvedName.Id,
                        ArrestedAt: GenerateArrestedAt(request.ArrestedFrom, request.ArrestedTo),
                        IncidentRecordNumbers: [],
                        LocationId: arrestLocation.Id),
                    cancellationToken);

                created++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Arrest {index + 1}: {ex.Message}");
            }
        }

        return new GenerateTestArrestsResult(
            created,
            failed,
            namesCreated,
            namesReusedFromExisting,
            namesReusedFromCurrentRun,
            locationsCreated,
            locationsReusedFromExisting,
            locationsReusedFromCurrentRun,
            errors);
    }

    private async Task<ResolvedRecord> ResolveNameAsync(
        TestDataRecordLinkStrategy strategy,
        IReadOnlyList<Guid> existingIds,
        List<Guid> currentRunIds,
        Dictionary<Guid, int> usageCounts,
        int maxUses,
        IReadOnlyList<Guid> stateIds,
        IReadOnlyList<Guid> hairColorIds,
        IReadOnlyList<Guid> eyeColorIds,
        Guid jurisdictionId,
        Guid agencyId,
        CancellationToken cancellationToken)
    {
        return strategy switch
        {
            TestDataRecordLinkStrategy.Existing => new ResolvedRecord(PickAvailable(existingIds, usageCounts, maxUses, "Name"), false, false),
            TestDataRecordLinkStrategy.CreateNew => new ResolvedRecord(
                await CreateNameAsync(currentRunIds, usageCounts, stateIds, hairColorIds, eyeColorIds, jurisdictionId, agencyId, cancellationToken),
                true,
                false),
            TestDataRecordLinkStrategy.RecentlyCreatedOrCreateNew => await ResolveCurrentRunOrCreateAsync(
                currentRunIds,
                usageCounts,
                maxUses,
                () => CreateNameAsync(currentRunIds, usageCounts, stateIds, hairColorIds, eyeColorIds, jurisdictionId, agencyId, cancellationToken)),
            _ => throw new InvalidOperationException($"Unsupported name strategy '{strategy}'.")
        };
    }

    private async Task<ResolvedRecord> ResolveLocationAsync(
        TestDataRecordLinkStrategy strategy,
        IReadOnlyList<Guid> existingIds,
        List<Guid> currentRunIds,
        Dictionary<Guid, int> usageCounts,
        int maxUses,
        IReadOnlyList<GooglePlaceResult> searchedPlaces,
        Dictionary<string, Guid> directionDict,
        Dictionary<string, Guid> streetTypeDict,
        Dictionary<string, Guid> stateDict,
        Dictionary<string, Guid> countryDict,
        Guid jurisdictionId,
        CancellationToken cancellationToken)
    {
        return strategy switch
        {
            TestDataRecordLinkStrategy.Existing => new ResolvedRecord(PickAvailable(existingIds, usageCounts, maxUses, "Location"), false, false),
            TestDataRecordLinkStrategy.CreateNew => new ResolvedRecord(
                await CreateLocationAsync(currentRunIds, usageCounts, searchedPlaces, directionDict, streetTypeDict, stateDict, countryDict, jurisdictionId, cancellationToken),
                true,
                false),
            TestDataRecordLinkStrategy.RecentlyCreatedOrCreateNew => await ResolveCurrentRunOrCreateAsync(
                currentRunIds,
                usageCounts,
                maxUses,
                () => CreateLocationAsync(currentRunIds, usageCounts, searchedPlaces, directionDict, streetTypeDict, stateDict, countryDict, jurisdictionId, cancellationToken)),
            _ => throw new InvalidOperationException($"Unsupported location strategy '{strategy}'.")
        };
    }

    private static Guid PickAvailable(
        IReadOnlyList<Guid> ids,
        Dictionary<Guid, int> usageCounts,
        int maxUses,
        string label)
    {
        var availableIds = ids
            .Where(id => usageCounts.GetValueOrDefault(id) < maxUses)
            .ToList();

        if (availableIds.Count == 0)
        {
            throw new InvalidOperationException($"No {label} records remain under the configured max-uses limit.");
        }

        var selectedId = availableIds[Random.Shared.Next(availableIds.Count)];
        usageCounts[selectedId] = usageCounts.GetValueOrDefault(selectedId) + 1;
        return selectedId;
    }

    private static Task<ResolvedRecord> ResolveCurrentRunOrCreateAsync(
        List<Guid> currentRunIds,
        Dictionary<Guid, int> usageCounts,
        int maxUses,
        Func<Task<Guid>> createAsync)
    {
        var reusableIds = currentRunIds
            .Where(id => usageCounts.GetValueOrDefault(id) < maxUses)
            .ToList();

        if (reusableIds.Count > 0)
        {
            var reusedId = reusableIds[Random.Shared.Next(reusableIds.Count)];
            usageCounts[reusedId] = usageCounts.GetValueOrDefault(reusedId) + 1;
            return Task.FromResult(new ResolvedRecord(reusedId, false, true));
        }

        return CreateNewResolvedAsync(createAsync);
    }

    private static async Task<ResolvedRecord> CreateNewResolvedAsync(Func<Task<Guid>> createAsync)
        => new(await createAsync(), true, false);

    private async Task<Guid> CreateNameAsync(
        ICollection<Guid> currentRunIds,
        Dictionary<Guid, int> usageCounts,
        IReadOnlyList<Guid> stateIds,
        IReadOnlyList<Guid> hairColorIds,
        IReadOnlyList<Guid> eyeColorIds,
        Guid jurisdictionId,
        Guid agencyId,
        CancellationToken cancellationToken)
    {
        var sexValue = TestDataFakeNameGenerator.GenerateSex();
        var raceValue = TestDataFakeNameGenerator.GenerateRaceValue();
        var dob = TestDataFakeNameGenerator.GenerateDob();
        var age = TestDataFakeNameGenerator.GetAge(dob);

        var sexId = await LookupPicklistIdAsync(PicklistTypes.Sex, sexValue, jurisdictionId, agencyId, cancellationToken);
        var raceId = await LookupPicklistIdAsync(PicklistTypes.Race, raceValue, jurisdictionId, agencyId, cancellationToken);
        var hairColorId = TestDataFakeNameGenerator.PickRandom(hairColorIds);
        var eyeColorId = TestDataFakeNameGenerator.PickRandom(eyeColorIds);

        string? dlNumber = null;
        Guid? dlStateId = null;
        if (age >= 17)
        {
            dlNumber = TestDataFakeNameGenerator.GenerateDlNumber();
            dlStateId = TestDataFakeNameGenerator.PickRandom(stateIds);
        }

        var recordNumber = await _sender.Send(
            new CreateNameCommand(
                NameType: NameTypes.Person,
                LastOrBusinessName: TestDataFakeNameGenerator.PickLastName(),
                FirstName: TestDataFakeNameGenerator.PickFirstName(sexValue),
                MiddleName: TestDataFakeNameGenerator.PickMiddleName(),
                SexId: sexId,
                RaceId: raceId,
                DateOfBirth: dob,
                DriversLicenseNumber: dlNumber,
                DriversLicenseStateId: dlStateId,
                HeightInches: TestDataFakeNameGenerator.GenerateHeight(sexValue),
                WeightLbs: TestDataFakeNameGenerator.GenerateWeight(sexValue),
                HairColorId: hairColorId,
                EyeColorId: eyeColorId,
                SuffixId: null,
                PlaceOfBirth: null,
                FbiNumber: null,
                LocalNumber: null,
                SocialSecurityNumber: null,
                IsCitizen: false,
                DeceasedDate: null,
                PrimaryPhone: null,
                PrimaryPhoneExtension: null,
                WorkPhone: null,
                WorkPhoneExtension: null,
                OtherPhone: null,
                OtherPhoneExtension: null),
            cancellationToken);

        var nameId = await _dbContext.Names
            .AsNoTracking()
            .Where(n => n.JurisdictionId == jurisdictionId && n.RecordNumber == recordNumber)
            .Select(n => n.Id)
            .SingleAsync(cancellationToken);

        currentRunIds.Add(nameId);
        usageCounts[nameId] = usageCounts.GetValueOrDefault(nameId) + 1;
        return nameId;
    }

    private async Task<Guid> CreateLocationAsync(
        ICollection<Guid> currentRunIds,
        Dictionary<Guid, int> usageCounts,
        IReadOnlyList<GooglePlaceResult> searchedPlaces,
        Dictionary<string, Guid> directionDict,
        Dictionary<string, Guid> streetTypeDict,
        Dictionary<string, Guid> stateDict,
        Dictionary<string, Guid> countryDict,
        Guid jurisdictionId,
        CancellationToken cancellationToken)
    {
        var place = searchedPlaces[Random.Shared.Next(searchedPlaces.Count)];

        var recordNumber = await _sender.Send(
            LocationSeedPlaceCommandFactory.BuildCreateLocationCommand(
                place,
                directionDict,
                streetTypeDict,
                stateDict,
                countryDict),
            cancellationToken);

        var locationId = await _dbContext.Locations
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jurisdictionId && l.RecordNumber == recordNumber)
            .Select(l => l.Id)
            .SingleAsync(cancellationToken);

        currentRunIds.Add(locationId);
        usageCounts[locationId] = usageCounts.GetValueOrDefault(locationId) + 1;
        return locationId;
    }

    private async Task<List<Guid>> LoadPicklistIdsAsync(string picklistType, Guid jurisdictionId, Guid agencyId, CancellationToken cancellationToken)
    {
        return await _dbContext.PicklistItems
            .AsNoTracking()
            .Where(p => p.JurisdictionId == jurisdictionId && p.AgencyId == agencyId && p.PicklistType == picklistType && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task<Guid?> LookupPicklistIdAsync(
        string picklistType,
        string value,
        Guid jurisdictionId,
        Guid agencyId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.PicklistItems
            .AsNoTracking()
            .Where(p => p.JurisdictionId == jurisdictionId && p.AgencyId == agencyId && p.PicklistType == picklistType && p.Value == value && p.IsActive)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task EnsureNameLocationsAsync(
        Name name,
        Guid? primaryLocationId,
        Guid? secondaryLocationId,
        CancellationToken cancellationToken)
    {
        if (name.PrimaryLocationId == primaryLocationId && name.SecondaryLocationId == secondaryLocationId)
        {
            return;
        }

        name.SetLocations(primaryLocationId, secondaryLocationId, _modificationContext);
        name.UpdateDetails(
            name.NameType,
            name.LastOrBusinessName,
            name.FirstName,
            name.MiddleName,
            name.SexId,
            name.RaceId,
            name.DateOfBirth,
            name.DriversLicenseNumber,
            name.DriversLicenseStateId,
            name.HeightInches,
            name.WeightLbs,
            name.HairColorId,
            name.EyeColorId,
            name.SuffixId,
            name.PlaceOfBirth,
            name.FbiNumber,
            name.LocalNumber,
            name.SocialSecurityNumber,
            name.IsCitizen,
            name.DeceasedDate,
            _modificationContext,
            name.PrimaryPhone,
            name.PrimaryPhoneExtension,
            name.WorkPhone,
            name.WorkPhoneExtension,
            name.OtherPhone,
            name.OtherPhoneExtension);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTime GenerateArrestedAt(DateTime from, DateTime to)
    {
        if (from == to)
        {
            return from;
        }

        var totalSeconds = (to - from).TotalSeconds;
        var offsetSeconds = Random.Shared.NextDouble() * totalSeconds;
        return from.AddSeconds(offsetSeconds);
    }

    private static void IncrementCounters(
        ResolvedRecord resolvedRecord,
        ref int created,
        ref int reusedFromExisting,
        ref int reusedFromCurrentRun)
    {
        if (resolvedRecord.Created)
        {
            created++;
        }
        else if (resolvedRecord.ReusedFromCurrentRun)
        {
            reusedFromCurrentRun++;
        }
        else
        {
            reusedFromExisting++;
        }
    }

    private static GenerateTestArrestsResult CreateFailureResult(int count, IReadOnlyList<string> errors)
        => new(0, count, 0, 0, 0, 0, 0, 0, errors);

    private sealed record ResolvedRecord(Guid Id, bool Created, bool ReusedFromCurrentRun);
}
