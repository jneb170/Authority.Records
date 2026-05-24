using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class NarrativeReadModelConfiguration : IEntityTypeConfiguration<NarrativeReadModel>
{
    public void Configure(EntityTypeBuilder<NarrativeReadModel> builder)
    {
        builder.ToTable("NarrativeReadModels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(Narrative.MaxTitleLength).IsRequired();
        builder.Property(x => x.Content).IsRequired();

        builder.HasIndex(x => x.JurisdictionId);
    }
}
