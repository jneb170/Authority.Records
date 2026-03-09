using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AgencyEntity = Shared.Infrastructure.Identity.Agency;
using JurisdictionEntity = Shared.Infrastructure.Identity.Jurisdiction;
using UserAgencyEntity = Shared.Infrastructure.Identity.ApplicationUserAgency;

namespace Shared.Infrastructure.Persistence;

public class AuthDbContext : IdentityDbContext<Identity.ApplicationUser, IdentityRole, string>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public DbSet<JurisdictionEntity> Jurisdictions => Set<JurisdictionEntity>();
    public DbSet<AgencyEntity> Agencies => Set<AgencyEntity>();
    public DbSet<UserAgencyEntity> UserAgencies => Set<UserAgencyEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("auth");

        builder.Entity<JurisdictionEntity>(e =>
        {
            e.ToTable("Jurisdictions");
            e.HasKey(j => j.Id);
            e.Property(j => j.Name).IsRequired().HasMaxLength(200);
            e.Property(j => j.State).HasMaxLength(100);
            e.Property(j => j.Code).HasMaxLength(50);
        });

        builder.Entity<AgencyEntity>(e =>
        {
            e.ToTable("Agencies");
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).IsRequired().HasMaxLength(200);
            e.Property(a => a.Code).HasMaxLength(50);
        });

        builder.Entity<UserAgencyEntity>(e =>
        {
            e.ToTable("UserAgencies");
            e.HasKey(ua => new { ua.UserId, ua.AgencyId });
        });
    }
}
