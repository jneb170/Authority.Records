using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class IncidentArrestLinkConfiguration : IEntityTypeConfiguration<IncidentArrestLink>
{
    public void Configure(EntityTypeBuilder<IncidentArrestLink> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.IncidentId, x.ArrestId }).IsUnique();
        builder.HasIndex(x => x.ArrestId);
        builder.HasIndex(x => x.IncidentId);
    }
}
