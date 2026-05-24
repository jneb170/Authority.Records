using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Arrests.DomainEventHandlers;
using Modules.Records.Application.Citations.DomainEventHandlers;
using Modules.Records.Application.Incidents.DomainEventHandlers;
using Modules.Records.Application.Names.DomainEventHandlers;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.ReadModels;

public sealed class ProjectionUpdateHandlerTests
{
    [Fact]
    public async Task IncidentProjectionUpdate_UsesEventPayloadOnly()
    {
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var now = new DateTime(2026, 3, 10, 10, 30, 0, DateTimeKind.Utc);
        var modifiedBy = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var incidentId = Guid.NewGuid();

        db.IncidentReadModels.Add(IncidentReadModel.Create(
            incidentId,
            101,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new IncidentDetails { Description = "Original", IncidentNum = "INC-101", LocalNum = "LOC-101", CFSNum = "CFS-101" },
            RecordStatus.Open,
            now.AddDays(-1),
            Guid.NewGuid()));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new IncidentProjectionHandler(db);
        await handler.Handle(
            new IncidentDetailsUpdatedDomainEvent(
                incidentId,
                new IncidentDetails { Description = "Updated", IncidentNum = "INC-202", LocalNum = "LOC-202", CFSNum = "CFS-202" },
                now,
                locationId,
                modifiedBy)
            {
                OccurredOnUtc = now
            },
            CancellationToken.None);

        var readModel = await db.IncidentReadModels.SingleAsync(i => i.Id == incidentId);
        Assert.Equal("Updated", readModel.Description);
        Assert.Equal(locationId, readModel.LocationId);
        Assert.Equal(modifiedBy, readModel.ModifiedBy);
        Assert.Equal(now, readModel.UpdatedAtUtc);
    }

