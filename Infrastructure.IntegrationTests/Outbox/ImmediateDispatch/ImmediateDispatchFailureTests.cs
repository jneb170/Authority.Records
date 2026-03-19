using Infrastructure.IntegrationTests.TestInfrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Infrastructure.IntegrationTests.Outbox.ImmediateDispatch;

public sealed class ImmediateDispatchFailureTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteTestAppDbContext> _options;
    private readonly Guid _jurisdictionId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();

    public ImmediateDispatchFailureTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SqliteTestAppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext(new ThrowingDomainEventDispatcher());
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenImmediateDispatchFails_PersistsWriteAndOutboxMessage()
    {
        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = _jurisdictionId,
            AgencyId = _agencyId,
            Details = new IncidentDetails
            {
                Description = "Dispatch failure test",
                IncidentNum = "INC-DISPATCH",
                LocalNum = "LOC-DISPATCH"
            }
        });

        await using (var context = CreateContext(new ThrowingDomainEventDispatcher()))
        {
            context.Incidents.Add(incident);

            var exception = await Record.ExceptionAsync(() => context.SaveChangesAsync());

            Assert.Null(exception);
        }

        await using (var verificationContext = CreateContext(new SilentDomainEventDispatcher()))
        {
            var savedIncident = await verificationContext.Incidents.SingleAsync(i => i.Id == incident.Id);
            Assert.Equal("Dispatch failure test", savedIncident.Description);

            var outboxMessages = await verificationContext.OutboxMessages
                .Where(message => message.AggregateId == incident.Id)
                .ToListAsync();

            Assert.Single(outboxMessages);
            Assert.Null(outboxMessages[0].ProcessedOnUtc);
        }
    }

    private SqliteTestAppDbContext CreateContext(IDomainEventDispatcher dispatcher) =>
        new(
            _options,
            new FixedTenantProvider(_jurisdictionId, _agencyId, Guid.NewGuid()),
            dispatcher);

    public void Dispose()
    {
        _connection.Dispose();
    }

    private sealed class FixedTenantProvider(Guid jurisdictionId, Guid agencyId, Guid userId) : ITenantProvider
    {
        public Guid GetJurisdictionId() => jurisdictionId;

        public Guid GetAgencyId() => agencyId;

        public Guid GetUserId() => userId;

        public void SetJurisdictionId(Guid jurisdictionId)
        {
        }
    }

    private sealed class ThrowingDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated projection failure.");

        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated projection failure.");
    }

    private sealed class SilentDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
