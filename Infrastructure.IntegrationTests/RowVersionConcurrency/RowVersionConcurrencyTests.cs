using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Entities;
using Infrastructure.IntegrationTests.TestInfrastructure;
using System;
using System.Threading.Tasks;
using Xunit;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.Common.Implementations;

namespace Infrastructure.IntegrationTests.RowVersionConcurrency;

public class RowVersionConcurrencyTests
{
    [Fact]
    public async Task Should_Throw_DbUpdateConcurrencyException_When_RowVersion_Mismatch()
    {
        var tenantId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        UserModificationContext userModificationContext = new(Guid.NewGuid(), false, false, false);

        using var factory = new SqliteTestDbContextFactory(tenantId);

        Guid incidentId;

        // Seed
        using (var context = factory.CreateContext())
        {
            var incident = new IncidentFactory().Create(new CreateIncidentRequest
            {
                JurisdictionId = tenantId,
                AgencyId       = agencyId,
                Details        = new Modules.Records.Domain.ValueObjects.IncidentDetails { IncidentNum = "INC-001", Description = "Original", LocalNum = "" },
            });
            context.Incidents.Add(incident);
            await context.SaveChangesAsync();
            incidentId = incident.Id;
        }

        using var contextA = factory.CreateContext();
        using var contextB = factory.CreateContext();

        var incidentA = await contextA.Incidents.FirstAsync(x => x.Id == incidentId);
        var incidentB = await contextB.Incidents.FirstAsync(x => x.Id == incidentId);

        incidentA.UpdateDetails(new Modules.Records.Domain.ValueObjects.IncidentDetails { IncidentNum = "INC-001", Description = "Updated A", LocalNum = "" }, userModificationContext);
        await contextA.SaveChangesAsync();

        var fresh = await contextA.Incidents
            .AsNoTracking()
            .FirstAsync(x => x.Id == incidentId);

        incidentB.UpdateDetails(new Modules.Records.Domain.ValueObjects.IncidentDetails { IncidentNum = "INC-001", Description = "Updated B", LocalNum = "" }, userModificationContext);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            contextB.SaveChangesAsync());
    }

}

