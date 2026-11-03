using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;
using Modules.Records.Application.Incidents.Commands.AcquireIncidentLock;
using Modules.Records.Application.Incidents.Commands.RestoreIncident;
using Modules.Records.Application.Names.Commands.AcquireNameLock;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.Narratives.Commands.CreateNarrative;
using Modules.Records.Application.Narratives.Queries.GetNarrativesByOwner;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Persistence;
using Infrastructure.IntegrationTests.TestInfrastructure;

namespace Infrastructure.IntegrationTests.AgencyIsolation;

/// <summary>
/// Issue #118: two agencies in one jurisdiction. Incidents, arrests and citations (and the
/// narratives attached to them) belong to one agency; names and locations are the shared
/// master index. Every check runs real handlers as a user of the other agency.
/// </summary>
public sealed class AgencyIsolationTests : IDisposable
{
    private static readonly Guid JurisdictionId = Guid.NewGuid();
    private static readonly Guid AgencyA = Guid.NewGuid();
    private static readonly Guid AgencyB = Guid.NewGuid();
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;
    private readonly Actor _actor = new();

    public AgencyIsolationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext, SqliteTestAppDbContext>(o => o.UseSqlite(_connection));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton(_actor);
        services.AddScoped<ITenantProvider>(sp => new ActorTenantProvider(sp.GetRequiredService<Actor>()));
        services.AddScoped<IModificationContext>(sp => new UserModificationContext(sp.GetRequiredService<Actor>().UserId));
        services.AddScoped<ICurrentUserContext, NotDemoUser>();
        services.AddScoped<IUserLookupService, NoUserNames>();
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.AddApplication();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task Other_agency_cannot_see_or_change_incidents_arrests_or_citations()
    {
        var (incidentId, arrestId, citationId) = await SeedAgencyARecordsAsync();

        ActAs(UserB, AgencyB);
        await using (var db = CreateDb())
        {
            Assert.False(await db.Incidents.AnyAsync(i => i.Id == incidentId));
            Assert.False(await db.Arrests.AnyAsync(a => a.Id == arrestId));
            Assert.False(await db.Citations.AnyAsync(c => c.Id == citationId));
        }

        // Commands load through the same filter, so a known ID is not enough to act on it.
        await Assert.ThrowsAsync<InvalidOperationException>(() => Send(new AcquireIncidentLockCommand(incidentId)));

        // Narratives inherit their owner's agency.
        Assert.Empty(await Send(new GetNarrativesByOwnerQuery(NarrativeOwnerTypes.Incident, incidentId)));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Send(new CreateNarrativeCommand(NarrativeOwnerTypes.Incident, incidentId, "Supplement", "From agency B")));

