using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class IncidentReadModelConfiguration : IEntityTypeConfiguration<IncidentReadModel>
{
    public void Configure(EntityTypeBuilder<IncidentReadModel> builder)
    {
        builder.ToTable("IncidentReadModels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);

        builder.HasIndex(x => x.JurisdictionId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.UpdatedAtUtc);
    }
}
