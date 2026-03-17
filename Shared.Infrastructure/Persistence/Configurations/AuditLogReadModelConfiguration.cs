using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class AuditLogReadModelConfiguration : IEntityTypeConfiguration<AuditLogReadModel>
{
    public void Configure(EntityTypeBuilder<AuditLogReadModel> builder)
    {
        builder.ToTable("AuditTrailEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.Severity).IsRequired().HasMaxLength(32);
        builder.Property(x => x.RecordType).IsRequired().HasMaxLength(64);
        builder.Property(x => x.ActionType).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Payload).IsRequired();

        builder.HasIndex(x => x.JurisdictionId);
        builder.HasIndex(x => x.OccurredOnUtc);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.Severity);
        builder.HasIndex(x => x.RecordType);
        builder.HasIndex(x => x.ActionType);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.JurisdictionId, x.OccurredOnUtc });
    }
}
