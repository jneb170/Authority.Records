using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Citations;

public sealed class SaveCitationPageHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesCitationLinksAndCharges_WithSingleSaveCall()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveCitationPageHandler(db, tenantProvider, modificationContext);

        var citation = new Citation(jurisdictionId, agencyId, "Original citation", DateTime.UtcNow.AddDays(-2), "CT-1");
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
        var chargeToRemove = new Charge(jurisdictionId, agencyId, "Remove", "Cat", "Group", "Person", "020", "Misdemeanor", null, true);
        var chargeToAdd = new Charge(jurisdictionId, agencyId, "Add", "Cat", "Group", "Person", "021", "Felony", null, true);

        db.Citations.Add(citation);
        db.Incidents.AddRange(incidentToRemove, incidentToAdd);
        db.Charges.AddRange(chargeToRemove, chargeToAdd);
        db.IncidentCitationLinks.Add(new IncidentCitationLink(jurisdictionId, incidentToRemove.Id, citation.Id, userId));
        db.CitationChargeLinks.Add(new CitationChargeLink(jurisdictionId, citation.Id, chargeToRemove.Id, userId));
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        var command = new SaveCitationPageCommand(
            citation.Id,
            "Updated citation",
            DateTime.UtcNow.AddHours(-3),
            Guid.NewGuid(),
            "CT-200",
            locationId,
            [incidentToAdd.Id],
            [incidentToRemove.Id],
            [chargeToAdd.Id],
            [chargeToRemove.Id]);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, db.SaveChangesCallCount);

        var savedCitation = await db.Citations.SingleAsync(c => c.Id == citation.Id);
        Assert.Equal("Updated citation", savedCitation.Description);
        Assert.Equal("CT-200", savedCitation.CitationNum);
        Assert.Equal(locationId, savedCitation.LocationId);

        var linkedIncidentIds = await db.IncidentCitationLinks
            .Where(link => link.CitationId == citation.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();
        Assert.Single(linkedIncidentIds);
        Assert.Contains(incidentToAdd.Id, linkedIncidentIds);

        var chargeLinkIds = await db.CitationChargeLinks
            .Where(link => link.CitationId == citation.Id)
            .Select(link => link.ChargeId)
            .ToListAsync();
        Assert.Single(chargeLinkIds);
        Assert.Contains(chargeToAdd.Id, chargeLinkIds);
    }
}
