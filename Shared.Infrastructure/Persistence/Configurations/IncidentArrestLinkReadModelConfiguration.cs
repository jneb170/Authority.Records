using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class IncidentArrestLinkReadModelConfiguration : IEntityTypeConfiguration<IncidentArrestLinkReadModel>
{
    public void Configure(EntityTypeBuilder<IncidentArrestLinkReadModel> builder)
    {
        builder.ToTable("IncidentArrestLinkReadModels");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ArrestId);
        builder.HasIndex(x => x.IncidentId);
        builder.Property(x => x.IncidentNum).HasMaxLength(100);
    }
}
