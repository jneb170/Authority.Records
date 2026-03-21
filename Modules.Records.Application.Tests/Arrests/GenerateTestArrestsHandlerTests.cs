using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Arrests.Commands.GenerateTestArrests;
using Modules.Records.Application.Locations.Commands.CreateLocation;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Application.Tests.Arrests;

public sealed class GenerateTestArrestsHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingStrategies_ReusesExistingNameAndLocation()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingName = new Name(jurisdictionId, agencyId, NameTypes.Person, "Doe", "Jordan", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
        var existingLocation = new Location(jurisdictionId, "Main St", "Springfield", "100");

        db.Names.Add(existingName);
        db.Locations.Add(existingLocation);
        await db.SaveChangesAsync(CancellationToken.None);

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var handler = new GenerateTestArrestsHandler(
            db,
            tenantProvider,
            new TestSender(db, tenantProvider),
            new FakePlacesClient(),
            new UserModificationContext(userId));

        var result = await handler.Handle(
            new GenerateTestArrestsCommand(
                Count: 3,
                ArrestedFrom: new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                ArrestedTo: new DateTime(2026, 3, 3, 23, 59, 59, DateTimeKind.Utc),
                NameStrategy: TestDataRecordLinkStrategy.Existing,
                LocationStrategy: TestDataRecordLinkStrategy.Existing,
                NameMaxUses: 3,
                LocationMaxUses: 9),
            CancellationToken.None);

        Assert.Equal(3, result.Created);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.NamesCreated);
        Assert.Equal(3, result.NamesReusedFromExisting);
        Assert.Equal(0, result.NamesReusedFromCurrentRun);
        Assert.Equal(0, result.LocationsCreated);
        Assert.Equal(5, result.LocationsReusedFromExisting);
        Assert.Equal(0, result.LocationsReusedFromCurrentRun);

        var arrests = await db.Arrests.ToListAsync();
        var updatedName = await db.Names.SingleAsync();
        Assert.Equal(3, arrests.Count);
        Assert.All(arrests, arrest => Assert.Equal(existingName.Id, arrest.NameId));
        Assert.All(arrests, arrest => Assert.Equal(existingLocation.Id, arrest.LocationId));
        Assert.Equal(existingLocation.Id, updatedName.PrimaryLocationId);
        Assert.Equal(existingLocation.Id, updatedName.SecondaryLocationId);
    }

    [Fact]
    public async Task Handle_WithCurrentRunStrategies_AssignsNameLocationsAndHonorsTimeframe()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var handler = new GenerateTestArrestsHandler(
            db,
            tenantProvider,
            new TestSender(db, tenantProvider),
            new FakePlacesClient(
                new GooglePlaceResult(
                    PlaceName: "Central Precinct",
                    FormattedAddress: "100 Main St, Springfield, TX 75001, USA",
                    StreetNumber: "100",
                    StreetAddress: "Main St",
                    AptSuite: null,
                    City: "Springfield",
                    Zip: "75001",
                    StateAbbreviation: "TX",
                    CountryCode: "US",
                    Lat: 32.7767,
                    Lng: -96.7970)),
            new UserModificationContext(userId));

        var arrestedFrom = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var arrestedTo = new DateTime(2026, 1, 31, 18, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(
            new GenerateTestArrestsCommand(
                Count: 3,
                ArrestedFrom: arrestedFrom,
                ArrestedTo: arrestedTo,
                NameStrategy: TestDataRecordLinkStrategy.RecentlyCreatedOrCreateNew,
                LocationStrategy: TestDataRecordLinkStrategy.RecentlyCreatedOrCreateNew,
                NameMaxUses: 3,
                LocationMaxUses: 10,
                LocationKeyword: "police stations near Dallas TX",
                LocationApiKey: "test-api-key"),
            CancellationToken.None);

        Assert.Equal(3, result.Created);
        Assert.Equal(0, result.Failed);
        Assert.Equal(1, result.NamesCreated);
        Assert.Equal(0, result.NamesReusedFromExisting);
        Assert.Equal(2, result.NamesReusedFromCurrentRun);
        Assert.Equal(1, result.LocationsCreated);
        Assert.Equal(0, result.LocationsReusedFromExisting);
        Assert.Equal(4, result.LocationsReusedFromCurrentRun);

        var names = await db.Names.ToListAsync();
        var locations = await db.Locations.ToListAsync();
        var arrests = await db.Arrests.ToListAsync();

        Assert.Single(names);
        Assert.Single(locations);
        Assert.Equal(3, arrests.Count);
        Assert.All(arrests, arrest => Assert.Equal(names[0].Id, arrest.NameId));
        Assert.All(arrests, arrest => Assert.InRange(arrest.ArrestedAt, arrestedFrom, arrestedTo));

        Assert.NotNull(names[0].PrimaryLocationId);
        Assert.NotNull(names[0].SecondaryLocationId);
        Assert.All(arrests, arrest => Assert.NotNull(arrest.LocationId));
    }

    [Fact]
    public async Task Handle_WithMaxUseOfOne_CreatesFreshNamesAndLocationsPerArrest()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var handler = new GenerateTestArrestsHandler(
            db,
            tenantProvider,
            new TestSender(db, tenantProvider),
            new FakePlacesClient(),
            new UserModificationContext(userId));

        var result = await handler.Handle(
            new GenerateTestArrestsCommand(
                Count: 3,
                ArrestedFrom: new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                ArrestedTo: new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc),
                NameStrategy: TestDataRecordLinkStrategy.RecentlyCreatedOrCreateNew,
                LocationStrategy: TestDataRecordLinkStrategy.RecentlyCreatedOrCreateNew,
                NameMaxUses: 1,
                LocationMaxUses: 1,
                LocationKeyword: "police stations near Dallas TX",
                LocationApiKey: "test-api-key"),
            CancellationToken.None);

        Assert.Equal(3, result.Created);
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, result.NamesCreated);
        Assert.Equal(0, result.NamesReusedFromCurrentRun);
        Assert.Equal(9, result.LocationsCreated);

        Assert.Equal(3, await db.Names.CountAsync());
        Assert.Equal(9, await db.Locations.CountAsync());
        Assert.Equal(3, await db.Arrests.CountAsync());
    }

    [Fact]
    public async Task Handle_IgnoresDuplicatePicklistValuesFromOtherAgencies()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var otherAgencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SeedPicklistItem(db, jurisdictionId, agencyId, PicklistTypes.Direction, "SE", "SE", 1);
        SeedPicklistItem(db, jurisdictionId, otherAgencyId, PicklistTypes.Direction, "SE", "SE", 1);
        SeedPicklistItem(db, jurisdictionId, agencyId, PicklistTypes.StreetType, "Ave", "Ave", 1);
        SeedPicklistItem(db, jurisdictionId, otherAgencyId, PicklistTypes.StreetType, "Ave", "Ave", 1);
        SeedPicklistItem(db, jurisdictionId, agencyId, PicklistTypes.State, "TX", "Texas", 1);
        SeedPicklistItem(db, jurisdictionId, otherAgencyId, PicklistTypes.State, "TX", "Texas", 1);
        SeedPicklistItem(db, jurisdictionId, agencyId, PicklistTypes.Country, "US", "United States", 1);
        SeedPicklistItem(db, jurisdictionId, otherAgencyId, PicklistTypes.Country, "US", "United States", 1);
        await db.SaveChangesAsync(CancellationToken.None);

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var handler = new GenerateTestArrestsHandler(
            db,
            tenantProvider,
            new TestSender(db, tenantProvider),
            new FakePlacesClient(
                new GooglePlaceResult(
                    PlaceName: "Southeast Precinct",
                    FormattedAddress: "200 SE Oak Ave, Springfield, TX 75002, USA",
                    StreetNumber: "200",
                    StreetAddress: "SE Oak Ave",
                    AptSuite: null,
                    City: "Springfield",
                    Zip: "75002",
                    StateAbbreviation: "TX",
                    CountryCode: "US",
                    Lat: 32.0,
                    Lng: -96.0)),
            new UserModificationContext(userId));

        var result = await handler.Handle(
            new GenerateTestArrestsCommand(
                Count: 1,
                ArrestedFrom: new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                ArrestedTo: new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                NameStrategy: TestDataRecordLinkStrategy.RecentlyCreatedOrCreateNew,
                LocationStrategy: TestDataRecordLinkStrategy.CreateNew,
                NameMaxUses: 1,
                LocationMaxUses: 1,
                LocationKeyword: "police stations near Dallas TX",
                LocationApiKey: "test-api-key"),
            CancellationToken.None);

        Assert.Equal(1, result.Created);
        Assert.Equal(0, result.Failed);
    }

    private static void SeedPicklistItem(
        RecordPageSaveTestDbContext db,
        Guid jurisdictionId,
        Guid agencyId,
        string picklistType,
        string value,
        string label,
        int sortOrder)
    {
        db.PicklistItems.Add(new PicklistItem(
            jurisdictionId,
            agencyId,
            picklistType,
            value,
            label,
            sortOrder,
            isSystemDefault: true));
    }

    private sealed class TestSender : ISender
    {
        private readonly RecordPageSaveTestDbContext _dbContext;
        private readonly TestTenantProvider _tenantProvider;
        private long _nextNameRecordNumber = 1000;
        private long _nextLocationRecordNumber = 2000;
        private long _nextArrestRecordNumber = 3000;

        public TestSender(RecordPageSaveTestDbContext dbContext, TestTenantProvider tenantProvider)
        {
            _dbContext = dbContext;
            _tenantProvider = tenantProvider;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unsupported untyped request '{request.GetType().Name}'.");

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                CreateNameCommand command => await HandleCreateNameAsync(command, cancellationToken),
                CreateLocationCommand command => await HandleCreateLocationAsync(command, cancellationToken),
                CreateArrestCommand command => await HandleCreateArrestAsync(command, cancellationToken),
                _ => throw new NotSupportedException($"Unsupported request '{request.GetType().Name}'.")
            };

            return (TResponse)response;
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException($"Unsupported request '{request.GetType().Name}'.");

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unsupported stream request '{request.GetType().Name}'.");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unsupported untyped stream request '{request.GetType().Name}'.");

        private async Task<long> HandleCreateNameAsync(CreateNameCommand command, CancellationToken cancellationToken)
        {
            var entity = new Name(
                _tenantProvider.GetJurisdictionId(),
                _tenantProvider.GetAgencyId(),
                command.NameType,
                command.LastOrBusinessName,
                command.FirstName,
                command.MiddleName,
                command.SexId,
                command.RaceId,
                command.DateOfBirth,
                command.DriversLicenseNumber,
                command.DriversLicenseStateId,
                command.HeightInches,
                command.WeightLbs,
                command.HairColorId,
                command.EyeColorId,
                command.SuffixId,
                command.PlaceOfBirth,
                command.FbiNumber,
                command.LocalNumber,
                command.SocialSecurityNumber,
                command.IsCitizen,
                command.DeceasedDate,
                command.PrimaryPhone,
                command.PrimaryPhoneExtension,
                command.WorkPhone,
                command.WorkPhoneExtension,
                command.OtherPhone,
                command.OtherPhoneExtension);

            var recordNumber = _nextNameRecordNumber++;
            SetRecordNumber(entity, recordNumber);

            _dbContext.Names.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return recordNumber;
        }

        private async Task<long> HandleCreateLocationAsync(CreateLocationCommand command, CancellationToken cancellationToken)
        {
            var entity = new Location(
                _tenantProvider.GetJurisdictionId(),
                command.StreetAddress,
                command.City,
                command.StreetNumber,
                command.PreDirectionId,
                command.StreetTypeId,
                command.PostDirectionId,
                command.StateId,
                command.CountryId,
                command.Zip,
                command.AptSuite,
                command.Coordinates,
                command.CommonPlaceName,
                command.Comments,
                command.Address);

            var recordNumber = _nextLocationRecordNumber++;
            SetRecordNumber(entity, recordNumber);

            _dbContext.Locations.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return recordNumber;
        }

        private async Task<long> HandleCreateArrestAsync(CreateArrestCommand command, CancellationToken cancellationToken)
        {
            var arrest = new ArrestFactory().Create(
                _tenantProvider.GetJurisdictionId(),
                _tenantProvider.GetAgencyId(),
                command.NameId,
                command.ArrestedAt,
                string.IsNullOrWhiteSpace(command.ArrestNum) ? $"AR-{_nextArrestRecordNumber}" : command.ArrestNum,
                command.PrimaryIncidentId);

            SetRecordNumber(arrest, _nextArrestRecordNumber++);
            arrest.SetLocation(command.LocationId, new UserModificationContext(_tenantProvider.GetUserId()));

            _dbContext.Arrests.Add(arrest);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return arrest.RecordNumber;
        }

        private static void SetRecordNumber(object entity, long recordNumber)
        {
            var property = entity.GetType().GetProperty("RecordNumber", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"RecordNumber property not found on {entity.GetType().Name}.");

            property.SetValue(entity, recordNumber);
        }
    }

    private sealed class FakePlacesClient(params GooglePlaceResult[] results) : IGoogleMapsPlacesClient
    {
        private readonly IReadOnlyList<GooglePlaceResult> _results = results;

        public Task<IReadOnlyList<GooglePlaceResult>> SearchAsync(string keyword, int maxResults, string apiKey, CancellationToken cancellationToken = default)
            => Task.FromResult(_results.Count == 0
                ? (IReadOnlyList<GooglePlaceResult>)
                [
                    new GooglePlaceResult(
                        PlaceName: "Test Location",
                        FormattedAddress: "200 Oak Ave, Springfield, TX 75002, USA",
                        StreetNumber: "200",
                        StreetAddress: "Oak Ave",
                        AptSuite: null,
                        City: "Springfield",
                        Zip: "75002",
                        StateAbbreviation: "TX",
                        CountryCode: "US",
                        Lat: 32.0,
                        Lng: -96.0)
                ]
                : _results);
    }
}
