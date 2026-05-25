using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Queries.GetMapMarkers;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Common.Queries.GetMapMarkers;

public sealed class GetMapMarkersHandlerTests
{
    [Fact]
    public async Task Handle_ExcludesIncidents_FromOtherAgencies()
    {
        await using var db = MapMarkersTestDbContext.Create();
        var jurisdictionId  = Guid.NewGuid();
        var agencyId        = Guid.NewGuid();
        var otherAgencyId   = Guid.NewGuid();
        var locationId      = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationId, jurisdictionId, "39.0,-95.0"));
        db.IncidentReadModels.Add(CreateIncident(Guid.NewGuid(), 1001, jurisdictionId, agencyId,    locationId));
        db.IncidentReadModels.Add(CreateIncident(Guid.NewGuid(), 9001, jurisdictionId, otherAgencyId, locationId));
        await db.SaveChangesAsync();

        var handler = new GetMapMarkersHandler(db, new FakeTenantProvider(jurisdictionId, agencyId));

        var result = await handler.Handle(new GetMapMarkersQuery(jurisdictionId, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(1001, result[0].RecordNumber);
        Assert.Equal("Incident", result[0].RecordType);
    }

    [Fact]
    public async Task Handle_ExcludesArrests_FromOtherAgencies()
    {
        await using var db = MapMarkersTestDbContext.Create();
        var jurisdictionId  = Guid.NewGuid();
        var agencyId        = Guid.NewGuid();
        var otherAgencyId   = Guid.NewGuid();
        var locationId      = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationId, jurisdictionId, "39.0,-95.0"));
        db.ArrestReadModels.Add(CreateArrest(Guid.NewGuid(), 2001, jurisdictionId, agencyId,    locationId));
        db.ArrestReadModels.Add(CreateArrest(Guid.NewGuid(), 9002, jurisdictionId, otherAgencyId, locationId));
        await db.SaveChangesAsync();

        var handler = new GetMapMarkersHandler(db, new FakeTenantProvider(jurisdictionId, agencyId));

        var result = await handler.Handle(new GetMapMarkersQuery(jurisdictionId, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(2001, result[0].RecordNumber);
        Assert.Equal("Arrest", result[0].RecordType);
    }

    [Fact]
    public async Task Handle_ExcludesCitations_FromOtherAgencies()
    {
        await using var db = MapMarkersTestDbContext.Create();
        var jurisdictionId  = Guid.NewGuid();
        var agencyId        = Guid.NewGuid();
        var otherAgencyId   = Guid.NewGuid();
        var locationId      = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationId, jurisdictionId, "39.0,-95.0"));
        db.CitationReadModels.Add(CreateCitation(Guid.NewGuid(), 4001, jurisdictionId, agencyId,    locationId));
        db.CitationReadModels.Add(CreateCitation(Guid.NewGuid(), 9004, jurisdictionId, otherAgencyId, locationId));
        await db.SaveChangesAsync();

        var handler = new GetMapMarkersHandler(db, new FakeTenantProvider(jurisdictionId, agencyId));

        var result = await handler.Handle(new GetMapMarkersQuery(jurisdictionId, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(4001, result[0].RecordNumber);
        Assert.Equal("Citation", result[0].RecordType);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenAgencyIdIsEmpty()
    {
        await using var db = MapMarkersTestDbContext.Create();
        var jurisdictionId = Guid.NewGuid();
        var locationId     = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationId, jurisdictionId, "39.0,-95.0"));
        db.IncidentReadModels.Add(CreateIncident(Guid.NewGuid(), 1001, jurisdictionId, Guid.NewGuid(), locationId));
        await db.SaveChangesAsync();

        var handler = new GetMapMarkersHandler(db, new FakeTenantProvider(jurisdictionId, Guid.Empty));

        var result = await handler.Handle(new GetMapMarkersQuery(jurisdictionId, null), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ExcludesRecordsWithoutParseableCoordinates()
    {
        await using var db = MapMarkersTestDbContext.Create();
        var jurisdictionId = Guid.NewGuid();
        var agencyId       = Guid.NewGuid();
        var locationWithCoords    = Guid.NewGuid();
        var locationWithoutCoords = Guid.NewGuid();

        db.LocationReadModels.Add(CreateLocation(locationWithCoords,    jurisdictionId, "39.0,-95.0"));
        db.LocationReadModels.Add(CreateLocation(locationWithoutCoords, jurisdictionId, null));
        db.IncidentReadModels.Add(CreateIncident(Guid.NewGuid(), 1001, jurisdictionId, agencyId, locationWithCoords));
        db.IncidentReadModels.Add(CreateIncident(Guid.NewGuid(), 1002, jurisdictionId, agencyId, locationWithoutCoords));
        await db.SaveChangesAsync();

        var handler = new GetMapMarkersHandler(db, new FakeTenantProvider(jurisdictionId, agencyId));

        var result = await handler.Handle(new GetMapMarkersQuery(jurisdictionId, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(1001, result[0].RecordNumber);
    }

    private static LocationReadModel CreateLocation(Guid id, Guid jurisdictionId, string? coordinates) =>
        LocationReadModel.Create(
            id, 6000, jurisdictionId,
            "100", null, "Main St", null, null, "Springfield",
            null, null, "12345", null,
            coordinates,
            null, null, "100 Main St, Springfield",
            DateTime.UtcNow, Guid.NewGuid());

    private static IncidentReadModel CreateIncident(
        Guid id, long recordNumber, Guid jurisdictionId, Guid agencyId, Guid locationId)
    {
        var model = IncidentReadModel.Create(
            id, recordNumber, jurisdictionId, agencyId,
            new IncidentDetails { IncidentNum = $"INC-{recordNumber}", LocalNum = string.Empty, Description = "Test", CFSNum = string.Empty },
            RecordStatus.Open,
            DateTime.UtcNow,
            Guid.NewGuid());
        model.ApplyLocationChanged(locationId);
        return model;
    }

    private static ArrestReadModel CreateArrest(
        Guid id, long recordNumber, Guid jurisdictionId, Guid agencyId, Guid locationId)
    {
        var model = ArrestReadModel.Create(
            id, recordNumber, jurisdictionId, agencyId,
            null, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(),
            $"AR-{recordNumber}");
        model.ApplyLocationChanged(locationId);
        return model;
    }

    private static CitationReadModel CreateCitation(
        Guid id, long recordNumber, Guid jurisdictionId, Guid agencyId, Guid locationId)
    {
        var model = CitationReadModel.Create(
            id, recordNumber, jurisdictionId, agencyId,
            "Test violation", DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(),
            $"CT-{recordNumber}");
        model.ApplyLocationChanged(locationId);
        return model;
    }

    private sealed class FakeTenantProvider(Guid jurisdictionId, Guid agencyId) : ITenantProvider
    {
        public Guid GetJurisdictionId() => jurisdictionId;
        public Guid GetAgencyId()       => agencyId;
        public Guid GetUserId()         => Guid.NewGuid();
        public void SetJurisdictionId(Guid jurisdictionId) => throw new NotSupportedException();
    }

    private sealed class MapMarkersTestDbContext(DbContextOptions<MapMarkersTestDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<IncidentReadModel>  IncidentReadModels  { get; set; } = null!;
        public DbSet<ArrestReadModel>    ArrestReadModels    { get; set; } = null!;
        public DbSet<CitationReadModel>  CitationReadModels  { get; set; } = null!;
        public DbSet<LocationReadModel>  LocationReadModels  { get; set; } = null!;

        // Unused stubs
        public DbSet<Incident>                      Incidents                      => throw new NotImplementedException();
        public IQueryable<Incident>                 AllIncidentsWithDeleted        => throw new NotImplementedException();
        public DbSet<Arrest>                        Arrests                        => throw new NotImplementedException();
        public DbSet<ArrestNameSnapshot>            ArrestNameSnapshots            => throw new NotImplementedException();
        public DbSet<Citation>                      Citations                      => throw new NotImplementedException();
        public DbSet<CitationNameSnapshot>          CitationNameSnapshots          => throw new NotImplementedException();
        public DbSet<CitationOfficerProfile>        CitationOfficerProfiles        => throw new NotImplementedException();
        public DbSet<CitationTexasDetails>          CitationTexasDetails           => throw new NotImplementedException();
        public DbSet<CitationOffenseDetails>        CitationOffenseDetails         => throw new NotImplementedException();
        public DbSet<CitationViolationFlag>         CitationViolationFlags         => throw new NotImplementedException();
        public DbSet<CitationVehicle>               CitationVehicles               => throw new NotImplementedException();
        public DbSet<Charge>                        Charges                        => throw new NotImplementedException();
        public DbSet<Name>                          Names                          => throw new NotImplementedException();
        public DbSet<Location>                      Locations                      => throw new NotImplementedException();
        public DbSet<Mugshot>                       Mugshots                       => throw new NotImplementedException();
        public DbSet<MugshotLink>                   MugshotLinks                   => throw new NotImplementedException();
    public DbSet<Narrative> Narratives => throw new NotImplementedException();
    public DbSet<NarrativeLink> NarrativeLinks => throw new NotImplementedException();
    public DbSet<NarrativeReadModel> NarrativeReadModels => throw new NotImplementedException();
    public DbSet<NarrativeLinkReadModel> NarrativeLinkReadModels => throw new NotImplementedException();
        public DbSet<IncidentArrestLink>            IncidentArrestLinks            => throw new NotImplementedException();
        public DbSet<IncidentCitationLink>          IncidentCitationLinks          => throw new NotImplementedException();
        public DbSet<ArrestChargeLink>              ArrestChargeLinks              => throw new NotImplementedException();
        public DbSet<CitationChargeLink>            CitationChargeLinks            => throw new NotImplementedException();
        public DbSet<IncidentChargeLink>            IncidentChargeLinks            => throw new NotImplementedException();
        public DbSet<NameReadModel>                 NameReadModels                 => throw new NotImplementedException();
        public DbSet<MugshotReadModel>              MugshotReadModels              => throw new NotImplementedException();
        public DbSet<MugshotLinkReadModel>          MugshotLinkReadModels          => throw new NotImplementedException();
        public DbSet<IncidentArrestLinkReadModel>   IncidentArrestLinkReadModels   => throw new NotImplementedException();
        public DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels => throw new NotImplementedException();
        public DbSet<AuditLogReadModel>             AuditLogReadModels             => throw new NotImplementedException();
        public DbSet<AgencyConfiguration>           AgencyConfigurations           => throw new NotImplementedException();
        public DbSet<AgencySequenceCounter>         AgencySequenceCounters         => throw new NotImplementedException();
        public DbSet<PicklistItem>                  PicklistItems                  => throw new NotImplementedException();
        public DbSet<PicklistSetting>               PicklistSettings               => throw new NotImplementedException();

        public void Detach<TEntity>(TEntity entity) where TEntity : class =>
            Entry(entity).State = EntityState.Detached;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IncidentReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<ArrestReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<CitationReadModel>().HasKey(x => x.Id);
            modelBuilder.Entity<LocationReadModel>().HasKey(x => x.Id);
        }

        public static MapMarkersTestDbContext Create()
        {
            var options = new DbContextOptionsBuilder<MapMarkersTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new MapMarkersTestDbContext(options);
        }
    }
}
