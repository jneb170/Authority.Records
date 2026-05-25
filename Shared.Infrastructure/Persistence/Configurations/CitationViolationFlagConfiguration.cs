using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class CitationViolationFlagConfiguration : IEntityTypeConfiguration<CitationViolationFlag>
{
    public void Configure(EntityTypeBuilder<CitationViolationFlag> builder)
    {
        builder.HasKey(x => x.Id);

        // Stored by name so flags stay queryable with readable values and survive enum reordering.
        builder.Property(x => x.Key)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(16);

        // One row per (citation, flag) regardless of provenance.
        builder.HasIndex(x => new { x.CitationId, x.Key })
            .IsUnique();

        // Cross-citation flag queries (e.g. "all citations marked NoSignal").
        builder.HasIndex(x => x.Key);

        // Charge-level flag queries / future charge-derived visibility filtering.
        builder.HasIndex(x => x.SourceChargeLinkId);

        builder.HasOne<Citation>()
            .WithMany()
            .HasForeignKey(x => x.CitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
