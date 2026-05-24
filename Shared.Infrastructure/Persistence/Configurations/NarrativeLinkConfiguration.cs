using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class NarrativeLinkConfiguration : IEntityTypeConfiguration<NarrativeLink>
{
    public void Configure(EntityTypeBuilder<NarrativeLink> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerType).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.OwnerType, x.OwnerId });
        builder.HasIndex(x => new { x.NarrativeId, x.OwnerType, x.OwnerId }).IsUnique();
    }
}
