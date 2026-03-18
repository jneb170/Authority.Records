using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OffenseName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.UcrCategory)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.NibrsGroup)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.CrimeAgainst)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.UcrCode)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.ChargeLevel)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.StateClass)
               .HasMaxLength(50)
               .IsRequired(false);

        builder.HasIndex(x => x.IsActive);

        builder.HasIndex(x => new { x.JurisdictionId, x.AgencyId, x.UcrCode, x.OffenseName })
               .IsUnique();
    }
}
