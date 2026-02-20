using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Entities;
using Infrastructure.IntegrationTests.TestInfrastructure;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Infrastructure.IntegrationTests;

public class RowVersionConcurrencyTests
{
    [Fact]
    public async Task Should_Throw_DbUpdateConcurrencyException_When_RowVersion_Mismatch()
    {
        var tenantId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();

        using var factory = new SqliteTestDbContextFactory(tenantId);

        Guid incidentId;

        // Seed
        using (var context = factory.CreateContext())
        {
            var incident = new Incident(tenantId, agencyId, "Original");
            context.Incidents.Add(incident);
            await context.SaveChangesAsync();
            incidentId = incident.Id;
        }

        using var contextA = factory.CreateContext();
        using var contextB = factory.CreateContext();

        var incidentA = await contextA.Incidents.FirstAsync(x => x.Id == incidentId);
        var incidentB = await contextB.Incidents.FirstAsync(x => x.Id == incidentId);

        incidentA.UpdateDescription("Updated A");
        await contextA.SaveChangesAsync();

        var fresh = await contextA.Incidents
            .AsNoTracking()
            .FirstAsync(x => x.Id == incidentId);

        incidentB.UpdateDescription("Updated B");

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            contextB.SaveChangesAsync());
    }

}
