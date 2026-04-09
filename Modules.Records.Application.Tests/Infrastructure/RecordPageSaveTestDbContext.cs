using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Tests.Infrastructure;

internal sealed class RecordPageSaveTestDbContext(DbContextOptions<RecordPageSaveTestDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public int SaveChangesCallCount { get; private set; }

    public DbSet<Incident> Incidents { get; set; } = null!;
    public IQueryable<Incident> AllIncidentsWithDeleted => Incidents;
    public DbSet<Arrest> Arrests { get; set; } = null!;
    public DbSet<ArrestNameSnapshot> ArrestNameSnapshots { get; set; } = null!;
    public DbSet<Citation> Citations { get; set; } = null!;
    public DbSet<CitationNameSnapshot> CitationNameSnapshots { get; set; } = null!;
    public DbSet<CitationOfficerProfile> CitationOfficerProfiles { get; set; } = null!;
    public DbSet<CitationTexasDetails> CitationTexasDetails { get; set; } = null!;
    public DbSet<CitationVehicle> CitationVehicles { get; set; } = null!;
    public DbSet<Charge> Charges { get; set; } = null!;
    public DbSet<Name> Names { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Mugshot> Mugshots => throw new NotImplementedException();
    public DbSet<MugshotLink> MugshotLinks => throw new NotImplementedException();
    public DbSet<IncidentArrestLink> IncidentArrestLinks { get; set; } = null!;
    public DbSet<IncidentCitationLink> IncidentCitationLinks { get; set; } = null!;
    public DbSet<ArrestChargeLink> ArrestChargeLinks { get; set; } = null!;
    public DbSet<CitationChargeLink> CitationChargeLinks { get; set; } = null!;
    public DbSet<IncidentChargeLink> IncidentChargeLinks { get; set; } = null!;

    public DbSet<IncidentReadModel> IncidentReadModels => throw new NotImplementedException();
    public DbSet<ArrestReadModel> ArrestReadModels => throw new NotImplementedException();
    public DbSet<CitationReadModel> CitationReadModels => throw new NotImplementedException();
    public DbSet<NameReadModel> NameReadModels => throw new NotImplementedException();
    public DbSet<LocationReadModel> LocationReadModels => throw new NotImplementedException();
    public DbSet<MugshotReadModel> MugshotReadModels => throw new NotImplementedException();
    public DbSet<MugshotLinkReadModel> MugshotLinkReadModels => throw new NotImplementedException();
    public DbSet<IncidentArrestLinkReadModel> IncidentArrestLinkReadModels => throw new NotImplementedException();
    public DbSet<IncidentCitationLinkReadModel> IncidentCitationLinkReadModels => throw new NotImplementedException();
    public DbSet<AuditLogReadModel> AuditLogReadModels => throw new NotImplementedException();
    public DbSet<AgencyConfiguration> AgencyConfigurations => throw new NotImplementedException();
    public DbSet<AgencySequenceCounter> AgencySequenceCounters => throw new NotImplementedException();
    public DbSet<PicklistItem> PicklistItems { get; set; } = null!;
    public DbSet<PicklistSetting> PicklistSettings => throw new NotImplementedException();

    public void Detach<TEntity>(TEntity entity) where TEntity : class
        => Entry(entity).State = EntityState.Detached;

    public void ResetSaveChangesCallCount() => SaveChangesCallCount = 0;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEntity<Incident>(modelBuilder);
        ConfigureEntity<Arrest>(modelBuilder);
        ConfigureEntity<ArrestNameSnapshot>(modelBuilder);
        ConfigureEntity<Citation>(modelBuilder);
        ConfigureEntity<CitationNameSnapshot>(modelBuilder);
        ConfigureEntity<CitationOfficerProfile>(modelBuilder);
        ConfigureEntity<CitationTexasDetails>(modelBuilder);
        ConfigureEntity<CitationVehicle>(modelBuilder);
        ConfigureEntity<Charge>(modelBuilder);
        ConfigureEntity<Name>(modelBuilder);
        ConfigureEntity<Location>(modelBuilder);
        ConfigureEntity<IncidentArrestLink>(modelBuilder);
        ConfigureEntity<IncidentCitationLink>(modelBuilder);
        ConfigureEntity<IncidentChargeLink>(modelBuilder);
        ConfigureEntity<ArrestChargeLink>(modelBuilder);
        ConfigureEntity<CitationChargeLink>(modelBuilder);
        ConfigureEntity<PicklistItem>(modelBuilder);
    }

    private static void ConfigureEntity<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>(builder =>
        {
            builder.HasKey("Id");

            if (typeof(TEntity).GetProperty(nameof(AggregateRoot.DomainEvents)) is not null)
            {
                builder.Ignore(nameof(AggregateRoot.DomainEvents));
            }
        });
    }
}

internal static class RecordPageSaveTestDbContextFactory
{
    public static RecordPageSaveTestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<RecordPageSaveTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RecordPageSaveTestDbContext(options);
    }
}

internal sealed class TestTenantProvider(Guid jurisdictionId, Guid agencyId, Guid userId) : ITenantProvider
{
    private Guid _jurisdictionId = jurisdictionId;

    public Guid GetJurisdictionId() => _jurisdictionId;

    public Guid GetAgencyId() => agencyId;

    public Guid GetUserId() => userId;

    public void SetJurisdictionId(Guid jurisdictionId) => _jurisdictionId = jurisdictionId;
}
