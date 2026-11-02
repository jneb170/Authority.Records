using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;
using Infrastructure.IntegrationTests.Common;
using Infrastructure.IntegrationTests.TestInfrastructure;

namespace Infrastructure.IntegrationTests.Audit;

/// <summary>
/// Regression tests for issue #119: every audit row names the user who executed that command,
/// never the aggregate's previous modifier and never "System" for a signed-in user.
/// </summary>
public sealed class AuditAttributionTests : IDisposable
{
    private static readonly Guid JurisdictionId = Guid.NewGuid();
    private static readonly Guid AgencyId = Guid.NewGuid();
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteTestAppDbContext> _options;
    private readonly SwitchableUserTenantProvider _tenant = new(JurisdictionId, AgencyId);
    private readonly StubUserLookup _users = new(new()
    {
        [UserA] = "Visitor A",
        [UserB] = "Visitor B"
    });

    public AuditAttributionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<SqliteTestAppDbContext>().UseSqlite(_connection).Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Delete_by_another_user_is_attributed_to_the_deleter_not_the_lock_holder()
    {
        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = JurisdictionId,
            AgencyId = AgencyId,
            Details = new IncidentDetails { IncidentNum = "26-000001", Description = "Test", LocalNum = "" }
        });

        // User A creates the incident, takes the lock and saves a change.
        _tenant.UserId = UserA;
        await using (var context = CreateContext())
        {
            context.Incidents.Add(incident);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var tracked = await context.Incidents.SingleAsync(i => i.Id == incident.Id);
            var contextA = new UserModificationContext(UserA);
            tracked.AcquireLock(contextA, TimeSpan.FromMinutes(10));
            tracked.UpdateDetails(
                new IncidentDetails { IncidentNum = "26-000001", Description = "Edited by A", LocalNum = "" },
                occurredOn: null,
                contextA);
            await context.SaveChangesAsync();
        }

        // User B deletes it while A still holds the lock.
        _tenant.UserId = UserB;
        await using (var context = CreateContext())
        {
            var tracked = await context.Incidents.SingleAsync(i => i.Id == incident.Id);
            tracked.SoftDelete(UserB);
            await context.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var rows = await read.AuditLogReadModels
            .Where(x => x.AggregateId == incident.Id)
            .ToListAsync();

        var created = Assert.Single(rows, x => x.ActionType == "Created");
        Assert.Equal(UserA, created.UserId);
        Assert.Equal("Visitor A", created.UserDisplayName);

        var updated = Assert.Single(rows, x => x.ActionType == "Updated");
        Assert.Equal(UserA, updated.UserId);

        var lockAcquired = Assert.Single(rows, x => x.ActionType == "LockAcquired");
        Assert.Equal(UserA, lockAcquired.UserId);
        Assert.Equal("IncidentLockAcquired", lockAcquired.EventType);

        var deleted = Assert.Single(rows, x => x.ActionType == "SoftDeleted");
        Assert.Equal(UserB, deleted.UserId);
        Assert.Equal("Visitor B", deleted.UserDisplayName);
        Assert.Contains(UserB.ToString(), deleted.Payload);
    }

    [Fact]
    public async Task Save_without_a_signed_in_user_is_recorded_as_system()
    {
        _tenant.UserId = Guid.Empty;
        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = JurisdictionId,
            AgencyId = AgencyId,
            Details = new IncidentDetails { IncidentNum = "26-000002", Description = "Seeded", LocalNum = "" }
        });

        await using (var context = CreateContext())
        {
            context.Incidents.Add(incident);
            await context.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var created = await read.AuditLogReadModels.SingleAsync(x => x.AggregateId == incident.Id);
        Assert.Null(created.UserId);
        Assert.Null(created.UserDisplayName);
    }

    [Theory]
    [InlineData("LifecycleStatusChangedDomainEvent`1", "Incident", "IncidentStatusChanged")]
    [InlineData("LockReleasedDomainEvent`1", "Arrest", "ArrestLockReleased")]
    [InlineData("NameUpdatedDomainEvent", "Name", "NameUpdated")]
    [InlineData("IncidentStatusChanged", "Incident", "IncidentStatusChanged")]
    public void Stored_event_types_are_shown_readably(string stored, string recordType, string expected)
    {
        Assert.Equal(expected, AuditEventTypeFormatter.Format(stored, recordType));
    }

    private SqliteTestAppDbContext CreateContext() =>
        new(_options, _tenant, new TestDomainEventDispatcher(), _users);

    public void Dispose() => _connection.Dispose();

    private sealed class SwitchableUserTenantProvider(Guid jurisdictionId, Guid agencyId) : ITenantProvider
    {
        public Guid UserId { get; set; }
        public Guid GetJurisdictionId() => jurisdictionId;
        public Guid GetAgencyId() => agencyId;
        public Guid GetUserId() => UserId;
        public void SetJurisdictionId(Guid id) { }
    }

    private sealed class StubUserLookup(Dictionary<Guid, string> names) : IUserLookupService
    {
        public Task<string?> GetDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(names.TryGetValue(userId, out var n) ? n : null);

        public Task<Dictionary<Guid, string>> GetDisplayNamesAsync(
            IEnumerable<Guid> userIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(userIds.Where(names.ContainsKey).Distinct().ToDictionary(id => id, id => names[id]));
    }
}
