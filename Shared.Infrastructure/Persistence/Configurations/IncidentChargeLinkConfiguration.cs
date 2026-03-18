using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class IncidentChargeLinkConfiguration : IEntityTypeConfiguration<IncidentChargeLink>
{
    public void Configure(EntityTypeBuilder<IncidentChargeLink> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.IncidentId, x.ChargeId }).IsUnique();
        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.ChargeId);
    }
}
