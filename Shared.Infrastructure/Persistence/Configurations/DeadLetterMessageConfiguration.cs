using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class DeadLetterMessageConfiguration : IEntityTypeConfiguration<DeadLetterMessage>
{
    public void Configure(EntityTypeBuilder<DeadLetterMessage> builder)
    {
        builder.ToTable("DeadLetterMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Content).IsRequired();

        builder.HasIndex(x => x.OriginalMessageId);
        builder.HasIndex(x => x.JurisdictionId);
        builder.HasIndex(x => x.DeadLetteredOnUtc);
    }
}
