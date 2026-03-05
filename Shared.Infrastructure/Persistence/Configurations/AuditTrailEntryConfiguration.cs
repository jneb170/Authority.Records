using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Audit;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class AuditTrailEntryConfiguration : IEntityTypeConfiguration<AuditTrailEntry>
{
    public void Configure(EntityTypeBuilder<AuditTrailEntry> builder)
    {
        builder.ToTable("AuditTrailEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.Payload).IsRequired();

        builder.HasIndex(x => x.JurisdictionId);
        builder.HasIndex(x => x.OccurredOnUtc);
        builder.HasIndex(x => x.EventType);
    }
}
