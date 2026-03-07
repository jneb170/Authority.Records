using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class AgencyConfigurationEntityConfiguration : IEntityTypeConfiguration<AgencyConfiguration>
{
    public void Configure(EntityTypeBuilder<AgencyConfiguration> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // One configuration entry per key per agency per jurisdiction
        builder.HasIndex(x => new { x.JurisdictionId, x.AgencyId, x.Key })
            .IsUnique();
    }
}
