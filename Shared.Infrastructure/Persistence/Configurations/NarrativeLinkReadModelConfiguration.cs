using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class NarrativeLinkReadModelConfiguration : IEntityTypeConfiguration<NarrativeLinkReadModel>
{
    public void Configure(EntityTypeBuilder<NarrativeLinkReadModel> builder)
    {
        builder.ToTable("NarrativeLinkReadModels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerType).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.JurisdictionId, x.OwnerType, x.OwnerId });
        builder.HasIndex(x => x.NarrativeId);
    }
}
