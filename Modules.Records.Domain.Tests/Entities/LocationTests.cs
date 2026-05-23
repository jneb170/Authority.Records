using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class LocationTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    private static IModificationContext CreateContext(
        Guid? userId = null,
        bool canOverrideLocks = false) =>
        new TestModificationContext(userId ?? TestUserId, canOverrideLocks);

    #region Constructor Tests

    [Fact]
    public void Constructor_WithRequiredFields_SetsProperties()
    {
        // Act
        var location = new Location(TestJurisdictionId, "Main St", "Springfield");

        // Assert
        Assert.NotEqual(Guid.Empty, location.Id);
        Assert.Equal(TestJurisdictionId, location.JurisdictionId);
        Assert.Equal("Main St", location.StreetAddress);
        Assert.Equal("Springfield", location.City);
    }

    [Fact]
    public void Constructor_WithAllOptionalFields_SetsAllProperties()
    {
        // Arrange
        var preDirectionId  = Guid.NewGuid();
        var streetTypeId    = Guid.NewGuid();
        var postDirectionId = Guid.NewGuid();
        var stateId         = Guid.NewGuid();
        var countryId       = Guid.NewGuid();

        // Act
        var location = new Location(
            TestJurisdictionId,
            "Oak Ave",
            "Shelbyville",
            streetNumber:   "456",
            preDirectionId: preDirectionId,
            streetTypeId:   streetTypeId,
            postDirectionId: postDirectionId,
            stateId:        stateId,
            countryId:      countryId,
            zip:            "62701",
            aptSuite:       "Apt 3B",
            coordinates:    "39.7984,-89.6441",
            commonPlaceName: "Town Hall",
            comments:       "Historic building");

        // Assert
        Assert.Equal("456", location.StreetNumber);
        Assert.Equal(preDirectionId, location.PreDirectionId);
        Assert.Equal(streetTypeId, location.StreetTypeId);
        Assert.Equal(postDirectionId, location.PostDirectionId);
        Assert.Equal(stateId, location.StateId);
        Assert.Equal(countryId, location.CountryId);
        Assert.Equal("62701", location.Zip);
        Assert.Equal("Apt 3B", location.AptSuite);
        Assert.Equal("39.7984,-89.6441", location.Coordinates);
        Assert.Equal("Town Hall", location.CommonPlaceName);
        Assert.Equal("Historic building", location.Comments);
    }

    [Fact]
    public void Constructor_RaisesLocationCreatedDomainEvent()
    {
        // Arrange
        var stateId = Guid.NewGuid();

        // Act
        var location = new Location(
            TestJurisdictionId, "Elm St", "Capital City",
            commonPlaceName: "Police HQ", stateId: stateId);

        // Assert
        var evt = Assert.Single(location.DomainEvents.OfType<LocationCreatedDomainEvent>());
        Assert.Equal(location.Id, evt.LocationId);
        Assert.Equal(TestJurisdictionId, evt.JurisdictionId);
        Assert.Equal("Elm St", evt.StreetAddress);
        Assert.Equal("Capital City", evt.City);
        Assert.Equal(stateId, evt.StateId);
        Assert.Equal("Police HQ", evt.CommonPlaceName);
    }

    [Fact]
    public void Constructor_NewLocation_IsNotDeleted_AndNotLocked()
    {
        // Act
        var location = new Location(TestJurisdictionId, "Oak Ave", "Springfield");

        // Assert
        Assert.False(location.IsDeleted);
        Assert.False(location.IsLocked);
        Assert.Null(location.DeletedBy);
        Assert.Null(location.DeletedAtUtc);
    }

    [Fact]
    public void Location_ImplementsIMultiTenant()
    {
        // Act
        var location = new Location(TestJurisdictionId, "Main St", "Springfield");

        // Assert
        Assert.IsAssignableFrom<IMultiTenant>(location);
        Assert.Equal(TestJurisdictionId, location.JurisdictionId);
    }

    #endregion

    #region UpdateDetails Tests

    [Fact]
    public void UpdateDetails_UpdatesAllFields()
    {
        // Arrange
        var location = CreateTestLocation();
        var context  = CreateContext();
        var newStateId = Guid.NewGuid();

        // Act
        location.UpdateDetails(
            "Broad St", "Shelbyville",
            "789", null, null, null,
            newStateId, null, "12345", "Suite 100",
            null, "City Hall", "Main entrance",
            null, context);

        // Assert
        Assert.Equal("Broad St", location.StreetAddress);
        Assert.Equal("Shelbyville", location.City);
        Assert.Equal("789", location.StreetNumber);
        Assert.Equal(newStateId, location.StateId);
        Assert.Equal("12345", location.Zip);
        Assert.Equal("Suite 100", location.AptSuite);
        Assert.Equal("City Hall", location.CommonPlaceName);
        Assert.Equal("Main entrance", location.Comments);
    }

    [Fact]
    public void UpdateDetails_RaisesLocationDetailsUpdatedDomainEvent()
    {
        // Arrange
        var location = CreateTestLocation();
        var context  = CreateContext();
        location.ClearDomainEvents();

        // Act
        location.UpdateDetails(
            "New St", "New City",
            null, null, null, null, null, null, null, null, null, null, null,
            null, context);

        // Assert
        var evt = Assert.Single(location.DomainEvents.OfType<LocationDetailsUpdatedDomainEvent>());
        Assert.Equal(location.Id, evt.LocationId);
        Assert.Equal("New St", evt.StreetAddress);
        Assert.Equal("New City", evt.City);
    }

    [Fact]
    public void UpdateDetails_CanClearOptionalFields()
    {
        // Arrange
        var stateId = Guid.NewGuid();
        var location = new Location(
            TestJurisdictionId, "Park Ave", "Metro",
            streetNumber: "100", stateId: stateId, zip: "99999");
        var context = CreateContext();

        // Act
        location.UpdateDetails(
            "Park Ave", "Metro",
            null, null, null, null, null, null, null, null, null, null, null,
            null, context);

        // Assert
        Assert.Null(location.StreetNumber);
        Assert.Null(location.StateId);
        Assert.Null(location.Zip);
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_MarksRecordAsDeleted()
    {
        // Arrange
        var location = CreateTestLocation();

        // Act
        location.SoftDelete(TestUserId);

        // Assert
        Assert.True(location.IsDeleted);
        Assert.Equal(TestUserId, location.DeletedBy);
        Assert.NotNull(location.DeletedAtUtc);
    }

    [Fact]
    public void SoftDelete_RaisesLocationSoftDeletedDomainEvent()
    {
        // Arrange
        var location = CreateTestLocation();
        location.ClearDomainEvents();

        // Act
        location.SoftDelete(TestUserId);

        // Assert
        var evt = Assert.Single(location.DomainEvents.OfType<LocationSoftDeletedDomainEvent>());
        Assert.Equal(location.Id, evt.LocationId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    #endregion

    #region Restore Tests

    [Fact]
    public void Restore_ClearsDeletedFields()
    {
        // Arrange
        var location = CreateTestLocation();
        location.SoftDelete(TestUserId);

        // Act
        location.Restore(TestUserId);

        // Assert
        Assert.False(location.IsDeleted);
        Assert.Null(location.DeletedBy);
        Assert.Null(location.DeletedAtUtc);
    }

    [Fact]
    public void Restore_RaisesLocationRestoredDomainEvent()
    {
        // Arrange
        var location = CreateTestLocation();
        location.SoftDelete(TestUserId);
        location.ClearDomainEvents();

        // Act
        location.Restore(TestUserId);

        // Assert
        var evt = Assert.Single(location.DomainEvents.OfType<LocationRestoredDomainEvent>());
        Assert.Equal(location.Id, evt.LocationId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    [Fact]
    public void SoftDelete_Then_Restore_LeavesRecordUndeleted()
    {
        // Arrange
        var location = CreateTestLocation();

        // Act
        location.SoftDelete(TestUserId);
        location.Restore(TestUserId);

        // Assert
        Assert.False(location.IsDeleted);
    }

    #endregion

    #region Lock Tests

    [Fact]
    public void AcquireLock_SetsLockedState()
    {
        // Arrange
        var location = CreateTestLocation();
        var context  = CreateContext();

        // Act
        location.AcquireLock(context, TimeSpan.FromMinutes(10));

        // Assert
        Assert.True(location.IsLocked);
        Assert.Equal(TestUserId, location.LockedByUserId);
    }

    [Fact]
    public void ReleaseLock_ClearsLockedState()
    {
        // Arrange
        var location = CreateTestLocation();
        var context  = CreateContext();
        location.AcquireLock(context, TimeSpan.FromMinutes(10));

        // Act
        location.ReleaseLock(context);

        // Assert
        Assert.False(location.IsLocked);
        Assert.Null(location.LockedByUserId);
    }

    [Fact]
    public void AcquireLock_WithLockingAgency_StampsLockedByAgencyId()
    {
        // Arrange — Location is the shared MLI with no permanent AgencyId, so the locking
        // agency (whose configured timeout governs the lock) is supplied at acquire time.
        var location = CreateTestLocation();
        var context  = CreateContext();
        var agencyId = Guid.NewGuid();

        // Act
        location.AcquireLock(context, TimeSpan.FromMinutes(10), agencyId);

        // Assert
        Assert.True(location.IsLocked);
        Assert.Equal(TestUserId, location.LockedByUserId);
        Assert.Equal(agencyId, location.LockedByAgencyId);
    }

    [Fact]
    public void ReleaseLock_ClearsLockedByAgencyId()
    {
        // Arrange
        var location = CreateTestLocation();
        var context  = CreateContext();
        location.AcquireLock(context, TimeSpan.FromMinutes(10), Guid.NewGuid());

        // Act
        location.ReleaseLock(context);

        // Assert
        Assert.Null(location.LockedByAgencyId);
    }

    [Fact]
    public void UpdateDetails_WhenLockedByOtherUser_Throws()
    {
        // Arrange
        var location     = CreateTestLocation();
        var ownerContext = CreateContext();
        var otherUserId  = Guid.NewGuid();
        var otherContext = CreateContext(otherUserId);

        location.AcquireLock(ownerContext, TimeSpan.FromMinutes(10));

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            location.UpdateDetails(
                "Blocked St", "Nowhere",
                null, null, null, null, null, null, null, null, null, null, null,
                null, otherContext));
    }

    [Fact]
    public void UpdateDetails_WhenLockedBySameUser_Succeeds()
    {
        // Arrange
        var location = CreateTestLocation();
        var context  = CreateContext();
        location.AcquireLock(context, TimeSpan.FromMinutes(10));

        // Act / Assert (no exception)
        location.UpdateDetails(
            "Allowed St", "Somewhere",
            null, null, null, null, null, null, null, null, null, null, null,
            null, context);

        Assert.Equal("Allowed St", location.StreetAddress);
    }

    #endregion

    #region Helper Methods

    private static Location CreateTestLocation() =>
        new Location(TestJurisdictionId, "Main St", "Springfield");

    #endregion

    #region Test IModificationContext

    private sealed class TestModificationContext : IModificationContext
    {
        public Guid UserId { get; }
        public bool CanOverrideLocks { get; }
        public bool CanModifyClosedRecords => false;
        public bool IsSystem => false;

        public TestModificationContext(Guid userId, bool canOverrideLocks = false)
        {
            UserId = userId;
            CanOverrideLocks = canOverrideLocks;
        }
    }

    #endregion
}