    [Fact]
    public async Task IncidentProjectionStatusChange_UpdatesReadModelStatus()
    {
        // Regression: lifecycle transitions raise LifecycleStatusChangedDomainEvent<Incident>.
        // The projection used to subscribe to never-raised Incident{Opened,Closed,Archived}DomainEvent,
        // so the read-model Status silently never updated and the UI Open button appeared to no-op.
        // The event is raised through the real aggregate so AddDomainEvent stamps AggregateId
        // (the base DomainEvent.AggregateId has an internal setter the test assembly can't reach).
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var now = new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

        var incident = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = Guid.NewGuid(),
            AgencyId = Guid.NewGuid(),
            Details = new IncidentDetails { Description = "Draft incident", IncidentNum = "INC-102", LocalNum = "LOC-102", CFSNum = "CFS-102" }
        });
        incident.Open(new UserModificationContext(Guid.NewGuid()), new DefaultLifecyclePolicy<Incident>(new DefaultClosePolicy<Incident>()));
        var evt = incident.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Incident>>().Single();

        db.IncidentReadModels.Add(IncidentReadModel.Create(
            incident.Id,
            102,
            incident.JurisdictionId,
            incident.AgencyId,
            new IncidentDetails { Description = "Draft incident", IncidentNum = "INC-102", LocalNum = "LOC-102", CFSNum = "CFS-102" },
            RecordStatus.Draft,
            now.AddDays(-1),
            Guid.NewGuid()));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new IncidentProjectionHandler(db);
        await handler.Handle(evt, CancellationToken.None);

        var readModel = await db.IncidentReadModels.SingleAsync(i => i.Id == incident.Id);
        Assert.Equal("Open", readModel.Status);
    }

    [Fact]
    public async Task ArrestProjectionStatusChange_UpdatesReadModelStatus()
    {
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var now = new DateTime(2026, 3, 10, 10, 15, 0, DateTimeKind.Utc);

        var arrest = new ArrestFactory().Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(-1),
            "AR-202",
            null);
        arrest.Open(new UserModificationContext(Guid.NewGuid()), new DefaultLifecyclePolicy<Arrest>(new DefaultClosePolicy<Arrest>()));
        var evt = arrest.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Arrest>>().Single();

        var readModel = ArrestReadModel.Create(
            arrest.Id,
            202,
            arrest.JurisdictionId,
            arrest.AgencyId,
            null,
            now.AddDays(-1),
            now.AddDays(-1),
            Guid.NewGuid(),
            "AR-202");
        readModel.ApplyStatusChange("Draft");
        db.ArrestReadModels.Add(readModel);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new ArrestProjectionHandler(db);
        await handler.Handle(evt, CancellationToken.None);

        var updated = await db.ArrestReadModels.SingleAsync(a => a.Id == arrest.Id);
        Assert.Equal("Open", updated.Status);
    }

    [Fact]
    public async Task ArrestProjectionUpdate_UsesEventPayloadOnly()
    {
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var now = new DateTime(2026, 3, 10, 11, 0, 0, DateTimeKind.Utc);
        var modifiedBy = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var arrestId = Guid.NewGuid();
        var nameId = Guid.NewGuid();
        var incidentId = Guid.NewGuid();

        db.ArrestReadModels.Add(ArrestReadModel.Create(
            arrestId,
            201,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            now.AddDays(-1),
            now.AddDays(-1),
            Guid.NewGuid(),
            "AR-201"));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new ArrestProjectionHandler(db);
        await handler.Handle(
            new ArrestDetailsUpdatedDomainEvent(arrestId, nameId, now, Guid.NewGuid(), "AR-202", incidentId, locationId, modifiedBy)
            {
                OccurredOnUtc = now
            },
            CancellationToken.None);

        var readModel = await db.ArrestReadModels.SingleAsync(a => a.Id == arrestId);
        Assert.Equal(nameId, readModel.NameId);
        Assert.Equal("AR-202", readModel.ArrestNum);
        Assert.Equal(incidentId, readModel.PrimaryIncidentId);
        Assert.Equal(locationId, readModel.LocationId);
        Assert.Equal(modifiedBy, readModel.ModifiedBy);
        Assert.Equal(now, readModel.UpdatedAtUtc);
    }

    [Fact]
    public async Task ArrestProjectionCreate_CopiesAggregateLocationId()
    {
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var arrestId = Guid.NewGuid();
        var nameId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);

        var arrest = new ArrestFactory().Create(
            jurisdictionId,
            agencyId,
            nameId,
            createdAt,
            "AR-900",
            null);

        typeof(Arrest).GetProperty(nameof(Arrest.Id))!.SetValue(arrest, arrestId);
        arrest.SetLocation(locationId, new UserModificationContext(Guid.NewGuid()));

        db.Arrests.Add(arrest);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new ArrestProjectionHandler(db);
        await handler.Handle(
            new ArrestCreatedDomainEvent(arrestId, jurisdictionId, nameId, createdAt, "AR-900", null)
            {
                OccurredOnUtc = createdAt
            },
            CancellationToken.None);

        var readModel = await db.ArrestReadModels.SingleAsync(a => a.Id == arrestId);
        Assert.Equal(locationId, readModel.LocationId);
    }

    [Fact]
    public async Task CitationProjectionUpdate_UsesEventPayloadOnly()
    {
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var now = new DateTime(2026, 3, 10, 11, 30, 0, DateTimeKind.Utc);
        var modifiedBy = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var citationId = Guid.NewGuid();

        db.CitationReadModels.Add(CitationReadModel.Create(
            citationId,
            301,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Original citation",
            now.AddDays(-1),
            now.AddDays(-1),
            Guid.NewGuid(),
            "CT-301"));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new CitationProjectionHandler(db);
        await handler.Handle(
            new CitationDetailsUpdatedDomainEvent(citationId, "Updated citation", now, Guid.NewGuid(), "CT-302", Guid.NewGuid(), locationId, modifiedBy)
            {
                OccurredOnUtc = now
            },
            CancellationToken.None);

        var readModel = await db.CitationReadModels.SingleAsync(c => c.Id == citationId);
        Assert.Equal("Updated citation", readModel.Description);
        Assert.Equal("CT-302", readModel.CitationNum);
        Assert.Equal(locationId, readModel.LocationId);
        Assert.Equal(modifiedBy, readModel.ModifiedBy);
        Assert.Equal(now, readModel.UpdatedAtUtc);
    }

    [Fact]
    public async Task CitationProjectionIssue_UsesIssuedEvent()
    {
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var now = new DateTime(2026, 3, 10, 11, 45, 0, DateTimeKind.Utc);
        var issuedBy = Guid.NewGuid();
        var citationId = Guid.NewGuid();

        db.CitationReadModels.Add(CitationReadModel.Create(
            citationId,
            302,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Original citation",
            now.AddDays(-1),
            now.AddDays(-1),
            Guid.NewGuid(),
            "CT-303"));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new CitationProjectionHandler(db);
        await handler.Handle(
            new CitationIssuedDomainEvent(citationId, issuedBy)
            {
                OccurredOnUtc = now
            },
            CancellationToken.None);

        var readModel = await db.CitationReadModels.SingleAsync(c => c.Id == citationId);
        Assert.True(readModel.IsIssued);
        Assert.Equal(issuedBy, readModel.ModifiedBy);
        Assert.Equal(now, readModel.UpdatedAtUtc);
    }

    [Fact]
    public async Task NameProjectionUpdate_UsesEventPayloadOnly()
    {
        await using var db = ProjectionUpdateTestDbContextFactory.Create();
        var now = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);
        var modifiedBy = Guid.NewGuid();
        var primaryLocationId = Guid.NewGuid();
        var secondaryLocationId = Guid.NewGuid();
        var nameId = Guid.NewGuid();

        db.NameReadModels.Add(NameReadModel.Create(
            id: nameId,
            recordNumber: 401,
            jurisdictionId: Guid.NewGuid(),
            agencyId: Guid.NewGuid(),
            nameType: NameTypes.Person,
            lastOrBusinessName: "Original",
            firstName: "Taylor",
            middleName: null,
            sexId: null,
            raceId: null,
            dateOfBirth: null,
            driversLicenseNumber: null,
            driversLicenseStateId: null,
            heightInches: null,
            weightLbs: null,
            hairColorId: null,
            eyeColorId: null,
            suffixId: null,
            placeOfBirth: null,
            fbiNumber: null,
            localNumber: null,
            socialSecurityNumber: null,
            isCitizen: false,
            deceasedDate: null,
            createdAtUtc: now.AddDays(-1),
            createdBy: Guid.NewGuid()));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new NameProjectionHandler(db);
        await handler.Handle(
            new NameDetailsUpdatedDomainEvent(
                nameId,
                NameTypes.Person,
                "Updated",
                "Morgan",
                null,
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateTime(1990, 1, 2),
                "DL-100",
                Guid.NewGuid(),
                72,
                190,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Springfield",
                "FBI-100",
                "LOCAL-100",
                "555-1000",
                "12",
                "555-2000",
                "34",
                "555-3000",
                "56",
                "111-22-3333",
                true,
                null,
                primaryLocationId,
                secondaryLocationId,
                modifiedBy)
            {
                OccurredOnUtc = now
            },
            CancellationToken.None);

        var readModel = await db.NameReadModels.SingleAsync(n => n.Id == nameId);
        Assert.Equal("Updated", readModel.LastOrBusinessName);
        Assert.Equal("Morgan", readModel.FirstName);
        Assert.Equal(primaryLocationId, readModel.PrimaryLocationId);
        Assert.Equal(secondaryLocationId, readModel.SecondaryLocationId);
        Assert.Equal(modifiedBy, readModel.ModifiedBy);
        Assert.Equal(now, readModel.UpdatedAtUtc);
    }
}

