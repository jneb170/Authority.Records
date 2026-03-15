using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class MugshotConfiguration : IEntityTypeConfiguration<Mugshot>
{
    public void Configure(EntityTypeBuilder<Mugshot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.PublicUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => new { x.JurisdictionId, x.CapturedAtUtc });
    }
}
