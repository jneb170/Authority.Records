using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Admin.Queries.GetAuditLogs;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Tests.Admin;

public sealed class GetAuditLogsHandlerTests
{
    [Fact]
    public async Task JurisdictionHandler_Filters_To_CurrentJurisdiction()
    {
        await using var db = AuditLogTestDbContext.Create();
        var tenantId = Guid.NewGuid();

        db.AuditLogReadModels.AddRange(
            CreateAuditLog(jurisdictionId: tenantId, recordType: "Incident", actionType: "Created"),
            CreateAuditLog(jurisdictionId: Guid.NewGuid(), recordType: "Arrest", actionType: "Created"),
            CreateAuditLog(jurisdictionId: null, recordType: "System", actionType: "LockExpired"));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetJurisdictionAuditLogsHandler(
            db,
            new FakeTenantProvider(tenantId),
            new FakeUserLookupService());

        var result = await handler.Handle(
            new GetJurisdictionAuditLogsQuery(new AuditLogSearchRequest()),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.All(result.Items, item => Assert.Equal(tenantId, item.JurisdictionId));
    }

    [Fact]
    public async Task SuperHandler_WhenScopeIsSystem_Returns_Only_SystemRows()
    {
        await using var db = AuditLogTestDbContext.Create();

        db.AuditLogReadModels.AddRange(
            CreateAuditLog(jurisdictionId: Guid.NewGuid(), recordType: "Incident", actionType: "Created"),
            CreateAuditLog(jurisdictionId: null, recordType: "System", actionType: "LockExpired"));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSuperAuditLogsHandler(db, new FakeUserLookupService());

        var result = await handler.Handle(
            new GetSuperAuditLogsQuery(new AuditLogSearchRequest(Scope: AuditLogScopes.System)),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Null(result.Items[0].JurisdictionId);
        Assert.Equal("System", result.Items[0].RecordType);
        Assert.Null(result.Items[0].RecordNumber);
        Assert.Null(result.Items[0].NavigationUrl);
    }

    [Fact]
    public async Task SuperHandler_Applies_User_And_Action_Filters()
    {
        await using var db = AuditLogTestDbContext.Create();
        var actorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        db.AuditLogReadModels.AddRange(
            CreateAuditLog(jurisdictionId: Guid.NewGuid(), recordType: "Incident", actionType: "Updated", userId: actorId, occurredOnUtc: DateTime.UtcNow.AddMinutes(-2)),
            CreateAuditLog(jurisdictionId: Guid.NewGuid(), recordType: "Incident", actionType: "Created", userId: actorId, occurredOnUtc: DateTime.UtcNow.AddMinutes(-1)),
            CreateAuditLog(jurisdictionId: Guid.NewGuid(), recordType: "Incident", actionType: "Updated", userId: otherUserId, occurredOnUtc: DateTime.UtcNow));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSuperAuditLogsHandler(
            db,
            new FakeUserLookupService(new Dictionary<Guid, string> { [actorId] = "Casey Admin" }));

        var result = await handler.Handle(
            new GetSuperAuditLogsQuery(new AuditLogSearchRequest(
                ActionType: "Updated",
                UserId: actorId,
                SortField: AuditLogSortFields.OccurredOnUtc,
                SortDescending: false)),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Casey Admin", result.Items[0].ActorDisplayName);
        Assert.Equal("Updated", result.Items[0].ActionType);
        Assert.Equal(actorId, result.Items[0].UserId);
    }

    [Fact]
    public async Task SuperHandler_Populates_RecordNumber_And_NavigationUrl_For_Navigable_RecordTypes()
    {
        await using var db = AuditLogTestDbContext.Create();
        var incidentId = Guid.NewGuid();

        db.IncidentReadModels.Add(CreateIncidentReadModel(incidentId, 12345));
        db.AuditLogReadModels.Add(CreateAuditLog(
            jurisdictionId: Guid.NewGuid(),
            recordType: "Incident",
            actionType: "Created",
            aggregateId: incidentId));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSuperAuditLogsHandler(db, new FakeUserLookupService());

        var result = await handler.Handle(
            new GetSuperAuditLogsQuery(new AuditLogSearchRequest()),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(12345, result.Items[0].RecordNumber);
        Assert.Equal("/incidents/12345", result.Items[0].NavigationUrl);
    }

    [Fact]
    public async Task SuperHandler_Filters_By_RecordNumber()
    {
        await using var db = AuditLogTestDbContext.Create();
        var matchingIncidentId = Guid.NewGuid();
        var otherIncidentId = Guid.NewGuid();

        db.IncidentReadModels.AddRange(
            CreateIncidentReadModel(matchingIncidentId, 12345),
            CreateIncidentReadModel(otherIncidentId, 54321));

        db.AuditLogReadModels.AddRange(
            CreateAuditLog(
                jurisdictionId: Guid.NewGuid(),
                recordType: "Incident",
                actionType: "Created",
                aggregateId: matchingIncidentId),
            CreateAuditLog(
                jurisdictionId: Guid.NewGuid(),
                recordType: "Incident",
                actionType: "Updated",
                aggregateId: otherIncidentId));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetSuperAuditLogsHandler(db, new FakeUserLookupService());

        var result = await handler.Handle(
            new GetSuperAuditLogsQuery(new AuditLogSearchRequest(RecordNumber: 12345)),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(12345, result.Items[0].RecordNumber);
        Assert.Equal(matchingIncidentId, result.Items[0].AggregateId);
    }

    private static AuditLogReadModel CreateAuditLog(
        Guid? jurisdictionId,
        string recordType,
        string actionType,
        Guid? userId = null,
        DateTime? occurredOnUtc = null,
        Guid? aggregateId = null)
    {
        return AuditLogReadModel.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"{recordType}{actionType}DomainEvent",
            AuditLogSeverities.Information,
            occurredOnUtc ?? DateTime.UtcNow,
            jurisdictionId,
            aggregateId ?? Guid.NewGuid(),
            1,
            recordType,
            actionType,
            userId,
            $"{recordType} {actionType.ToLowerInvariant()}.",
            "{}");
    }

    private static IncidentReadModel CreateIncidentReadModel(Guid id, long recordNumber)
    {
        return IncidentReadModel.Create(
            id,
            recordNumber,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Modules.Records.Domain.ValueObjects.IncidentDetails
            {
                IncidentNum = "INC-12345",
                LocalNum = string.Empty,
                Description = "Test incident",
                CFSNum = string.Empty
            },
            Modules.Records.Domain.Common.RecordStatus.Draft,
            DateTime.UtcNow,
            Guid.NewGuid());
    }

    private sealed class AuditLogTestDbContext : DbContext, IApplicationDbContext
    {
        public DbSet<AuditLogReadModel> AuditLogReadModels { get; set; } = null!;

        public DbSet<Incident> Incidents => throw new NotImplementedException();
        public IQueryable<Incident> AllIncidentsWithDeleted => throw new NotImplementedException();
        public DbSet<Arrest> Arrests => throw new NotImplementedException();
        public DbSet<ArrestNameSnapshot> ArrestNameSnapshots => throw new NotImplementedException();
        public DbSet<Citation> Citations => throw new NotImplementedException();
        public DbSet<Charge> Charges => throw new NotImplementedException();
        public DbSet<Name> Names => throw new NotImplementedException();
        public DbSet<Location> Locations => throw new NotImplementedException();
        public DbSet<Mugshot> Mugshots => throw new NotImplementedException();
        public DbSet<MugshotLink> MugshotLinks => throw new NotImplementedException();
        public DbSet<IncidentArrestLink> IncidentArrestLinks => throw new NotImplementedException();
        public DbSet<IncidentCitationLink> IncidentCitationLinks => throw new NotImplementedException();
        public DbSet<ArrestChargeLink> ArrestChargeLinks => throw new NotImplementedException();
        public DbSet<CitationChargeLink> CitationChargeLinks => throw new NotImplementedException();
        public DbSet<IncidentChargeLink> IncidentChargeLinks => throw new NotImplementedException();
        public DbSet<IncidentReadModel> IncidentReadModels { get; set; } = null!;
        public DbSet<ArrestReadModel> ArrestReadModels { get; set; } = null!;
        public DbSet<CitationReadModel> CitationReadModels { get; set; } = null!;
        public DbSet<NameReadModel> NameReadModels { get; set; } = null!;
        public DbSet<LocationReadModel> LocationReadModels { get; set; } = null!;
        public DbSet<MugshotReadModel> MugshotReadModels => throw new NotImplementedException();
        public DbSet<MugshotLinkReadModel> MugshotLinkReadModels => throw new NotImplementedException();
        public DbSet<IncidentArrestLinkReadModel> IncidentArrestLinkReadModels => throw new NotImplementedException();
        public DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels => throw new NotImplementedException();
        public DbSet<AgencyConfiguration> AgencyConfigurations => throw new NotImplementedException();
        public DbSet<AgencySequenceCounter> AgencySequenceCounters => throw new NotImplementedException();
        public DbSet<PicklistItem> PicklistItems => throw new NotImplementedException();
        public DbSet<PicklistSetting> PicklistSettings => throw new NotImplementedException();

        public AuditLogTestDbContext(DbContextOptions<AuditLogTestDbContext> options) : base(options) { }

        public void Detach<TEntity>(TEntity entity) where TEntity : class =>
            Entry(entity).State = EntityState.Detached;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IncidentReadModel>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.RecordNumber);
                builder.Property(x => x.IncidentNum).IsRequired();
                builder.Property(x => x.LocalNum).IsRequired();
                builder.Property(x => x.Description).IsRequired();
                builder.Property(x => x.CFSNum).IsRequired();
                builder.Property(x => x.Status).IsRequired();
            });

            modelBuilder.Entity<ArrestReadModel>(builder => builder.HasKey(x => x.Id));
            modelBuilder.Entity<CitationReadModel>(builder => builder.HasKey(x => x.Id));
            modelBuilder.Entity<NameReadModel>(builder => builder.HasKey(x => x.Id));
            modelBuilder.Entity<LocationReadModel>(builder => builder.HasKey(x => x.Id));

            modelBuilder.Entity<AuditLogReadModel>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.EventType).IsRequired();
                builder.Property(x => x.Severity).IsRequired();
                builder.Property(x => x.RecordType).IsRequired();
                builder.Property(x => x.ActionType).IsRequired();
                builder.Property(x => x.Message).IsRequired();
                builder.Property(x => x.Payload).IsRequired();
            });
        }

        public static AuditLogTestDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AuditLogTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AuditLogTestDbContext(options);
        }
    }

    private sealed class FakeTenantProvider : ITenantProvider
    {
        private Guid _jurisdictionId;

        public FakeTenantProvider(Guid jurisdictionId) => _jurisdictionId = jurisdictionId;

        public Guid GetJurisdictionId() => _jurisdictionId;
        public Guid GetAgencyId() => Guid.NewGuid();
        public Guid GetUserId() => Guid.NewGuid();
        public void SetJurisdictionId(Guid jurisdictionId) => _jurisdictionId = jurisdictionId;
    }

    private sealed class FakeUserLookupService : IUserLookupService
    {
        private readonly IReadOnlyDictionary<Guid, string> _names;

        public FakeUserLookupService(IReadOnlyDictionary<Guid, string>? names = null) =>
            _names = names ?? new Dictionary<Guid, string>();

        public Task<string?> GetDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_names.TryGetValue(userId, out var name) ? name : null);

        public Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(userIds
                .Distinct()
                .Where(_names.ContainsKey)
                .ToDictionary(id => id, id => _names[id]));
    }
}
