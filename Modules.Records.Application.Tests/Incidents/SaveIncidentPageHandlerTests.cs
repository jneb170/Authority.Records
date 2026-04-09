using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Incidents.Commands.SaveIncidentPage;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Incidents;

public sealed class SaveIncidentPageHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesIncidentAndChargeLinks_WithSingleSaveCall()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveIncidentPageHandler(db, tenantProvider, modificationContext);

        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Original incident",
                IncidentNum = "INC-ORIG",
                LocalNum = "LOC-ORIG"
            }
        });

        var chargeToRemove = new Charge(jurisdictionId, agencyId, "Remove", "Cat", "Group", "Person", "001", "Misdemeanor", null, false);
        var chargeToAdd = new Charge(jurisdictionId, agencyId, "Add", "Cat", "Group", "Person", "002", "Felony", null, false);

        db.Incidents.Add(incident);
        db.Charges.AddRange(chargeToRemove, chargeToAdd);
        db.IncidentChargeLinks.Add(new IncidentChargeLink(jurisdictionId, incident.Id, chargeToRemove.Id, userId));
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        var command = new SaveIncidentPageCommand(
            incident.Id,
            new IncidentDetails
            {
                Description = "Updated incident",
                IncidentNum = "INC-200",
                LocalNum = "LOC-1",
                CFSNum = "CFS-1"
            },
            locationId,
            new DateTime(2026, 3, 18, 8, 30, 0, DateTimeKind.Utc),
            [chargeToAdd.Id],
            [chargeToRemove.Id]);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, db.SaveChangesCallCount);

        var savedIncident = await db.Incidents.SingleAsync(i => i.Id == incident.Id);
        Assert.Equal("Updated incident", savedIncident.Description);
        Assert.Equal("INC-200", savedIncident.IncidentNum);
        Assert.Equal(locationId, savedIncident.LocationId);

        var chargeLinks = await db.IncidentChargeLinks
            .Where(link => link.IncidentId == incident.Id)
            .Select(link => link.ChargeId)
            .ToListAsync();

        Assert.Single(chargeLinks);
        Assert.Contains(chargeToAdd.Id, chargeLinks);
    }
}
