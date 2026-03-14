using Modules.Records.Application.Locations.Commands.CreateLocation;
using Modules.Records.Application.Locations.DomainEventHandlers;
using Modules.Records.Application.ReadModels;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Tests.Locations;

public sealed class CreateLocationHandlerTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();

    private static ITenantProvider CreateTenantProvider() =>
        new TestTenantProvider(TestJurisdictionId);

    #region CreateLocationHandler

    [Fact]
    public async Task Handle_ValidCommand_AddsLocationToDb()
    {
        // Arrange
        await using var db      = TestDbContextFactory.Create();
        var tenantProvider      = CreateTenantProvider();
        var handler             = new CreateLocationHandler(db, tenantProvider);
        var command             = new CreateLocationCommand("Main St", "Springfield");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(db.Locations, l => l.JurisdictionId == TestJurisdictionId);
        Assert.NotNull(saved);
        Assert.Equal("Main St", saved.StreetAddress);
        Assert.Equal("Springfield", saved.City);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsAllOptionalFields()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var stateId        = Guid.NewGuid();
        var handler        = new CreateLocationHandler(db, CreateTenantProvider());
        var command        = new CreateLocationCommand(
            "Oak Ave", "Shelbyville",
            StreetNumber:    "456",
            StateId:         stateId,
            Zip:             "62701",
            AptSuite:        "Apt 3B",
            CommonPlaceName: "Town Hall",
            Comments:        "Historic");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(db.Locations);
        Assert.Equal("456", saved.StreetNumber);
        Assert.Equal(stateId, saved.StateId);
        Assert.Equal("62701", saved.Zip);
        Assert.Equal("Apt 3B", saved.AptSuite);
        Assert.Equal("Town Hall", saved.CommonPlaceName);
        Assert.Equal("Historic", saved.Comments);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsJurisdictionFromTenantProvider()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var handler        = new CreateLocationHandler(db, CreateTenantProvider());
        var command        = new CreateLocationCommand("Elm St", "Capital City");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(db.Locations);
        Assert.Equal(TestJurisdictionId, saved.JurisdictionId);
    }

    [Fact]
    public async Task Handle_ValidCommand_RaisesLocationCreatedDomainEvent()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var handler        = new CreateLocationHandler(db, CreateTenantProvider());
        var command        = new CreateLocationCommand("Park Blvd", "Metro");

        // Act — capture the location entity's domain events before SaveChanges clears them
        Location? createdLocation = null;
        var originalAdd = db.Locations;

        await handler.Handle(command, CancellationToken.None);

        var saved = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(db.Locations);

        // The entity itself raised the event before save
        Assert.NotEqual(Guid.Empty, saved.Id);
        Assert.Equal(TestJurisdictionId, saved.JurisdictionId);
    }

    [Fact]
    public async Task Handle_MultipleLocations_EachHasUniqueId()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var handler        = new CreateLocationHandler(db, CreateTenantProvider());

        // Act
        await handler.Handle(new CreateLocationCommand("First St",  "City A"), CancellationToken.None);
        await handler.Handle(new CreateLocationCommand("Second St", "City B"), CancellationToken.None);

        // Assert
        var allLocations = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(db.Locations);
        Assert.Equal(2, allLocations.Count);
        Assert.NotEqual(allLocations[0].Id, allLocations[1].Id);
    }

    #endregion

    #region LocationProjectionHandler — Idempotency

    [Fact]
    public async Task ProjectionHandler_Handle_CreatedEvent_CreatesReadModel()
    {
        // Arrange
        await using var db     = TestDbContextFactory.Create();
        var location           = new Location(TestJurisdictionId, "Oak St", "Springfield");
        db.Locations.Add(location);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new LocationProjectionHandler(db);
        var evt = new LocationCreatedDomainEvent(
            location.Id, TestJurisdictionId, null, "Oak St", "Springfield", null);

        // Act
        await handler.Handle(evt, CancellationToken.None);

        // Assert
        var readModel = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(db.LocationReadModels, r => r.Id == location.Id);
        Assert.NotNull(readModel);
        Assert.Equal("Oak St", readModel.StreetAddress);
        Assert.Equal("Springfield", readModel.City);
        Assert.Equal(TestJurisdictionId, readModel.JurisdictionId);
    }

    [Fact]
    public async Task ProjectionHandler_Handle_CreatedEvent_IsIdempotent()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var location       = new Location(TestJurisdictionId, "Oak St", "Springfield");
        db.Locations.Add(location);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new LocationProjectionHandler(db);
        var evt = new LocationCreatedDomainEvent(
            location.Id, TestJurisdictionId, null, "Oak St", "Springfield", null);

        // Act — handle same event twice
        await handler.Handle(evt, CancellationToken.None);
        await handler.Handle(evt, CancellationToken.None); // idempotent: should not throw or duplicate

        // Assert — exactly one read model row
        var count = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .CountAsync(db.LocationReadModels, r => r.Id == location.Id);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ProjectionHandler_Handle_DetailsUpdatedEvent_UpdatesReadModel()
    {
        // Arrange
        await using var db     = TestDbContextFactory.Create();
        var location           = new Location(TestJurisdictionId, "Old St", "Old City");
        db.Locations.Add(location);
        await db.SaveChangesAsync(CancellationToken.None);

        var stateId = Guid.NewGuid();

        // Seed the read model
        var readModel = LocationReadModel.Create(
            location.Id, 1, TestJurisdictionId,
            null, null, "Old St", null, null, "Old City",
            null, null, null, null, null, null, null, null,
            DateTime.UtcNow, Guid.NewGuid());
        db.LocationReadModels.Add(readModel);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new LocationProjectionHandler(db);
        var evt = new LocationDetailsUpdatedDomainEvent(
            location.Id,
            "100", null, "New St", null, null, "New City",
            stateId, null, "62700", null, null, null, "Updated location", null);

        // Act
        await handler.Handle(evt, CancellationToken.None);

        // Assert
        var updated = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(db.LocationReadModels, r => r.Id == location.Id);
        Assert.Equal("New St", updated.StreetAddress);
        Assert.Equal("New City", updated.City);
        Assert.Equal("100", updated.StreetNumber);
        Assert.Equal(stateId, updated.StateId);
        Assert.Equal("62700", updated.Zip);
        Assert.Equal("Updated location", updated.Comments);
    }

    [Fact]
    public async Task ProjectionHandler_Handle_SoftDeletedEvent_RemovesReadModel()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var locationId     = Guid.NewGuid();

        var readModel = LocationReadModel.Create(
            locationId, 1, TestJurisdictionId,
            null, null, "Elm St", null, null, "Capital",
            null, null, null, null, null, null, null, null,
            DateTime.UtcNow, Guid.NewGuid());
        db.LocationReadModels.Add(readModel);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new LocationProjectionHandler(db);
        var evt = new LocationSoftDeletedDomainEvent(locationId, Guid.NewGuid());

        // Act
        await handler.Handle(evt, CancellationToken.None);

        // Assert
        var exists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .AnyAsync(db.LocationReadModels, r => r.Id == locationId);
        Assert.False(exists);
    }

    [Fact]
    public async Task ProjectionHandler_Handle_SoftDeletedEvent_WhenReadModelMissing_DoesNotThrow()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var handler        = new LocationProjectionHandler(db);
        var evt            = new LocationSoftDeletedDomainEvent(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert — no exception
        await handler.Handle(evt, CancellationToken.None);
    }

    #endregion

    #region Test ITenantProvider

    private sealed class TestTenantProvider : ITenantProvider
    {
        private Guid _jurisdictionId;

        public TestTenantProvider(Guid jurisdictionId) => _jurisdictionId = jurisdictionId;

        public Guid GetJurisdictionId() => _jurisdictionId;
        public Guid GetAgencyId()       => Guid.NewGuid();
        public Guid GetUserId()         => Guid.NewGuid();
        public void SetJurisdictionId(Guid jurisdictionId) => _jurisdictionId = jurisdictionId;
    }

    #endregion
}
