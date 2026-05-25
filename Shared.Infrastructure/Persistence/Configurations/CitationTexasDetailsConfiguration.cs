using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class CitationTexasDetailsConfiguration : IEntityTypeConfiguration<CitationTexasDetails>
{
    public void Configure(EntityTypeBuilder<CitationTexasDetails> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CitationId)
            .IsUnique();

        builder.Property(x => x.DocketNumber)
            .HasMaxLength(50);

        builder.Property(x => x.PageNumber)
            .HasMaxLength(25);

        builder.HasOne<Citation>()
            .WithOne()
            .HasForeignKey<CitationTexasDetails>(x => x.CitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
