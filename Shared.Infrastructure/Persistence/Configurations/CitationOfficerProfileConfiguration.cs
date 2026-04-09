using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class CitationOfficerProfileConfiguration : IEntityTypeConfiguration<CitationOfficerProfile>
{
    public void Configure(EntityTypeBuilder<CitationOfficerProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CitationId)
            .IsUnique();

        builder.Property(x => x.OfficerName)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(100);

        builder.Property(x => x.BadgeOrIdentifier)
            .HasMaxLength(50);

        builder.Property(x => x.UnitNumber)
            .HasMaxLength(50);

        builder.HasOne<Citation>()
            .WithOne()
            .HasForeignKey<CitationOfficerProfile>(x => x.CitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
