using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Common;
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
        var handler = new CreateArrestHandler(db, tenantProvider, new ArrestFactory());

        var name = new Name(jurisdictionId, agencyId, NameTypes.Person, "Doe", "Jordan", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false, null);
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

        var links = await db.IncidentArrestLinks
            .Where(link => link.ArrestId == arrest.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();

        Assert.Single(links);
        Assert.Contains(incident.Id, links);
    }
}
