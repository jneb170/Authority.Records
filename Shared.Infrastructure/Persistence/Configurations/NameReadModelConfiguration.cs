using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Records.Application.ReadModels;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class NameReadModelConfiguration : IEntityTypeConfiguration<NameReadModel>
{
    public void Configure(EntityTypeBuilder<NameReadModel> builder)
    {
        builder.ToTable("NameReadModels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.LastOrBusinessName).HasMaxLength(250).IsRequired();
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
        builder.Property(x => x.SocialSecurityNumber).HasMaxLength(11).IsRequired(false);
        builder.Property(x => x.IsCitizen).HasDefaultValue(false);
        builder.Property(x => x.DeceasedDate).IsRequired(false);
        builder.Property(x => x.PrimaryLocationId).IsRequired(false);
        builder.Property(x => x.SecondaryLocationId).IsRequired(false);

        // Search indexes — all keyed on JurisdictionId for multi-tenant isolation
        builder.HasIndex(x => new { x.JurisdictionId, x.LastOrBusinessName });
        builder.HasIndex(x => new { x.JurisdictionId, x.FirstName });
        builder.HasIndex(x => new { x.JurisdictionId, x.NameType });
        builder.HasIndex(x => new { x.JurisdictionId, x.SexId });
        builder.HasIndex(x => new { x.JurisdictionId, x.RaceId });
        builder.HasIndex(x => new { x.JurisdictionId, x.DateOfBirth });
        builder.HasIndex(x => new { x.JurisdictionId, x.HeightInches });
        builder.HasIndex(x => new { x.JurisdictionId, x.WeightLbs });
        builder.HasIndex(x => new { x.JurisdictionId, x.HairColorId });
        builder.HasIndex(x => new { x.JurisdictionId, x.EyeColorId });
        builder.HasIndex(x => new { x.JurisdictionId, x.SuffixId });
        builder.HasIndex(x => new { x.JurisdictionId, x.DeceasedDate });
        builder.HasIndex(x => x.DriversLicenseNumber);
    }
}
