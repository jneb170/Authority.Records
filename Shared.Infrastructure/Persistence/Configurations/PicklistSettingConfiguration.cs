using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class PicklistSettingConfiguration : IEntityTypeConfiguration<PicklistSetting>
{
    public void Configure(EntityTypeBuilder<PicklistSetting> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PicklistType)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.IsRequired).IsRequired();

        // One setting row per agency per type
        builder.HasIndex(x => new { x.JurisdictionId, x.AgencyId, x.PicklistType })
               .IsUnique();
    }
}
