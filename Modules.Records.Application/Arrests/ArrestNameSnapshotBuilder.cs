using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests;

internal static class ArrestNameSnapshotBuilder
{
    public static ArrestNameSnapshot CreateFromName(
        Arrest arrest,
        Name name,
        Location? primaryLocation,
        Location? secondaryLocation,
        Guid copiedByUserId)
        => ArrestNameSnapshot.Create(
            arrest.JurisdictionId,
            arrest.AgencyId,
            arrest.Id,
            name.Id,
            name.RecordNumber,
            name.NameType,
            name.LastOrBusinessName,
            name.FirstName,
            name.MiddleName,
            name.SexId,
            name.RaceId,
            name.DateOfBirth,
            name.DriversLicenseNumber,
            name.DriversLicenseStateId,
            name.HeightInches,
            name.WeightLbs,
            name.HairColorId,
            name.EyeColorId,
            name.SuffixId,
            name.PlaceOfBirth,
            name.FbiNumber,
            name.LocalNumber,
            name.PrimaryPhone,
            name.PrimaryPhoneExtension,
            name.WorkPhone,
            name.WorkPhoneExtension,
            name.OtherPhone,
            name.OtherPhoneExtension,
            name.SocialSecurityNumber,
            name.IsCitizen,
            name.DeceasedDate,
            primaryLocation?.Id,
            primaryLocation?.RecordNumber,
            FormatLocationAddress(primaryLocation),
            secondaryLocation?.Id,
            secondaryLocation?.RecordNumber,
            FormatLocationAddress(secondaryLocation),
            copiedByUserId);

    public static void UpdateFromInput(
        ArrestNameSnapshot snapshot,
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        NameSnapshotInput input)
        => snapshot.UpdateDetails(
            sourceNameId,
            sourceNameRecordNumber,
            input.NameType,
            input.LastOrBusinessName,
            input.FirstName,
            input.MiddleName,
            input.SexId,
            input.RaceId,
            input.DateOfBirth,
            input.DriversLicenseNumber,
            input.DriversLicenseStateId,
            input.HeightInches,
            input.WeightLbs,
            input.HairColorId,
            input.EyeColorId,
            input.SuffixId,
            input.PlaceOfBirth,
            input.FbiNumber,
            input.LocalNumber,
            input.PrimaryPhone,
            input.PrimaryPhoneExtension,
            input.WorkPhone,
            input.WorkPhoneExtension,
            input.OtherPhone,
            input.OtherPhoneExtension,
            input.SocialSecurityNumber,
            input.IsCitizen,
            input.DeceasedDate,
            input.PrimaryAddress?.LocationId,
            input.PrimaryAddress?.LocationRecordNumber,
            input.PrimaryAddress?.Address,
            input.SecondaryAddress?.LocationId,
            input.SecondaryAddress?.LocationRecordNumber,
            input.SecondaryAddress?.Address);

    public static ArrestNameSnapshot CreateFromInput(
        Arrest arrest,
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        NameSnapshotInput input,
        Guid copiedByUserId)
        => ArrestNameSnapshot.Create(
            arrest.JurisdictionId,
            arrest.AgencyId,
            arrest.Id,
            sourceNameId,
            sourceNameRecordNumber,
            input.NameType,
            input.LastOrBusinessName,
            input.FirstName,
            input.MiddleName,
            input.SexId,
            input.RaceId,
            input.DateOfBirth,
            input.DriversLicenseNumber,
            input.DriversLicenseStateId,
            input.HeightInches,
            input.WeightLbs,
            input.HairColorId,
            input.EyeColorId,
            input.SuffixId,
            input.PlaceOfBirth,
            input.FbiNumber,
            input.LocalNumber,
            input.PrimaryPhone,
            input.PrimaryPhoneExtension,
            input.WorkPhone,
            input.WorkPhoneExtension,
            input.OtherPhone,
            input.OtherPhoneExtension,
            input.SocialSecurityNumber,
            input.IsCitizen,
            input.DeceasedDate,
            input.PrimaryAddress?.LocationId,
            input.PrimaryAddress?.LocationRecordNumber,
            input.PrimaryAddress?.Address,
            input.SecondaryAddress?.LocationId,
            input.SecondaryAddress?.LocationRecordNumber,
            input.SecondaryAddress?.Address,
            copiedByUserId);

    public static NameSnapshotDto ToDto(ArrestNameSnapshot snapshot) => new(
        snapshot.SourceNameId,
        snapshot.SourceNameRecordNumber,
        snapshot.NameType,
        snapshot.LastOrBusinessName,
        snapshot.FirstName,
        snapshot.MiddleName,
        snapshot.SexId,
        snapshot.RaceId,
        snapshot.DateOfBirth,
        snapshot.DriversLicenseNumber,
        snapshot.DriversLicenseStateId,
        snapshot.HeightInches,
        snapshot.WeightLbs,
        snapshot.HairColorId,
        snapshot.EyeColorId,
        snapshot.SuffixId,
        snapshot.PlaceOfBirth,
        snapshot.FbiNumber,
        snapshot.LocalNumber,
        snapshot.PrimaryPhone,
        snapshot.PrimaryPhoneExtension,
        snapshot.WorkPhone,
        snapshot.WorkPhoneExtension,
        snapshot.OtherPhone,
        snapshot.OtherPhoneExtension,
        snapshot.SocialSecurityNumber,
        snapshot.IsCitizen,
        snapshot.DeceasedDate,
        snapshot.PrimaryLocationId.HasValue || snapshot.PrimaryLocationRecordNumber.HasValue || !string.IsNullOrWhiteSpace(snapshot.PrimaryLocationAddress)
            ? new NameSnapshotAddressDto(snapshot.PrimaryLocationId, snapshot.PrimaryLocationRecordNumber, snapshot.PrimaryLocationAddress)
            : null,
        snapshot.SecondaryLocationId.HasValue || snapshot.SecondaryLocationRecordNumber.HasValue || !string.IsNullOrWhiteSpace(snapshot.SecondaryLocationAddress)
            ? new NameSnapshotAddressDto(snapshot.SecondaryLocationId, snapshot.SecondaryLocationRecordNumber, snapshot.SecondaryLocationAddress)
            : null,
        snapshot.LastCopiedAtUtc,
        snapshot.LastCopiedByUserId);

    private static string? FormatLocationAddress(Location? location)
    {
        if (location is null)
            return null;

        if (!string.IsNullOrWhiteSpace(location.Address))
            return location.Address;

        var line1 = string.Join(" ", new[]
        {
            location.StreetNumber,
            location.StreetAddress
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var line2 = string.Join(", ", new[]
        {
            location.City,
            location.Zip
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var combined = string.Join(", ", new[] { line1, line2 }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }
}
