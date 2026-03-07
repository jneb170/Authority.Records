using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Common.Implementations;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class AgencySequenceCounterEntityConfiguration : IEntityTypeConfiguration<AgencySequenceCounter>
{
    public void Configure(EntityTypeBuilder<AgencySequenceCounter> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CounterKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NextValue)
            .IsRequired();

        // SQL Server rowversion — auto-updated by DB on every write; used for optimistic concurrency
        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // One counter per key per agency per year
        builder.HasIndex(x => new { x.JurisdictionId, x.AgencyId, x.CounterKey, x.Year })
            .IsUnique();
    }
}
