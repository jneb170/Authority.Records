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

        builder.Property(x => x.ViolationSection)
            .HasMaxLength(50);

        builder.Property(x => x.PrimaryViolationDescription)
            .HasMaxLength(250);

        builder.Property(x => x.NarrativeOtherViolations)
            .HasMaxLength(1000);

        builder.Property(x => x.OccurredAtText)
            .HasMaxLength(250);

        builder.Property(x => x.ComplainantSignatureText)
            .HasMaxLength(150);

        builder.Property(x => x.DefendantSignatureText)
            .HasMaxLength(150);

        builder.Property(x => x.AcceptedBondNotes)
            .HasMaxLength(500);

        builder.Property(x => x.ReceiptNumber)
            .HasMaxLength(50);

        builder.HasOne<Citation>()
            .WithOne()
            .HasForeignKey<CitationTexasDetails>(x => x.CitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
