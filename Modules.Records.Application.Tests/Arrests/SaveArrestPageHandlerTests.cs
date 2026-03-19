using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Arrests.Commands.SaveArrestPage;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Arrests;

public sealed class SaveArrestPageHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesArrestLinksAndCharges_WithSingleSaveCall()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveArrestPageHandler(db, tenantProvider, modificationContext);

        var name = new Name(jurisdictionId, agencyId, NameTypes.Person, "Doe", "Jane", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
        var arrest = new Arrest(jurisdictionId, agencyId, name.Id, DateTime.UtcNow.AddDays(-2), "AR-1", null);
        var incidentToRemove = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Old incident",
                IncidentNum = "INC-OLD",
                LocalNum = "LOC-OLD"
            }
        });
        var incidentToAdd = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "New incident",
                IncidentNum = "INC-NEW",
                LocalNum = "LOC-NEW"
            }
        });
        var chargeToRemove = new Charge(jurisdictionId, agencyId, "Remove", "Cat", "Group", "Person", "010", "Misdemeanor", null, false);
        var chargeToAdd = new Charge(jurisdictionId, agencyId, "Add", "Cat", "Group", "Person", "011", "Felony", null, false);

        db.Names.Add(name);
        db.Arrests.Add(arrest);
        db.Incidents.AddRange(incidentToRemove, incidentToAdd);
        db.Charges.AddRange(chargeToRemove, chargeToAdd);
        db.IncidentArrestLinks.Add(new IncidentArrestLink(jurisdictionId, incidentToRemove.Id, arrest.Id, userId));
        db.ArrestChargeLinks.Add(new ArrestChargeLink(jurisdictionId, arrest.Id, chargeToRemove.Id, userId));
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        var command = new SaveArrestPageCommand(
            arrest.Id,
            name.Id,
            DateTime.UtcNow.AddHours(-1),
            Guid.NewGuid(),
            "AR-200",
            locationId,
            incidentToAdd.Id,
            [incidentToAdd.Id],
            [incidentToRemove.Id],
            [chargeToAdd.Id],
            [chargeToRemove.Id]);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, db.SaveChangesCallCount);

        var savedArrest = await db.Arrests.SingleAsync(a => a.Id == arrest.Id);
        Assert.Equal("AR-200", savedArrest.ArrestNum);
        Assert.Equal(locationId, savedArrest.LocationId);
        Assert.Equal(incidentToAdd.Id, savedArrest.PrimaryIncidentId);

        var snapshot = await db.ArrestNameSnapshots.SingleAsync(s => s.ArrestId == arrest.Id);
        Assert.Equal(name.Id, snapshot.SourceNameId);
        Assert.Equal(name.RecordNumber, snapshot.SourceNameRecordNumber);
        Assert.Equal("Doe", snapshot.LastOrBusinessName);
        Assert.Equal("Jane", snapshot.FirstName);

        var linkedIncidentIds = await db.IncidentArrestLinks
            .Where(link => link.ArrestId == arrest.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();
        Assert.Single(linkedIncidentIds);
        Assert.Contains(incidentToAdd.Id, linkedIncidentIds);

        var chargeLinkIds = await db.ArrestChargeLinks
            .Where(link => link.ArrestId == arrest.Id)
            .Select(link => link.ChargeId)
            .ToListAsync();
        Assert.Single(chargeLinkIds);
        Assert.Contains(chargeToAdd.Id, chargeLinkIds);
    }

    [Fact]
    public async Task Handle_PrimaryIncidentNotInIdsToAdd_StillCreatesLink()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveArrestPageHandler(db, tenantProvider, modificationContext);

        var name = new Name(jurisdictionId, agencyId, NameTypes.Person, "Smith", "John", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
        var arrest = new Arrest(jurisdictionId, agencyId, name.Id, DateTime.UtcNow.AddDays(-1), "AR-2", null);
        var primaryIncident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Primary incident",
                IncidentNum = "INC-PRIMARY",
                LocalNum = "LOC-PRIMARY"
            }
        });

        db.Names.Add(name);
        db.Arrests.Add(arrest);
        db.Incidents.Add(primaryIncident);
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        // PrimaryIncidentId is set but NOT included in IncidentIdsToAdd
        var command = new SaveArrestPageCommand(
            arrest.Id,
            name.Id,
            DateTime.UtcNow.AddHours(-1),
            null,
            "AR-2",
            PrimaryIncidentId: primaryIncident.Id,
            IncidentIdsToAdd: null,
            IncidentIdsToRemove: null);

        await handler.Handle(command, CancellationToken.None);

        var linkedIncidentIds = await db.IncidentArrestLinks
            .Where(link => link.ArrestId == arrest.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();

        Assert.Single(linkedIncidentIds);
        Assert.Contains(primaryIncident.Id, linkedIncidentIds);
    }

    [Fact]
    public async Task Handle_PrimaryIncidentInIdsToRemove_RemainsLinked()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveArrestPageHandler(db, tenantProvider, modificationContext);

        var name = new Name(jurisdictionId, agencyId, NameTypes.Person, "Jones", "Alice", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
        var arrest = new Arrest(jurisdictionId, agencyId, name.Id, DateTime.UtcNow.AddDays(-1), "AR-3", null);
        var primaryIncident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Primary incident",
                IncidentNum = "INC-PRI",
                LocalNum = "LOC-PRI"
            }
        });

        db.Names.Add(name);
        db.Arrests.Add(arrest);
        db.Incidents.Add(primaryIncident);
        db.IncidentArrestLinks.Add(new IncidentArrestLink(jurisdictionId, primaryIncident.Id, arrest.Id, userId));
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        // Client mistakenly includes the primary incident in the remove set
        var command = new SaveArrestPageCommand(
            arrest.Id,
            name.Id,
            DateTime.UtcNow.AddHours(-1),
            null,
            "AR-3",
            PrimaryIncidentId: primaryIncident.Id,
            IncidentIdsToAdd: null,
            IncidentIdsToRemove: [primaryIncident.Id]);

        await handler.Handle(command, CancellationToken.None);

        var linkedIncidentIds = await db.IncidentArrestLinks
            .Where(link => link.ArrestId == arrest.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();

        // The primary incident must remain linked
        Assert.Contains(primaryIncident.Id, linkedIncidentIds);

        var savedArrest = await db.Arrests.SingleAsync(a => a.Id == arrest.Id);
        Assert.Equal(primaryIncident.Id, savedArrest.PrimaryIncidentId);
    }

    [Fact]
    public async Task Handle_PrimaryIncidentAlsoInIdsToAdd_DoesNotCreateDuplicateLink()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveArrestPageHandler(db, tenantProvider, modificationContext);

        var name = new Name(jurisdictionId, agencyId, NameTypes.Person, "Brown", "Bob", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
        var arrest = new Arrest(jurisdictionId, agencyId, name.Id, DateTime.UtcNow.AddDays(-1), "AR-4", null);
        var primaryIncident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Primary incident",
                IncidentNum = "INC-DUP",
                LocalNum = "LOC-DUP"
            }
        });

        db.Names.Add(name);
        db.Arrests.Add(arrest);
        db.Incidents.Add(primaryIncident);
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        // PrimaryIncidentId is set AND also explicitly included in IncidentIdsToAdd
        var command = new SaveArrestPageCommand(
            arrest.Id,
            name.Id,
            DateTime.UtcNow.AddHours(-1),
            null,
            "AR-4",
            PrimaryIncidentId: primaryIncident.Id,
            IncidentIdsToAdd: [primaryIncident.Id]);

        await handler.Handle(command, CancellationToken.None);

        var linkedIncidentIds = await db.IncidentArrestLinks
            .Where(link => link.ArrestId == arrest.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();

        // Exactly one link should exist — no duplicates
        Assert.Single(linkedIncidentIds);
        Assert.Contains(primaryIncident.Id, linkedIncidentIds);
    }

    [Fact]
    public async Task Handle_UpdatesAtTimeOfSnapshot_WhenManualValuesProvided()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveArrestPageHandler(db, tenantProvider, modificationContext);

        var name = new Name(jurisdictionId, agencyId, NameTypes.Person, "Original", "Casey", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
        var arrest = new Arrest(jurisdictionId, agencyId, name.Id, DateTime.UtcNow.AddDays(-1), "AR-5", null);

        db.Names.Add(name);
        db.Arrests.Add(arrest);
        db.ArrestNameSnapshots.Add(ArrestNameSnapshot.Create(
            jurisdictionId,
            agencyId,
            arrest.Id,
            name.Id,
            10001,
            NameTypes.Person,
            "Original",
            "Casey",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            null,
            "Old primary address",
            null,
            null,
            "Old secondary address",
            userId));
        await db.SaveChangesAsync(CancellationToken.None);

        await handler.Handle(
            new SaveArrestPageCommand(
                arrest.Id,
                name.Id,
                DateTime.UtcNow.AddHours(-1),
                null,
                "AR-5",
                AtTimeOfName: new Modules.Records.Application.DTOs.NameSnapshotInput(
                    NameTypes.Person,
                    "Updated",
                    "Taylor",
                    null,
                    null,
                    null,
                    new DateTime(1992, 5, 6),
                    "DL-200",
                    null,
                    71,
                    190,
                    null,
                    null,
                    null,
                    "Plano",
                    "FBI-200",
                    "LOCAL-200",
                    "555-0100",
                    "12",
                    "555-0200",
                    "34",
                    "555-0300",
                    "56",
                    "222-33-4444",
                    true,
                    null,
                    new Modules.Records.Application.DTOs.NameSnapshotAddressDto(null, null, "New primary address"),
                    new Modules.Records.Application.DTOs.NameSnapshotAddressDto(null, null, "New secondary address"))),
            CancellationToken.None);

        var snapshot = await db.ArrestNameSnapshots.SingleAsync(s => s.ArrestId == arrest.Id);
        Assert.Equal("Updated", snapshot.LastOrBusinessName);
        Assert.Equal("Taylor", snapshot.FirstName);
        Assert.Equal("DL-200", snapshot.DriversLicenseNumber);
        Assert.Equal("Plano", snapshot.PlaceOfBirth);
        Assert.Equal("New primary address", snapshot.PrimaryLocationAddress);
        Assert.Equal("New secondary address", snapshot.SecondaryLocationAddress);
        Assert.Equal(true, snapshot.IsCitizen);
        Assert.Equal(userId, snapshot.LastCopiedByUserId);
        Assert.NotNull(snapshot.LastCopiedAtUtc);
    }
}
