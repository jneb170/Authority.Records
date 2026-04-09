using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Tests.Infrastructure;

/// <summary>
/// Minimal EF InMemory DbContext for application-layer tests.
/// Only implements the DbSets needed by Location-related handlers.
/// All other members throw <see cref="NotImplementedException"/>.
/// </summary>
internal sealed class TestAppDbContext : DbContext, IApplicationDbContext
{
    public TestAppDbContext(DbContextOptions<TestAppDbContext> options) : base(options) { }

    public DbSet<Location>          Locations          { get; set; } = null!;
    public DbSet<LocationReadModel> LocationReadModels { get; set; } = null!;

    // ---- unused members (stubs) ----
    public DbSet<Incident>                      Incidents                      => throw new NotImplementedException();
    public IQueryable<Incident>                 AllIncidentsWithDeleted        => throw new NotImplementedException();
    public DbSet<Arrest>                        Arrests                        => throw new NotImplementedException();
    public DbSet<ArrestNameSnapshot>            ArrestNameSnapshots            => throw new NotImplementedException();
    public DbSet<Citation>                      Citations                      => throw new NotImplementedException();
    public DbSet<CitationNameSnapshot>          CitationNameSnapshots          => throw new NotImplementedException();
    public DbSet<CitationOfficerProfile>        CitationOfficerProfiles        => throw new NotImplementedException();
    public DbSet<CitationTexasDetails>          CitationTexasDetails           => throw new NotImplementedException();
    public DbSet<CitationVehicle>               CitationVehicles               => throw new NotImplementedException();
    public DbSet<Charge>                        Charges                        => throw new NotImplementedException();
    public DbSet<Name>                          Names                          => throw new NotImplementedException();
    public DbSet<Mugshot>                       Mugshots                       => throw new NotImplementedException();
    public DbSet<MugshotLink>                   MugshotLinks                   => throw new NotImplementedException();
    public DbSet<IncidentArrestLink>            IncidentArrestLinks            => throw new NotImplementedException();
    public DbSet<IncidentCitationLink>          IncidentCitationLinks          => throw new NotImplementedException();
    public DbSet<ArrestChargeLink>              ArrestChargeLinks              => throw new NotImplementedException();
    public DbSet<CitationChargeLink>            CitationChargeLinks            => throw new NotImplementedException();
    public DbSet<IncidentChargeLink>            IncidentChargeLinks            => throw new NotImplementedException();
    public DbSet<IncidentReadModel>             IncidentReadModels             => throw new NotImplementedException();
    public DbSet<ArrestReadModel>               ArrestReadModels               => throw new NotImplementedException();
    public DbSet<CitationReadModel>             CitationReadModels             => throw new NotImplementedException();
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

    public void Detach<TEntity>(TEntity entity) where TEntity : class
        => Entry(entity!).State = EntityState.Detached;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.StreetAddress).IsRequired();
            b.Property(l => l.City).IsRequired();
            b.Property(l => l.RecordNumber).ValueGeneratedOnAdd();
            b.Ignore(l => l.DomainEvents);
        });

        modelBuilder.Entity<LocationReadModel>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.StreetAddress).IsRequired();
            b.Property(l => l.City).IsRequired();
        });
    }
}

/// <summary>Builds a fresh InMemory TestAppDbContext for each test.</summary>
internal static class TestDbContextFactory
{
    public static TestAppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAppDbContext(options);
    }
}