        // The owning agency still sees everything.
        ActAs(UserA, AgencyA);
        await using (var db = CreateDb())
        {
            Assert.True(await db.Incidents.AnyAsync(i => i.Id == incidentId));
            Assert.True(await db.Arrests.AnyAsync(a => a.Id == arrestId));
            Assert.True(await db.Citations.AnyAsync(c => c.Id == citationId));
        }
        Assert.Single(await Send(new GetNarrativesByOwnerQuery(NarrativeOwnerTypes.Incident, incidentId)));
    }

    [Fact]
    public async Task Other_agency_cannot_restore_a_deleted_record()
    {
        var (incidentId, _, _) = await SeedAgencyARecordsAsync();
        await using (var db = CreateDb())
        {
            var incident = await db.Incidents.SingleAsync(i => i.Id == incidentId);
            incident.SoftDelete(UserA);
            await db.SaveChangesAsync();
        }

        // Restore skips only the soft-delete filter, never the agency filter.
        ActAs(UserB, AgencyB);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Send(new RestoreIncidentCommand(incidentId)));

        ActAs(UserA, AgencyA);
        await Send(new RestoreIncidentCommand(incidentId));
    }

    [Fact]
    public async Task Names_are_shared_across_agencies_in_the_jurisdiction()
    {
        ActAs(UserA, AgencyA);
        var recordNumber = await Send(new CreateNameCommand(
            "Individual", "Carter", "Marcus", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, false, null));

        ActAs(UserB, AgencyB);
        Guid nameId;
        await using (var db = CreateDb())
        {
            nameId = await db.Names.Where(n => n.RecordNumber == recordNumber).Select(n => n.Id).SingleAsync();
        }

        // Agency B can open and lock agency A's master name record.
        await Send(new AcquireNameLockCommand(nameId));
        await using (var db = CreateDb())
        {
            Assert.Equal(UserB, (await db.Names.SingleAsync(n => n.Id == nameId)).LockedByUserId);
        }
    }

    [Fact]
    public async Task Rebuild_by_one_agency_keeps_the_other_agencys_read_models()
    {
        var (incidentId, arrestId, citationId) = await SeedAgencyARecordsAsync();

        ActAs(UserB, AgencyB);
        await Send(new RebuildReadModelsCommand());

        await using var db = CreateDb();
        Assert.True(await db.IncidentReadModels.AnyAsync(r => r.Id == incidentId));
        Assert.True(await db.ArrestReadModels.AnyAsync(r => r.Id == arrestId));
        Assert.True(await db.CitationReadModels.AnyAsync(r => r.Id == citationId));
    }

    [Fact]
    public async Task Audit_log_shows_own_agency_and_shared_master_index_only()
    {
        var (incidentId, _, _) = await SeedAgencyARecordsAsync();
        ActAs(UserA, AgencyA);
        var nameRecordNumber = await Send(new CreateNameCommand(
            "Individual", "Shared", "Person", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, false, null));

        ActAs(UserB, AgencyB);
        var asB = await Send(new GetJurisdictionAuditLogsQuery(new AuditLogSearchRequest(PageSize: 200)));
        Assert.DoesNotContain(asB.Items, x => x.AggregateId == incidentId);
        Assert.DoesNotContain(asB.Items, x => x.RecordType is "Incident" or "Arrest" or "Citation" or "Narrative");
        Assert.Contains(asB.Items, x => x.RecordType == "Name");

        ActAs(UserA, AgencyA);
        var asA = await Send(new GetJurisdictionAuditLogsQuery(new AuditLogSearchRequest(PageSize: 200)));
        Assert.Contains(asA.Items, x => x.AggregateId == incidentId);
        Assert.Contains(asA.Items, x => x.RecordType == "Narrative");
        Assert.Contains(asA.Items, x => x.RecordType == "Name");
    }

    [Fact]
    public async Task Background_work_without_a_user_is_jurisdiction_wide()
    {
        var (incidentId, _, _) = await SeedAgencyARecordsAsync();

        ActAs(Guid.Empty, Guid.Empty);
        await using var db = CreateDb();
        Assert.True(await db.Incidents.AnyAsync(i => i.Id == incidentId));
    }

    private async Task<(Guid IncidentId, Guid ArrestId, Guid CitationId)> SeedAgencyARecordsAsync()
    {
        ActAs(UserA, AgencyA);

        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = JurisdictionId,
            AgencyId = AgencyA,
            Details = new IncidentDetails { IncidentNum = "26-" + Random.Shared.Next(100000, 999999), Description = "A's incident", LocalNum = "" }
        });
        var arrest = new ArrestFactory().Create(JurisdictionId, AgencyA, nameId: null, DateTime.UtcNow, "AR-1");
        var citation = new Modules.Records.Domain.Entities.Citation(JurisdictionId, AgencyA, "Speeding", DateTime.UtcNow, "CT-1");

        await using (var db = CreateDb())
        {
            db.Incidents.Add(incident);
            db.Arrests.Add(arrest);
            db.Citations.Add(citation);
            await db.SaveChangesAsync();
        }

        await Send(new CreateNarrativeCommand(NarrativeOwnerTypes.Incident, incident.Id, "Report", "A's narrative"));
        await Send(new RebuildReadModelsCommand());

        return (incident.Id, arrest.Id, citation.Id);
    }

    private void ActAs(Guid userId, Guid agencyId)
    {
        _actor.UserId = userId;
        _actor.AgencyId = agencyId;
    }

    private AppDbContext CreateDb() => _provider.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

    private async Task<T> Send<T>(IRequest<T> request)
    {
        using var scope = _provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    private async Task Send(IRequest request)
    {
        using var scope = _provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }

    private sealed class Actor
    {
        public Guid UserId { get; set; }
        public Guid AgencyId { get; set; }
    }

    private sealed class ActorTenantProvider(Actor actor) : ITenantProvider
    {
        public Guid GetJurisdictionId() => JurisdictionId;
        public Guid GetAgencyId() => actor.AgencyId;
        public Guid GetUserId() => actor.UserId;
        public void SetJurisdictionId(Guid jurisdictionId) { }
    }

    /// <summary>Each test user is an admin of their own agency (rebuild is Admin-only, #125).</summary>
    private sealed class NotDemoUser : ICurrentUserContext
    {
        public bool IsInRole(string roleName) => roleName == RoleNames.Admin;
        public bool IsDemoUser => false;
    }

    private sealed class NoUserNames : IUserLookupService
    {
        public Task<string?> GetDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);

        public Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new Dictionary<Guid, string>());
    }
}
