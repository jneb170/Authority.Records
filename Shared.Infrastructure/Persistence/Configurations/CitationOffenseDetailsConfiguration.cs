using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class CitationOffenseDetailsConfiguration : IEntityTypeConfiguration<CitationOffenseDetails>
{
    public void Configure(EntityTypeBuilder<CitationOffenseDetails> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CitationId)
            .IsUnique();

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
            .HasForeignKey<CitationOffenseDetails>(x => x.CitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
