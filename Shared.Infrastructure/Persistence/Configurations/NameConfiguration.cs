using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Domain.Entities;

namespace Shared.Infrastructure.Persistence.Configurations;

internal sealed class NameConfiguration : IEntityTypeConfiguration<Name>
{
    public void Configure(EntityTypeBuilder<Name> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordNumber)
               .ValueGeneratedOnAdd()
               .UseIdentityColumn(seed: 10000, increment: 1);

        builder.HasIndex(x => x.RecordNumber).IsUnique();

        builder.Property(x => x.NameType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.LastOrBusinessName).HasMaxLength(250).IsRequired();

        // Person-only — all nullable
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.MiddleName).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.SexId).IsRequired(false);
        builder.Property(x => x.RaceId).IsRequired(false);
        builder.Property(x => x.DateOfBirth).IsRequired(false);
        builder.Property(x => x.DriversLicenseNumber).HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.DriversLicenseStateId).IsRequired(false);
        builder.Property(x => x.HeightInches).IsRequired(false);
        builder.Property(x => x.WeightLbs).IsRequired(false);
        builder.Property(x => x.HairColorId).IsRequired(false);
        builder.Property(x => x.EyeColorId).IsRequired(false);

        // Extended person-only fields
        builder.Property(x => x.SuffixId).IsRequired(false);
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
        builder.Property(x => x.IsCitizen).HasDefaultValue(false);
        builder.Property(x => x.DeceasedDate).IsRequired(false);
        builder.Property(x => x.PrimaryLocationId).IsRequired(false);
        builder.Property(x => x.SecondaryLocationId).IsRequired(false);
    }
}
