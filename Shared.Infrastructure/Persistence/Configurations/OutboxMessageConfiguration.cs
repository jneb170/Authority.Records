using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure.Persistence.Configurations
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken()
                .ValueGeneratedNever();
        }
    }
}
