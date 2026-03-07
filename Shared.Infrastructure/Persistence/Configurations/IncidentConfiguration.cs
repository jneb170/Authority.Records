using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordNumber)
               .ValueGeneratedOnAdd()
               .UseIdentityColumn(seed: 10000, increment: 1);

        builder.HasIndex(x => x.RecordNumber)
               .IsUnique();

        builder.Property(x => x.CFSNum)
               .HasMaxLength(30)
               .IsRequired()
               .HasDefaultValue(string.Empty);

        builder.Property(x => x.LocalNum)
               .HasMaxLength(30)
               .HasDefaultValue(string.Empty);

        builder.Property(x => x.RowVersion)
               .IsConcurrencyToken()
               .ValueGeneratedNever();

        // Enforce uniqueness of IncidentNum per agency (blank values are excluded — they have no number yet)
        builder.HasIndex(x => new { x.JurisdictionId, x.AgencyId, x.IncidentNum })
               .IsUnique()
               .HasFilter("[IncidentNum] <> ''");

        // Details is a computed [NotMapped] property — EF must not try to map it
        builder.Ignore(x => x.Details);
    }
}
