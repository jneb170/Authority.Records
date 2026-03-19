using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Citations.Commands.CreateCitation;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Citations;

public sealed class CreateCitationHandlerTests
{
    [Fact]
    public async Task Handle_CreatesCitationAndIncidentLinks_WithSingleSaveCall()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var handler = new CreateCitationHandler(db, tenantProvider);

        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Linked incident",
                IncidentNum = "INC-200",
                LocalNum = "LOC-200"
            }
        });

        db.Incidents.Add(incident);
        db.Entry(incident).Property(nameof(Incident.RecordNumber)).CurrentValue = 2001L;
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        var recordNumber = await handler.Handle(
            new CreateCitationCommand(
                Description: "Created citation",
                IssueDate: new DateTime(2026, 3, 2, 8, 0, 0, DateTimeKind.Utc),
                IncidentRecordNumbers: [2001L, 2001L],
                CitationNum: "CT-500"),
            CancellationToken.None);

        Assert.Equal(1, db.SaveChangesCallCount);

        var citation = await db.Citations.SingleAsync(c => c.RecordNumber == recordNumber);
        Assert.Equal("Created citation", citation.Description);

        var links = await db.IncidentCitationLinks
            .Where(link => link.CitationId == citation.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();

        Assert.Single(links);
        Assert.Contains(incident.Id, links);
    }
}
