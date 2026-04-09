using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations
{
    internal class CitationConfiguration : IEntityTypeConfiguration<Citation>
    {
        public void Configure(EntityTypeBuilder<Citation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RecordNumber)
                   .ValueGeneratedOnAdd()
                   .UseIdentityColumn(seed: 10000, increment: 1);

            builder.HasIndex(x => x.RecordNumber)
                   .IsUnique();

            builder.Property(x => x.CourtId)
                   .IsRequired(false);

            builder.Property(x => x.DefendantNameId)
                   .IsRequired(false);

            builder.Property(x => x.CitationNum)
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.Property(x => x.LocationId).IsRequired(false);

            builder.Property(x => x.Status)
                   .HasConversion<int>();
        }
    }
}
