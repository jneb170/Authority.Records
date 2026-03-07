using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class PicklistItemConfiguration : IEntityTypeConfiguration<PicklistItem>
{
    public void Configure(EntityTypeBuilder<PicklistItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PicklistType)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Value)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Label)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.IsSystemDefault).IsRequired();

        // Unique per agency + type + value (prevents duplicate system keys per agency)
        builder.HasIndex(x => new { x.JurisdictionId, x.AgencyId, x.PicklistType, x.Value })
               .IsUnique();
    }
}
