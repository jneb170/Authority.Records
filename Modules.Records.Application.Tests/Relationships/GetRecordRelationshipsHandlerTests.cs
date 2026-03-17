using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Application.Relationships.Queries.GetRecordRelationships;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Relationships;

public sealed class GetRecordRelationshipsHandlerTests
{
    [Fact]
    public async Task IncidentQuery_Returns_Location_Arrests_And_Citations()
    {
        await using var db = RelationshipTestDbContext.Create();
        var jurisdictionId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var incidentId = Guid.NewGuid();
        var arrestId = Guid.NewGuid();
        var citationId = Guid.NewGuid();
        var nameId = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationId, 6001, jurisdictionId, "Central Precinct"));
        db.IncidentReadModels.Add(CreateIncident(incidentId, 1001, jurisdictionId, locationId, "INC-1001", "Burglary"));
        db.NameReadModels.Add(CreateName(nameId, 3001, jurisdictionId, "Mills", "Casey"));
        db.ArrestReadModels.Add(CreateArrest(arrestId, 2001, jurisdictionId, nameId, "AR-2001"));
        db.CitationReadModels.Add(CreateCitation(citationId, 4001, jurisdictionId, "Noise violation", "CT-4001"));
        db.IncidentArrestLinkReadModels.Add(IncidentArrestLinkReadModel.Create(Guid.NewGuid(), jurisdictionId, incidentId, 1001, "INC-1001", arrestId, DateTime.UtcNow));
        db.IncidentCitationLinkReadModels.Add(IncidentCitationLinkReadModel.Create(Guid.NewGuid(), jurisdictionId, incidentId, 1001, "INC-1001", citationId, DateTime.UtcNow));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRecordRelationshipsHandler(db, new FakeTenantProvider(jurisdictionId));

        var result = await handler.Handle(
            new GetRecordRelationshipsQuery(RecordRelationshipRecordTypes.Incident, 1001),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("INC-1001", result.Source.Title);
        Assert.Contains(result.Groups, g => g.Title == "Location" && g.Items.Single().NavigationUrl == "/locations/6001");
        Assert.Contains(result.Groups, g => g.Title == "Linked Arrests" && g.Items.Single().NavigationUrl == "/arrests/2001");
        Assert.Contains(result.Groups, g => g.Title == "Linked Citations" && g.Items.Single().NavigationUrl == "/citations/4001");
    }

    [Fact]
    public async Task ArrestQuery_Returns_PrimaryIncident_LinkedIncidents_Suspect_And_Location()
    {
        await using var db = RelationshipTestDbContext.Create();
        var jurisdictionId = Guid.NewGuid();
        var primaryIncidentId = Guid.NewGuid();
        var linkedIncidentId = Guid.NewGuid();
        var arrestId = Guid.NewGuid();
        var nameId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationId, 6001, jurisdictionId, "Station"));
        db.IncidentReadModels.Add(CreateIncident(primaryIncidentId, 1001, jurisdictionId, null, "INC-1001", "Robbery"));
        db.IncidentReadModels.Add(CreateIncident(linkedIncidentId, 1002, jurisdictionId, null, "INC-1002", "Trespassing"));
        db.NameReadModels.Add(CreateName(nameId, 3001, jurisdictionId, "Smith", "John"));
        db.ArrestReadModels.Add(CreateArrest(arrestId, 2001, jurisdictionId, nameId, "AR-2001", primaryIncidentId, locationId));
        db.IncidentArrestLinkReadModels.Add(IncidentArrestLinkReadModel.Create(Guid.NewGuid(), jurisdictionId, linkedIncidentId, 1002, "INC-1002", arrestId, DateTime.UtcNow));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRecordRelationshipsHandler(db, new FakeTenantProvider(jurisdictionId));

        var result = await handler.Handle(
            new GetRecordRelationshipsQuery(RecordRelationshipRecordTypes.Arrest, 2001),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("AR-2001", result.Source.Title);
        Assert.Contains(result.Groups, g => g.Title == "Primary Incident" && g.Items.Single().NavigationUrl == "/incidents/1001");
        Assert.Contains(result.Groups, g => g.Title == "Linked Incidents" && g.Items.Single().NavigationUrl == "/incidents/1002");
        Assert.Contains(result.Groups, g => g.Title == "Suspect" && g.Items.Single().NavigationUrl == "/names/3001");
        Assert.Contains(result.Groups, g => g.Title == "Location" && g.Items.Single().NavigationUrl == "/locations/6001");
    }

    [Fact]
    public async Task CitationQuery_Returns_LinkedIncidents_And_Location()
    {
        await using var db = RelationshipTestDbContext.Create();
        var jurisdictionId = Guid.NewGuid();
        var citationId = Guid.NewGuid();
        var incidentId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationId, 6001, jurisdictionId, "Intersection"));
        db.IncidentReadModels.Add(CreateIncident(incidentId, 1001, jurisdictionId, null, "INC-1001", "Traffic stop"));
        db.CitationReadModels.Add(CreateCitation(citationId, 4001, jurisdictionId, "Speeding", "CT-4001", locationId));
        db.IncidentCitationLinkReadModels.Add(IncidentCitationLinkReadModel.Create(Guid.NewGuid(), jurisdictionId, incidentId, 1001, "INC-1001", citationId, DateTime.UtcNow));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRecordRelationshipsHandler(db, new FakeTenantProvider(jurisdictionId));

        var result = await handler.Handle(
            new GetRecordRelationshipsQuery(RecordRelationshipRecordTypes.Citation, 4001),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("CT-4001", result.Source.Title);
        Assert.Contains(result.Groups, g => g.Title == "Linked Incidents" && g.Items.Single().NavigationUrl == "/incidents/1001");
        Assert.Contains(result.Groups, g => g.Title == "Location" && g.Items.Single().NavigationUrl == "/locations/6001");
    }

    [Fact]
    public async Task UnsupportedRecordType_Throws_ArgumentOutOfRangeException()
    {
        await using var db = RelationshipTestDbContext.Create();
        var jurisdictionId = Guid.NewGuid();

        var handler = new GetRecordRelationshipsHandler(db, new FakeTenantProvider(jurisdictionId));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            handler.Handle(
                new GetRecordRelationshipsQuery("UnknownType", 1001),
                CancellationToken.None));
    }

    [Fact]
    public async Task NameQuery_Returns_Arrests_And_Locations()
    {
        await using var db = RelationshipTestDbContext.Create();
        var jurisdictionId = Guid.NewGuid();
        var nameId = Guid.NewGuid();
        var primaryLocationId = Guid.NewGuid();
        var secondaryLocationId = Guid.NewGuid();

        db.LocationReadModels.AddRange(
            CreateLocation(primaryLocationId, 7001, jurisdictionId, "Primary Address"),
            CreateLocation(secondaryLocationId, 7002, jurisdictionId, "Secondary Address"));

        db.NameReadModels.Add(CreateName(nameId, 3001, jurisdictionId, "Jordan", "Alex", primaryLocationId, secondaryLocationId));
        db.ArrestReadModels.Add(CreateArrest(Guid.NewGuid(), 2001, jurisdictionId, nameId, "AR-2001"));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRecordRelationshipsHandler(db, new FakeTenantProvider(jurisdictionId));

        var result = await handler.Handle(
            new GetRecordRelationshipsQuery(RecordRelationshipRecordTypes.Name, 3001),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains(result.Groups, g => g.Title == "Related Arrests" && g.Items.Single().NavigationUrl == "/arrests/2001");
        Assert.Contains(result.Groups, g => g.Title == "Primary Location" && g.Items.Single().NavigationUrl == "/locations/7001");
        Assert.Contains(result.Groups, g => g.Title == "Secondary Location" && g.Items.Single().NavigationUrl == "/locations/7002");
    }

    [Fact]
    public async Task LocationQuery_Excludes_Relationships_From_Other_Jurisdictions()
    {
        await using var db = RelationshipTestDbContext.Create();
        var tenantJurisdictionId = Guid.NewGuid();
        var otherJurisdictionId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        db.LocationReadModels.AddRange(
            CreateLocation(locationId, 8001, tenantJurisdictionId, "Shared Title"),
            CreateLocation(Guid.NewGuid(), 8001, otherJurisdictionId, "Other Location"));

        db.IncidentReadModels.AddRange(
            CreateIncident(Guid.NewGuid(), 1101, tenantJurisdictionId, locationId, "INC-1101", "Tenant incident"),
            CreateIncident(Guid.NewGuid(), 9901, otherJurisdictionId, locationId, "INC-9901", "Other incident"));

        db.NameReadModels.AddRange(
            CreateName(Guid.NewGuid(), 3101, tenantJurisdictionId, "Tenant", "Taylor", primaryLocationId: locationId),
            CreateName(Guid.NewGuid(), 3901, otherJurisdictionId, "Other", "Olivia", primaryLocationId: locationId));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRecordRelationshipsHandler(db, new FakeTenantProvider(tenantJurisdictionId));

        var result = await handler.Handle(
            new GetRecordRelationshipsQuery(RecordRelationshipRecordTypes.Location, 8001),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains(result.Groups, g => g.Title == "Related Incidents" && g.Items.Single().RecordNumber == 1101);
        Assert.Contains(result.Groups, g => g.Title == "Related Names" && g.Items.Single().RecordNumber == 3101);
        Assert.DoesNotContain(result.Groups.SelectMany(g => g.Items), item => item.RecordNumber == 9901 || item.RecordNumber == 3901);
    }

    private static IncidentReadModel CreateIncident(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        Guid? locationId,
        string incidentNum,
        string description)
    {
        var model = IncidentReadModel.Create(
            id,
            recordNumber,
            jurisdictionId,
            Guid.NewGuid(),
            new IncidentDetails
            {
                IncidentNum = incidentNum,
                LocalNum = string.Empty,
                Description = description,
                CFSNum = string.Empty
            },
            RecordStatus.Open,
            DateTime.UtcNow,
            Guid.NewGuid());

        if (locationId.HasValue)
        {
            model.ApplyLocationChanged(locationId.Value);
        }

        return model;
    }

    private static ArrestReadModel CreateArrest(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        Guid? nameId,
        string arrestNumber,
        Guid? primaryIncidentId = null,
        Guid? locationId = null)
    {
        var model = ArrestReadModel.Create(
            id,
            recordNumber,
            jurisdictionId,
            Guid.NewGuid(),
            nameId,
            DateTime.UtcNow,
            DateTime.UtcNow,
            Guid.NewGuid(),
            arrestNumber,
            primaryIncidentId);

        if (locationId.HasValue)
        {
            model.ApplyLocationChanged(locationId.Value);
        }

        return model;
    }

    private static CitationReadModel CreateCitation(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        string description,
        string citationNumber,
        Guid? locationId = null)
    {
        var model = CitationReadModel.Create(
            id,
            recordNumber,
            jurisdictionId,
            Guid.NewGuid(),
            description,
            DateTime.UtcNow,
            DateTime.UtcNow,
            Guid.NewGuid(),
            citationNumber);

        if (locationId.HasValue)
        {
            model.ApplyLocationChanged(locationId.Value);
        }

        return model;
    }

    private static NameReadModel CreateName(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        string lastName,
        string firstName,
        Guid? primaryLocationId = null,
        Guid? secondaryLocationId = null)
    {
        var model = NameReadModel.Create(
            id,
            recordNumber,
            jurisdictionId,
            Guid.NewGuid(),
            NameTypes.Person,
            lastName,
            firstName,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            DateTime.UtcNow,
            Guid.NewGuid());

        model.ApplyLocationChanged(primaryLocationId, secondaryLocationId);
        return model;
    }

    private static LocationReadModel CreateLocation(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        string commonPlaceName)
    {
        return LocationReadModel.Create(
            id,
            recordNumber,
            jurisdictionId,
            "123",
            null,
            "Main",
            null,
            null,
            "Metro",
            null,
            null,
            "12345",
            null,
            null,
            commonPlaceName,
            null,
            $"123 Main, Metro",
            DateTime.UtcNow,
            Guid.NewGuid());
    }

    private sealed class RelationshipTestDbContext(DbContextOptions<RelationshipTestDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<Incident> Incidents => throw new NotImplementedException();
        public IQueryable<Incident> AllIncidentsWithDeleted => throw new NotImplementedException();
        public DbSet<Arrest> Arrests => throw new NotImplementedException();
        public DbSet<Citation> Citations => throw new NotImplementedException();
        public DbSet<Name> Names => throw new NotImplementedException();
        public DbSet<Location> Locations => throw new NotImplementedException();
        public DbSet<Mugshot> Mugshots => throw new NotImplementedException();
        public DbSet<MugshotLink> MugshotLinks => throw new NotImplementedException();
        public DbSet<IncidentArrestLink> IncidentArrestLinks => throw new NotImplementedException();
        public DbSet<IncidentCitationLink> IncidentCitationLinks => throw new NotImplementedException();
        public DbSet<IncidentReadModel> IncidentReadModels { get; set; } = null!;
        public DbSet<ArrestReadModel> ArrestReadModels { get; set; } = null!;
        public DbSet<CitationReadModel> CitationReadModels { get; set; } = null!;
        public DbSet<NameReadModel> NameReadModels { get; set; } = null!;
        public DbSet<LocationReadModel> LocationReadModels { get; set; } = null!;
        public DbSet<MugshotReadModel> MugshotReadModels => throw new NotImplementedException();
        public DbSet<MugshotLinkReadModel> MugshotLinkReadModels => throw new NotImplementedException();
        public DbSet<IncidentArrestLinkReadModel> IncidentArrestLinkReadModels { get; set; } = null!;
        public DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels { get; set; } = null!;
        public DbSet<AuditLogReadModel> AuditLogReadModels => throw new NotImplementedException();
        public DbSet<AgencyConfiguration> AgencyConfigurations => throw new NotImplementedException();
        public DbSet<AgencySequenceCounter> AgencySequenceCounters => throw new NotImplementedException();
        public DbSet<PicklistItem> PicklistItems => throw new NotImplementedException();
        public DbSet<PicklistSetting> PicklistSettings => throw new NotImplementedException();

        public void Detach<TEntity>(TEntity entity) where TEntity : class =>
            Entry(entity).State = EntityState.Detached;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IncidentReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<ArrestReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<CitationReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<NameReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<LocationReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<IncidentArrestLinkReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<IncidentCitationLinkReadModel>().HasKey(x => x.Id);
        }

        public static RelationshipTestDbContext Create()
        {
            var options = new DbContextOptionsBuilder<RelationshipTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RelationshipTestDbContext(options);
        }
    }

    private sealed class FakeTenantProvider(Guid jurisdictionId) : ITenantProvider
    {
        public Guid GetJurisdictionId() => jurisdictionId;
        public Guid GetAgencyId() => Guid.NewGuid();
        public Guid GetUserId() => Guid.NewGuid();
        public void SetJurisdictionId(Guid jurisdictionId) => throw new NotSupportedException();
    }
}
