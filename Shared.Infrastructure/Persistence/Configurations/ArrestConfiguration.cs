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

            builder.Property(x => x.NameId).IsRequired(false);
            builder.Property(x => x.PrimaryIncidentId).IsRequired(false);
            builder.Property(x => x.LocationId).IsRequired(false);

            builder.HasIndex(x => x.NameId);
            builder.HasIndex(x => x.PrimaryIncidentId);

            builder.HasOne<Name>()
                   .WithMany()
                   .HasForeignKey(x => x.NameId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Incident>()
                   .WithMany()
                   .HasForeignKey(x => x.PrimaryIncidentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
