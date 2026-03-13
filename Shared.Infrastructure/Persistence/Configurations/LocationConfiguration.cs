using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordNumber)
               .ValueGeneratedOnAdd()
               .UseIdentityColumn(seed: 20000, increment: 1);

        builder.HasIndex(x => x.RecordNumber).IsUnique();

        builder.Property(x => x.StreetAddress).HasMaxLength(200).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();

        builder.Property(x => x.StreetNumber).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.PreDirectionId).IsRequired(false);
        builder.Property(x => x.StreetTypeId).IsRequired(false);
        builder.Property(x => x.PostDirectionId).IsRequired(false);
        builder.Property(x => x.StateId).IsRequired(false);
        builder.Property(x => x.CountryId).IsRequired(false);
        builder.Property(x => x.Zip).HasMaxLength(10).IsRequired(false);
        builder.Property(x => x.AptSuite).HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.Coordinates).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.CommonPlaceName).HasMaxLength(250).IsRequired(false);
        builder.Property(x => x.Comments).HasMaxLength(500).IsRequired(false);

        // Indexes for common search patterns — all keyed on JurisdictionId for multi-tenant isolation
        builder.HasIndex(x => new { x.JurisdictionId, x.City });
        builder.HasIndex(x => new { x.JurisdictionId, x.StreetAddress });
        builder.HasIndex(x => new { x.JurisdictionId, x.CommonPlaceName });
        builder.HasIndex(x => new { x.JurisdictionId, x.Zip });
        builder.HasIndex(x => new { x.JurisdictionId, x.StateId });
    }
}
