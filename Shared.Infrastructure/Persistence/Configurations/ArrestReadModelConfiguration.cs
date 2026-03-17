using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class ArrestReadModelConfiguration : IEntityTypeConfiguration<ArrestReadModel>
{
    public void Configure(EntityTypeBuilder<ArrestReadModel> builder)
    {
        builder.ToTable("ArrestReadModels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ArrestNum).HasMaxLength(50).IsRequired(false);

        builder.HasIndex(x => x.JurisdictionId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.NameId);
        builder.HasIndex(x => x.PrimaryIncidentId);
    }
}
