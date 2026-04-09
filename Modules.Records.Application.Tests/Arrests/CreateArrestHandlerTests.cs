using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Arrests;

public sealed class CreateArrestHandlerTests
{
    [Fact]
    public async Task Handle_CreatesArrestAndPrimaryIncidentLink_WithSingleSaveCall()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var handler = new CreateArrestHandler(db, tenantProvider, new ArrestFactory(), new UserModificationContext(userId));

        var homeLocation = new Location(
            jurisdictionId,
            streetAddress: "Main",
            city: "Metro",
            streetNumber: "123",
            zip: "12345",
            address: "123 Main, Metro, TX 12345");
        var name = new Name(
            jurisdictionId,
            agencyId,
            NameTypes.Person,
            "Doe",
            "Jordan",
            null,
            null,
            null,
            new DateTime(1990, 1, 2),
            "DL-123",
            null,
            70,
            180,
            null,
            null,
            null,
            "Springfield",
            "FBI-1",
            "LOCAL-1",
            "111-22-3333",
            true,
            null,
            primaryPhone: "555-1000",
            primaryPhoneExtension: "12",
            workPhone: "555-2000",
            workPhoneExtension: "34",
            otherPhone: "555-3000",
            otherPhoneExtension: "56");
        name.SetLocations(homeLocation.Id, null, new UserModificationContext(userId));
        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Linked incident",
                IncidentNum = "INC-100",
                LocalNum = "LOC-100"
            }
        });

        db.Locations.Add(homeLocation);
        db.Names.Add(name);
        db.Incidents.Add(incident);
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        var recordNumber = await handler.Handle(
            new CreateArrestCommand(
                NameId: name.Id,
                ArrestedAt: new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc),
                IncidentRecordNumbers: [],
                ArrestNum: "AR-500",
                PrimaryIncidentId: incident.Id),
            CancellationToken.None);

        Assert.Equal(1, db.SaveChangesCallCount);

        var arrest = await db.Arrests.SingleAsync(a => a.RecordNumber == recordNumber);
        Assert.Equal(name.Id, arrest.NameId);
        Assert.Equal(incident.Id, arrest.PrimaryIncidentId);

        var snapshot = await db.ArrestNameSnapshots.SingleAsync(s => s.ArrestId == arrest.Id);
        Assert.Equal(name.Id, snapshot.SourceNameId);
        Assert.Equal(name.RecordNumber, snapshot.SourceNameRecordNumber);
        Assert.Equal("Doe", snapshot.LastOrBusinessName);
        Assert.Equal("Jordan", snapshot.FirstName);
        Assert.Equal(homeLocation.Id, snapshot.PrimaryLocationId);
        Assert.Equal(homeLocation.RecordNumber, snapshot.PrimaryLocationRecordNumber);
        Assert.Equal("123 Main, Metro, TX 12345", snapshot.PrimaryLocationAddress);

        var links = await db.IncidentArrestLinks
            .Where(link => link.ArrestId == arrest.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();

        Assert.Single(links);
        Assert.Contains(incident.Id, links);
    }

    [Fact]
    public async Task Handle_WithLocationId_SetsLocationOnCreatedArrest()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var handler = new CreateArrestHandler(db, tenantProvider, new ArrestFactory(), new UserModificationContext(userId));

        var name = new Name(jurisdictionId, agencyId, NameTypes.Person, "Doe", "Jordan", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
        var location = new Location(jurisdictionId, "Main St", "Springfield", "100");

        db.Names.Add(name);
        db.Locations.Add(location);
        await db.SaveChangesAsync(CancellationToken.None);

        var recordNumber = await handler.Handle(
            new CreateArrestCommand(
                NameId: name.Id,
                ArrestedAt: new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc),
                IncidentRecordNumbers: [],
                ArrestNum: "AR-700",
                LocationId: location.Id),
            CancellationToken.None);

        var arrest = await db.Arrests.SingleAsync(a => a.RecordNumber == recordNumber);
        Assert.Equal(location.Id, arrest.LocationId);
    }
}
