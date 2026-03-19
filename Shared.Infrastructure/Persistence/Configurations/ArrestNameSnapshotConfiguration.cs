using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class ArrestNameSnapshotConfiguration : IEntityTypeConfiguration<ArrestNameSnapshot>
{
    public void Configure(EntityTypeBuilder<ArrestNameSnapshot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ArrestId)
            .IsUnique();

        builder.Property(x => x.NameType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.LastOrBusinessName)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.MiddleName).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.DriversLicenseNumber).HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.PlaceOfBirth).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.FbiNumber).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.LocalNumber).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.PrimaryPhone).HasMaxLength(25).IsRequired(false);
        builder.Property(x => x.PrimaryPhoneExtension).HasMaxLength(10).IsRequired(false);
        builder.Property(x => x.WorkPhone).HasMaxLength(25).IsRequired(false);
        builder.Property(x => x.WorkPhoneExtension).HasMaxLength(10).IsRequired(false);
        builder.Property(x => x.OtherPhone).HasMaxLength(25).IsRequired(false);
        builder.Property(x => x.OtherPhoneExtension).HasMaxLength(10).IsRequired(false);
        builder.Property(x => x.SocialSecurityNumber).HasMaxLength(11).IsRequired(false);
        builder.Property(x => x.PrimaryLocationAddress).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.SecondaryLocationAddress).HasMaxLength(500).IsRequired(false);

        builder.HasOne<Arrest>()
            .WithOne()
            .HasForeignKey<ArrestNameSnapshot>(x => x.ArrestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
