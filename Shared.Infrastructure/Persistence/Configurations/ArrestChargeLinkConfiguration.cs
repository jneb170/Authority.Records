using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class ArrestChargeLinkConfiguration : IEntityTypeConfiguration<ArrestChargeLink>
{
    public void Configure(EntityTypeBuilder<ArrestChargeLink> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ArrestId, x.ChargeId }).IsUnique();
        builder.HasIndex(x => x.ArrestId);
        builder.HasIndex(x => x.ChargeId);
    }
}
