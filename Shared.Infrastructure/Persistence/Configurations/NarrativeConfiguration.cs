using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class NarrativeConfiguration : IEntityTypeConfiguration<Narrative>
{
    public void Configure(EntityTypeBuilder<Narrative> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordNumber)
               .ValueGeneratedOnAdd()
               .UseIdentityColumn(seed: 30000, increment: 1);

        builder.HasIndex(x => x.RecordNumber).IsUnique();

        builder.Property(x => x.Title).HasMaxLength(Narrative.MaxTitleLength).IsRequired();

        // Content is long-form: left unbounded (nvarchar(max)/TEXT). The MaxContentLength
        // ceiling is enforced in the domain, not as a column length.
        builder.Property(x => x.Content).IsRequired();

        builder.HasIndex(x => x.JurisdictionId);
    }
}
