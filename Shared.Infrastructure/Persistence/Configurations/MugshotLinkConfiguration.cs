using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class MugshotLinkConfiguration : IEntityTypeConfiguration<MugshotLink>
{
    public void Configure(EntityTypeBuilder<MugshotLink> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerType)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => new { x.OwnerType, x.OwnerId });
        builder.HasIndex(x => new { x.MugshotId, x.OwnerType, x.OwnerId }).IsUnique();
    }
}
