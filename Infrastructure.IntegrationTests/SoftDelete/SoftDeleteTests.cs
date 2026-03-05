using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.SoftDelete;

public class SoftDeleteTests
{
    private readonly DbContextOptions<AppDbContext> _options;

    public SoftDeleteTests()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(_options, new FakeTenantProvider(), new FakeDomainEventDispatcher());
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Should_Filter_Out_SoftDeleted_Records()
    {
        var incidentJurisdictionId = Guid.NewGuid();
        var incidentAgencyId = Guid.NewGuid();

        UserModificationContext userModificationContext = new(Guid.NewGuid(), false, false, false);
        var incident = new IncidentFactory().Create(incidentJurisdictionId, incidentAgencyId, "Test Incident");

        // Insert a record
        using (var context = new AppDbContext(_options, new FakeTenantProvider(), new FakeDomainEventDispatcher()))
        {
            context.Incidents.Add(incident);
            await context.SaveChangesAsync();

            // Soft delete it
            incident.SoftDelete(userModificationContext.UserId);
            await context.SaveChangesAsync();
        }

        // Regular query should exclude
        using (var context = new AppDbContext(_options, new FakeTenantProvider(), new FakeDomainEventDispatcher()))
        {
            var incidents = await context.Incidents.ToListAsync();
            Assert.Empty(incidents); // soft deleted records are filtered out
        }

        // IgnoreQueryFilters retrieves it
        using (var context = new AppDbContext(_options, new FakeTenantProvider(), new FakeDomainEventDispatcher()))
        {
            var allWithDeleted = await context.Incidents.IgnoreQueryFilters().ToListAsync();
            Assert.Contains(allWithDeleted, i => i.Id == incident.Id);
        }
    }

    private class FakeTenantProvider : ITenantProvider
    {
        public Guid GetAgencyId()
        {
            throw new NotImplementedException();
        }

        public Guid GetJurisdictionId() => Guid.NewGuid();

        public Guid GetUserId()
        {
            throw new NotImplementedException();
        }

        public void SetJurisdictionId(Guid jurisdictionId)
        {
            throw new NotImplementedException();
        }
    }

    private class FakeDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, 
            CancellationToken cancellationToken = default) 
            => Task.CompletedTask;

        public Task DispatchAsync(IDomainEvent domainEvent, 
            CancellationToken cancellationToken = default) 
            => Task.CompletedTask;
    }
}