internal sealed class ProjectionUpdateTestDbContext(DbContextOptions<ProjectionUpdateTestDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<IncidentReadModel> IncidentReadModels { get; set; } = null!;
    public DbSet<ArrestReadModel> ArrestReadModels { get; set; } = null!;
    public DbSet<CitationReadModel> CitationReadModels { get; set; } = null!;
    public DbSet<NameReadModel> NameReadModels { get; set; } = null!;
    public DbSet<Arrest> Arrests { get; set; } = null!;

    public DbSet<Incident> Incidents => throw new NotImplementedException();
    public IQueryable<Incident> AllIncidentsWithDeleted => throw new NotImplementedException();
    public DbSet<ArrestNameSnapshot> ArrestNameSnapshots => throw new NotImplementedException();
    public DbSet<Citation> Citations => throw new NotImplementedException();
    public DbSet<CitationNameSnapshot> CitationNameSnapshots => throw new NotImplementedException();
    public DbSet<CitationOfficerProfile> CitationOfficerProfiles => throw new NotImplementedException();
    public DbSet<CitationTexasDetails> CitationTexasDetails => throw new NotImplementedException();
    public DbSet<CitationVehicle> CitationVehicles => throw new NotImplementedException();
    public DbSet<Charge> Charges => throw new NotImplementedException();
    public DbSet<Name> Names => throw new NotImplementedException();
    public DbSet<Location> Locations => throw new NotImplementedException();
    public DbSet<Mugshot> Mugshots => throw new NotImplementedException();
    public DbSet<MugshotLink> MugshotLinks => throw new NotImplementedException();
    public DbSet<Narrative> Narratives => throw new NotImplementedException();
    public DbSet<NarrativeLink> NarrativeLinks => throw new NotImplementedException();
    public DbSet<NarrativeReadModel> NarrativeReadModels => throw new NotImplementedException();
    public DbSet<NarrativeLinkReadModel> NarrativeLinkReadModels => throw new NotImplementedException();
    public DbSet<IncidentArrestLink> IncidentArrestLinks => throw new NotImplementedException();
    public DbSet<IncidentCitationLink> IncidentCitationLinks => throw new NotImplementedException();
    public DbSet<ArrestChargeLink> ArrestChargeLinks => throw new NotImplementedException();
    public DbSet<CitationChargeLink> CitationChargeLinks => throw new NotImplementedException();
    public DbSet<IncidentChargeLink> IncidentChargeLinks => throw new NotImplementedException();
    public DbSet<LocationReadModel> LocationReadModels => throw new NotImplementedException();
    public DbSet<MugshotReadModel> MugshotReadModels => throw new NotImplementedException();
    public DbSet<MugshotLinkReadModel> MugshotLinkReadModels => throw new NotImplementedException();
    public DbSet<IncidentArrestLinkReadModel> IncidentArrestLinkReadModels => throw new NotImplementedException();
    public DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels => throw new NotImplementedException();
    public DbSet<AuditLogReadModel> AuditLogReadModels => throw new NotImplementedException();
    public DbSet<AgencyConfiguration> AgencyConfigurations => throw new NotImplementedException();
    public DbSet<AgencySequenceCounter> AgencySequenceCounters => throw new NotImplementedException();
    public DbSet<PicklistItem> PicklistItems => throw new NotImplementedException();
    public DbSet<PicklistSetting> PicklistSettings => throw new NotImplementedException();

    public void Detach<TEntity>(TEntity entity) where TEntity : class
        => Entry(entity).State = EntityState.Detached;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncidentReadModel>().HasKey(x => x.Id);
        modelBuilder.Entity<ArrestReadModel>().HasKey(x => x.Id);
        modelBuilder.Entity<CitationReadModel>().HasKey(x => x.Id);
        modelBuilder.Entity<NameReadModel>().HasKey(x => x.Id);
        modelBuilder.Entity<Arrest>(b =>
        {
            b.HasKey(x => x.Id);
            b.Ignore(x => x.DomainEvents);
        });
    }
}

internal static class ProjectionUpdateTestDbContextFactory
{
    public static ProjectionUpdateTestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ProjectionUpdateTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProjectionUpdateTestDbContext(options);
    }
}
