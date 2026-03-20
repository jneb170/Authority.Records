using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class CitationVehicleConfiguration : IEntityTypeConfiguration<CitationVehicle>
{
    public void Configure(EntityTypeBuilder<CitationVehicle> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CitationId)
            .IsUnique();

        builder.Property(x => x.PlateNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Make)
            .HasMaxLength(50);

        builder.Property(x => x.Style)
            .HasMaxLength(50);

        builder.Property(x => x.Color)
            .HasMaxLength(50);

        builder.HasOne<Citation>()
            .WithOne()
            .HasForeignKey<CitationVehicle>(x => x.CitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
