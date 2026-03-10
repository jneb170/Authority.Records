using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class NameTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    private static IModificationContext CreateContext(
        Guid? userId = null,
        bool canOverrideLocks = false,
        bool canModifyClosedRecords = false,
        bool isSystem = false) =>
        new TestModificationContext(
            userId ?? TestUserId,
            canOverrideLocks,
            canModifyClosedRecords,
            isSystem);

    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesPerson_WithAllRequiredFields()
    {
        // Arrange
        var nameType = "Person";
        var lastName = "Smith";
        var firstName = "John";
        var middleName = "Michael";

        // Act
        var name = new Name(
            TestJurisdictionId,
            TestAgencyId,
            nameType,
            lastName,
            firstName,
            middleName,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);

        // Assert
        Assert.NotEqual(Guid.Empty, name.Id);
        Assert.Equal(TestJurisdictionId, name.JurisdictionId);
        Assert.Equal(TestAgencyId, name.AgencyId);
        Assert.Equal(nameType, name.NameType);
        Assert.Equal(lastName, name.LastOrBusinessName);
        Assert.Equal(firstName, name.FirstName);
        Assert.Equal(middleName, name.MiddleName);
    }

    [Fact]
    public void Constructor_CreatesBusiness_WithBusinessName()
    {
        // Arrange
        var nameType = "Business";
        var businessName = "Acme Corporation";

        // Act
        var name = new Name(
            TestJurisdictionId,
            TestAgencyId,
            nameType,
            businessName,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);

        // Assert
        Assert.Equal(nameType, name.NameType);
        Assert.Equal(businessName, name.LastOrBusinessName);
        Assert.Null(name.FirstName);
        Assert.Null(name.MiddleName);
    }

    [Fact]
    public void Constructor_WithAllPersonFields_SetsAllProperties()
    {
        // Arrange
        var sexId = Guid.NewGuid();
        var raceId = Guid.NewGuid();
        var dateOfBirth = new DateTime(1990, 5, 15);
        var dlNumber = "D1234567";
        var dlStateId = Guid.NewGuid();
        var heightInches = 72;
        var weightLbs = 180;
        var hairColorId = Guid.NewGuid();
        var eyeColorId = Guid.NewGuid();
        var suffixId = Guid.NewGuid();
        var placeOfBirth = "New York, NY";
        var fbiNumber = "FBI123456";
        var localNumber = "LOCAL789";
        var ssn = "123-45-6789";
        var isCitizen = true;
        var deceasedDate = new DateTime(2025, 1, 1);

        // Act
        var name = new Name(
            TestJurisdictionId,
            TestAgencyId,
            "Person",
            "Doe",
            "Jane",
            "Marie",
            sexId,
            raceId,
            dateOfBirth,
            dlNumber,
            dlStateId,
            heightInches,
            weightLbs,
            hairColorId,
            eyeColorId,
            suffixId,
            placeOfBirth,
            fbiNumber,
            localNumber,
            ssn,
            isCitizen,
            deceasedDate);

        // Assert
        Assert.Equal(sexId, name.SexId);
        Assert.Equal(raceId, name.RaceId);
        Assert.Equal(dateOfBirth, name.DateOfBirth);
        Assert.Equal(dlNumber, name.DriversLicenseNumber);
        Assert.Equal(dlStateId, name.DriversLicenseStateId);
        Assert.Equal(heightInches, name.HeightInches);
        Assert.Equal(weightLbs, name.WeightLbs);
        Assert.Equal(hairColorId, name.HairColorId);
        Assert.Equal(eyeColorId, name.EyeColorId);
        Assert.Equal(suffixId, name.SuffixId);
        Assert.Equal(placeOfBirth, name.PlaceOfBirth);
        Assert.Equal(fbiNumber, name.FbiNumber);
        Assert.Equal(localNumber, name.LocalNumber);
        Assert.Equal(ssn, name.SocialSecurityNumber);
        Assert.Equal(isCitizen, name.IsCitizen);
        Assert.Equal(deceasedDate, name.DeceasedDate);
    }

    [Fact]
    public void Constructor_RaisesNameCreatedDomainEvent()
    {
        // Arrange & Act
        var name = new Name(
            TestJurisdictionId,
            TestAgencyId,
            "Person",
            "Smith",
            "John",
            "Michael",
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);

        // Assert
        var domainEvents = name.DomainEvents;
        var createdEvent = Assert.Single(domainEvents.OfType<NameCreatedDomainEvent>());
        Assert.Equal(name.Id, createdEvent.NameId);
        Assert.Equal(TestJurisdictionId, createdEvent.JurisdictionId);
        Assert.Equal("Person", createdEvent.NameType);
        Assert.Equal("Smith", createdEvent.LastOrBusinessName);
        Assert.Equal("John", createdEvent.FirstName);
        Assert.Equal("Michael", createdEvent.MiddleName);
    }

    #endregion

    #region UpdateDetails Tests

    [Fact]
    public void UpdateDetails_UpdatesAllFields_Successfully()
    {
        // Arrange
        var name = CreateTestPerson();
        var context = CreateContext();

        var newSexId = Guid.NewGuid();
        var newRaceId = Guid.NewGuid();
        var newDateOfBirth = new DateTime(1985, 3, 20);
        var newDlNumber = "NEW123";
        var newDlStateId = Guid.NewGuid();
        var newHeight = 68;
        var newWeight = 150;
        var newHairColorId = Guid.NewGuid();
        var newEyeColorId = Guid.NewGuid();
        var newSuffixId = Guid.NewGuid();
        var newPlaceOfBirth = "Boston, MA";
        var newFbiNumber = "FBI999";
        var newLocalNumber = "LOCAL999";
        var newSsn = "987-65-4321";
        var newIsCitizen = true;
        var newDeceasedDate = new DateTime(2026, 2, 1);

        // Act
        name.UpdateDetails(
            "Person",
            "NewLastName",
            "NewFirstName",
            "NewMiddleName",
            newSexId,
            newRaceId,
            newDateOfBirth,
            newDlNumber,
            newDlStateId,
            newHeight,
            newWeight,
            newHairColorId,
            newEyeColorId,
            newSuffixId,
            newPlaceOfBirth,
            newFbiNumber,
            newLocalNumber,
            newSsn,
            newIsCitizen,
            newDeceasedDate,
            context);

        // Assert
        Assert.Equal("NewLastName", name.LastOrBusinessName);
        Assert.Equal("NewFirstName", name.FirstName);
        Assert.Equal("NewMiddleName", name.MiddleName);
        Assert.Equal(newSexId, name.SexId);
        Assert.Equal(newRaceId, name.RaceId);
        Assert.Equal(newDateOfBirth, name.DateOfBirth);
        Assert.Equal(newDlNumber, name.DriversLicenseNumber);
        Assert.Equal(newDlStateId, name.DriversLicenseStateId);
        Assert.Equal(newHeight, name.HeightInches);
        Assert.Equal(newWeight, name.WeightLbs);
        Assert.Equal(newHairColorId, name.HairColorId);
        Assert.Equal(newEyeColorId, name.EyeColorId);
        Assert.Equal(newSuffixId, name.SuffixId);
        Assert.Equal(newPlaceOfBirth, name.PlaceOfBirth);
        Assert.Equal(newFbiNumber, name.FbiNumber);
        Assert.Equal(newLocalNumber, name.LocalNumber);
        Assert.Equal(newSsn, name.SocialSecurityNumber);
        Assert.Equal(newIsCitizen, name.IsCitizen);
        Assert.Equal(newDeceasedDate, name.DeceasedDate);
    }

    [Fact]
    public void UpdateDetails_RaisesNameDetailsUpdatedDomainEvent()
    {
        // Arrange
        var name = CreateTestPerson();
        var context = CreateContext();
        name.ClearDomainEvents(); // Clear creation event

        // Act
        name.UpdateDetails(
            "Person",
            "UpdatedLastName",
            "UpdatedFirstName",
            null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null,
            context);

        // Assert
        var domainEvents = name.DomainEvents;
        var updatedEvent = Assert.Single(domainEvents.OfType<NameDetailsUpdatedDomainEvent>());
        Assert.Equal(name.Id, updatedEvent.NameId);
        Assert.Equal("UpdatedLastName", updatedEvent.LastOrBusinessName);
        Assert.Equal("UpdatedFirstName", updatedEvent.FirstName);
    }

    [Fact]
    public void UpdateDetails_CanConvertPersonToBusiness()
    {
        // Arrange
        var name = CreateTestPerson();
        var context = CreateContext();

        // Act
        name.UpdateDetails(
            "Business",
            "Acme Corporation",
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null,
            context);

        // Assert
        Assert.Equal("Business", name.NameType);
        Assert.Equal("Acme Corporation", name.LastOrBusinessName);
        Assert.Null(name.FirstName);
        Assert.Null(name.MiddleName);
    }

    [Fact]
    public void UpdateDetails_CanClearOptionalFields()
    {
        // Arrange
        var name = new Name(
            TestJurisdictionId,
            TestAgencyId,
            "Person",
            "Doe",
            "John",
            "Michael",
            Guid.NewGuid(), // sexId
            Guid.NewGuid(), // raceId
            new DateTime(1990, 1, 1),
            "DL123",
            Guid.NewGuid(),
            72,
            180,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NYC",
            "FBI123",
            "LOCAL123",
            "123-45-6789",
            true,
            null);

        var context = CreateContext();

        // Act - Clear all optional fields
        name.UpdateDetails(
            "Person",
            "Doe",
            "John",
            null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null,
            context);

        // Assert
        Assert.Null(name.MiddleName);
        Assert.Null(name.SexId);
        Assert.Null(name.RaceId);
        Assert.Null(name.DateOfBirth);
        Assert.Null(name.DriversLicenseNumber);
        Assert.Null(name.DriversLicenseStateId);
        Assert.Null(name.HeightInches);
        Assert.Null(name.WeightLbs);
        Assert.Null(name.HairColorId);
        Assert.Null(name.EyeColorId);
        Assert.Null(name.SuffixId);
        Assert.Null(name.PlaceOfBirth);
        Assert.Null(name.FbiNumber);
        Assert.Null(name.LocalNumber);
        Assert.Null(name.SocialSecurityNumber);
        Assert.False(name.IsCitizen);
        Assert.Null(name.DeceasedDate);
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_MarksRecordAsDeleted()
    {
        // Arrange
        var name = CreateTestPerson();

        // Act
        name.SoftDelete(TestUserId);

        // Assert
        Assert.True(name.IsDeleted);
        Assert.Equal(TestUserId, name.DeletedBy);
        Assert.NotNull(name.DeletedAtUtc);
    }

    [Fact]
    public void SoftDelete_RaisesNameSoftDeletedDomainEvent()
    {
        // Arrange
        var name = CreateTestPerson();
        name.ClearDomainEvents();

        // Act
        name.SoftDelete(TestUserId);

        // Assert
        var domainEvents = name.DomainEvents;
        var deletedEvent = Assert.Single(domainEvents.OfType<NameSoftDeletedDomainEvent>());
        Assert.Equal(name.Id, deletedEvent.NameId);
        Assert.Equal(TestUserId, deletedEvent.UserId);
    }

    #endregion

    #region Restore Tests

    [Fact]
    public void Restore_RestoresDeletedRecord()
    {
        // Arrange
        var name = CreateTestPerson();
        name.SoftDelete(TestUserId);

        // Act
        name.Restore(TestUserId);

        // Assert
        Assert.False(name.IsDeleted);
        Assert.Null(name.DeletedBy);
        Assert.Null(name.DeletedAtUtc);
    }

    [Fact]
    public void Restore_RaisesNameRestoredDomainEvent()
    {
        // Arrange
        var name = CreateTestPerson();
        name.SoftDelete(TestUserId);
        name.ClearDomainEvents();

        // Act
        name.Restore(TestUserId);

        // Assert
        var domainEvents = name.DomainEvents;
        var restoredEvent = Assert.Single(domainEvents.OfType<NameRestoredDomainEvent>());
        Assert.Equal(name.Id, restoredEvent.NameId);
        Assert.Equal(TestUserId, restoredEvent.UserId);
    }

    #endregion

    #region Multi-Tenant Tests

    [Fact]
    public void Name_ImplementsIMultiTenant()
    {
        // Arrange
        var name = CreateTestPerson();

        // Assert
        Assert.IsAssignableFrom<IMultiTenant>(name);
        Assert.Equal(TestJurisdictionId, name.JurisdictionId);
        Assert.Equal(TestAgencyId, name.AgencyId);
    }

    [Fact]
    public void Name_JurisdictionAndAgency_AreImmutable()
    {
        // Arrange
        var name = CreateTestPerson();
        var context = CreateContext();

        var originalJurisdiction = name.JurisdictionId;
        var originalAgency = name.AgencyId;

        // Act - Update details doesn't change jurisdiction/agency
        name.UpdateDetails(
            "Person",
            "NewName",
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null,
            context);

        // Assert
        Assert.Equal(originalJurisdiction, name.JurisdictionId);
        Assert.Equal(originalAgency, name.AgencyId);
    }

    #endregion

    #region Helper Methods

    private static Name CreateTestPerson() =>
        new Name(
            TestJurisdictionId,
            TestAgencyId,
            "Person",
            "TestLastName",
            "TestFirstName",
            "TestMiddleName",
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);

    #endregion

    #region Test Implementation of IModificationContext

    private sealed class TestModificationContext : IModificationContext
    {
        public Guid UserId { get; }
        public bool CanOverrideLocks { get; }
        public bool CanModifyClosedRecords { get; }
        public bool IsSystem { get; }

        public TestModificationContext(
            Guid userId,
            bool canOverrideLocks,
            bool canModifyClosedRecords,
            bool isSystem)
        {
            UserId = userId;
            CanOverrideLocks = canOverrideLocks;
            CanModifyClosedRecords = canModifyClosedRecords;
            IsSystem = isSystem;
        }
    }

    #endregion
}