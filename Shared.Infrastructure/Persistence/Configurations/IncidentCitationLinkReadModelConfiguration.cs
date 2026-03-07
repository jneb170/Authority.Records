using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class IncidentCitationLinkReadModelConfiguration : IEntityTypeConfiguration<IncidentCitationLinkReadModel>
{
    public void Configure(EntityTypeBuilder<IncidentCitationLinkReadModel> builder)
    {
        builder.ToTable("IncidentCitationLinkReadModels");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.CitationId);
        builder.HasIndex(x => x.IncidentId);
        builder.Property(x => x.IncidentNum).HasMaxLength(100);
    }
}
