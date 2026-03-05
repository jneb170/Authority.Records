using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class CitationReadModelConfiguration : IEntityTypeConfiguration<CitationReadModel>
{
    public void Configure(EntityTypeBuilder<CitationReadModel> builder)
    {
        builder.ToTable("CitationReadModels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).IsRequired();

        builder.HasIndex(x => x.JurisdictionId);
        builder.HasIndex(x => x.IsIssued);
        builder.HasIndex(x => x.UpdatedAtUtc);
    }
}
