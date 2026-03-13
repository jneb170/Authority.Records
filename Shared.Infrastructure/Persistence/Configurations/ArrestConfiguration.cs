using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations
{
    internal class ArrestConfiguration : IEntityTypeConfiguration<Arrest>
    {
        public void Configure(EntityTypeBuilder<Arrest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RecordNumber)
                   .ValueGeneratedOnAdd()
                   .UseIdentityColumn(seed: 10000, increment: 1);

            builder.HasIndex(x => x.RecordNumber)
                   .IsUnique();

            builder.Property(x => x.ArrestTypeId)
                   .IsRequired(false);

            builder.Property(x => x.ArrestNum)
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.Property(x => x.LocationId).IsRequired(false);
        }
    }
}
