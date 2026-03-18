using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class CitationChargeLinkConfiguration : IEntityTypeConfiguration<CitationChargeLink>
{
    public void Configure(EntityTypeBuilder<CitationChargeLink> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.CitationId, x.ChargeId }).IsUnique();
        builder.HasIndex(x => x.CitationId);
        builder.HasIndex(x => x.ChargeId);
    }
}
