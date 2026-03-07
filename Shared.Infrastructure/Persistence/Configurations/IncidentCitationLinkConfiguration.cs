using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class IncidentCitationLinkConfiguration : IEntityTypeConfiguration<IncidentCitationLink>
{
    public void Configure(EntityTypeBuilder<IncidentCitationLink> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.IncidentId, x.CitationId }).IsUnique();
        builder.HasIndex(x => x.CitationId);
        builder.HasIndex(x => x.IncidentId);
    }
}
