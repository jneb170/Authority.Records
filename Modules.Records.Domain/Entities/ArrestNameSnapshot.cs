using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Entities;

public sealed class ArrestNameSnapshot : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid ArrestId { get; private set; }
    public Guid? SourceNameId { get; private set; }
    public long? SourceNameRecordNumber { get; private set; }
    public DateTime? LastCopiedAtUtc { get; private set; }
    public Guid? LastCopiedByUserId { get; private set; }

    public string NameType { get; private set; } = string.Empty;
    public string LastOrBusinessName { get; private set; } = string.Empty;
    public string? FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public Guid? SexId { get; private set; }
    public Guid? RaceId { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? DriversLicenseNumber { get; private set; }
    public Guid? DriversLicenseStateId { get; private set; }
    public int? HeightInches { get; private set; }
    public int? WeightLbs { get; private set; }
    public Guid? HairColorId { get; private set; }
    public Guid? EyeColorId { get; private set; }
    public Guid? SuffixId { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? FbiNumber { get; private set; }
    public string? LocalNumber { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public string? PrimaryPhoneExtension { get; private set; }
    public string? WorkPhone { get; private set; }
    public string? WorkPhoneExtension { get; private set; }
    public string? OtherPhone { get; private set; }
    public string? OtherPhoneExtension { get; private set; }
    public string? SocialSecurityNumber { get; private set; }
    public bool IsCitizen { get; private set; }
    public DateTime? DeceasedDate { get; private set; }

    public Guid? PrimaryLocationId { get; private set; }
    public long? PrimaryLocationRecordNumber { get; private set; }
    public string? PrimaryLocationAddress { get; private set; }
    public Guid? SecondaryLocationId { get; private set; }
    public long? SecondaryLocationRecordNumber { get; private set; }
    public string? SecondaryLocationAddress { get; private set; }

    private ArrestNameSnapshot()
    {
    }

    private ArrestNameSnapshot(Guid jurisdictionId, Guid agencyId, Guid arrestId)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        ArrestId = arrestId;
    }

    public static ArrestNameSnapshot Create(
        Guid jurisdictionId,
        Guid agencyId,
        Guid arrestId,
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        string nameType,
        string lastOrBusinessName,
        string? firstName,
        string? middleName,
        Guid? sexId,
        Guid? raceId,
        DateTime? dateOfBirth,
        string? driversLicenseNumber,
        Guid? driversLicenseStateId,
        int? heightInches,
        int? weightLbs,
        Guid? hairColorId,
        Guid? eyeColorId,
        Guid? suffixId,
        string? placeOfBirth,
        string? fbiNumber,
        string? localNumber,
        string? primaryPhone,
        string? primaryPhoneExtension,
        string? workPhone,
        string? workPhoneExtension,
        string? otherPhone,
        string? otherPhoneExtension,
        string? socialSecurityNumber,
        bool isCitizen,
        DateTime? deceasedDate,
        Guid? primaryLocationId,
        long? primaryLocationRecordNumber,
        string? primaryLocationAddress,
        Guid? secondaryLocationId,
        long? secondaryLocationRecordNumber,
        string? secondaryLocationAddress,
        Guid copiedByUserId)
    {
        var snapshot = new ArrestNameSnapshot(jurisdictionId, agencyId, arrestId);
        snapshot.ApplyValues(
            sourceNameId,
            sourceNameRecordNumber,
            nameType,
            lastOrBusinessName,
            firstName,
            middleName,
            sexId,
            raceId,
            dateOfBirth,
            driversLicenseNumber,
            driversLicenseStateId,
            heightInches,
            weightLbs,
            hairColorId,
            eyeColorId,
            suffixId,
            placeOfBirth,
            fbiNumber,
            localNumber,
            primaryPhone,
            primaryPhoneExtension,
            workPhone,
            workPhoneExtension,
            otherPhone,
            otherPhoneExtension,
            socialSecurityNumber,
            isCitizen,
            deceasedDate,
            primaryLocationId,
            primaryLocationRecordNumber,
            primaryLocationAddress,
            secondaryLocationId,
            secondaryLocationRecordNumber,
            secondaryLocationAddress);
        snapshot.MarkCopied(copiedByUserId);
        return snapshot;
    }

    public void RefreshFromSource(
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        string nameType,
        string lastOrBusinessName,
        string? firstName,
        string? middleName,
        Guid? sexId,
        Guid? raceId,
        DateTime? dateOfBirth,
        string? driversLicenseNumber,
        Guid? driversLicenseStateId,
        int? heightInches,
        int? weightLbs,
        Guid? hairColorId,
        Guid? eyeColorId,
        Guid? suffixId,
        string? placeOfBirth,
        string? fbiNumber,
        string? localNumber,
        string? primaryPhone,
        string? primaryPhoneExtension,
        string? workPhone,
        string? workPhoneExtension,
        string? otherPhone,
        string? otherPhoneExtension,
        string? socialSecurityNumber,
        bool isCitizen,
        DateTime? deceasedDate,
        Guid? primaryLocationId,
        long? primaryLocationRecordNumber,
        string? primaryLocationAddress,
        Guid? secondaryLocationId,
        long? secondaryLocationRecordNumber,
        string? secondaryLocationAddress,
        Guid copiedByUserId)
    {
        ApplyValues(
            sourceNameId,
            sourceNameRecordNumber,
            nameType,
            lastOrBusinessName,
            firstName,
            middleName,
            sexId,
            raceId,
            dateOfBirth,
            driversLicenseNumber,
            driversLicenseStateId,
            heightInches,
            weightLbs,
            hairColorId,
            eyeColorId,
            suffixId,
            placeOfBirth,
            fbiNumber,
            localNumber,
            primaryPhone,
            primaryPhoneExtension,
            workPhone,
            workPhoneExtension,
            otherPhone,
            otherPhoneExtension,
            socialSecurityNumber,
            isCitizen,
            deceasedDate,
            primaryLocationId,
            primaryLocationRecordNumber,
            primaryLocationAddress,
            secondaryLocationId,
            secondaryLocationRecordNumber,
            secondaryLocationAddress);
        MarkCopied(copiedByUserId);
    }

    public void UpdateDetails(
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        string nameType,
        string lastOrBusinessName,
        string? firstName,
        string? middleName,
        Guid? sexId,
        Guid? raceId,
        DateTime? dateOfBirth,
        string? driversLicenseNumber,
        Guid? driversLicenseStateId,
        int? heightInches,
        int? weightLbs,
        Guid? hairColorId,
        Guid? eyeColorId,
        Guid? suffixId,
        string? placeOfBirth,
        string? fbiNumber,
        string? localNumber,
        string? primaryPhone,
        string? primaryPhoneExtension,
        string? workPhone,
        string? workPhoneExtension,
        string? otherPhone,
        string? otherPhoneExtension,
        string? socialSecurityNumber,
        bool isCitizen,
        DateTime? deceasedDate,
        Guid? primaryLocationId,
        long? primaryLocationRecordNumber,
        string? primaryLocationAddress,
        Guid? secondaryLocationId,
        long? secondaryLocationRecordNumber,
        string? secondaryLocationAddress)
    {
        ApplyValues(
            sourceNameId,
            sourceNameRecordNumber,
            nameType,
            lastOrBusinessName,
            firstName,
            middleName,
            sexId,
            raceId,
            dateOfBirth,
            driversLicenseNumber,
            driversLicenseStateId,
            heightInches,
            weightLbs,
            hairColorId,
            eyeColorId,
            suffixId,
            placeOfBirth,
            fbiNumber,
            localNumber,
            primaryPhone,
            primaryPhoneExtension,
            workPhone,
            workPhoneExtension,
            otherPhone,
            otherPhoneExtension,
            socialSecurityNumber,
            isCitizen,
            deceasedDate,
            primaryLocationId,
            primaryLocationRecordNumber,
            primaryLocationAddress,
            secondaryLocationId,
            secondaryLocationRecordNumber,
            secondaryLocationAddress);
    }

    private void ApplyValues(
        Guid? sourceNameId,
        long? sourceNameRecordNumber,
        string nameType,
        string lastOrBusinessName,
        string? firstName,
        string? middleName,
        Guid? sexId,
        Guid? raceId,
        DateTime? dateOfBirth,
        string? driversLicenseNumber,
        Guid? driversLicenseStateId,
        int? heightInches,
        int? weightLbs,
        Guid? hairColorId,
        Guid? eyeColorId,
        Guid? suffixId,
        string? placeOfBirth,
        string? fbiNumber,
        string? localNumber,
        string? primaryPhone,
        string? primaryPhoneExtension,
        string? workPhone,
        string? workPhoneExtension,
        string? otherPhone,
        string? otherPhoneExtension,
        string? socialSecurityNumber,
        bool isCitizen,
        DateTime? deceasedDate,
        Guid? primaryLocationId,
        long? primaryLocationRecordNumber,
        string? primaryLocationAddress,
        Guid? secondaryLocationId,
        long? secondaryLocationRecordNumber,
        string? secondaryLocationAddress)
    {
        SourceNameId = sourceNameId;
        SourceNameRecordNumber = sourceNameRecordNumber;
        NameType = nameType;
        LastOrBusinessName = lastOrBusinessName;
        FirstName = firstName;
        MiddleName = middleName;
        SexId = sexId;
        RaceId = raceId;
        DateOfBirth = dateOfBirth;
        DriversLicenseNumber = driversLicenseNumber;
        DriversLicenseStateId = driversLicenseStateId;
        HeightInches = heightInches;
        WeightLbs = weightLbs;
        HairColorId = hairColorId;
        EyeColorId = eyeColorId;
        SuffixId = suffixId;
        PlaceOfBirth = placeOfBirth;
        FbiNumber = fbiNumber;
        LocalNumber = localNumber;
        PrimaryPhone = primaryPhone;
        PrimaryPhoneExtension = primaryPhoneExtension;
        WorkPhone = workPhone;
        WorkPhoneExtension = workPhoneExtension;
        OtherPhone = otherPhone;
        OtherPhoneExtension = otherPhoneExtension;
        SocialSecurityNumber = socialSecurityNumber;
        IsCitizen = isCitizen;
        DeceasedDate = deceasedDate;
        PrimaryLocationId = primaryLocationId;
        PrimaryLocationRecordNumber = primaryLocationRecordNumber;
        PrimaryLocationAddress = primaryLocationAddress;
        SecondaryLocationId = secondaryLocationId;
        SecondaryLocationRecordNumber = secondaryLocationRecordNumber;
        SecondaryLocationAddress = secondaryLocationAddress;
    }

    private void MarkCopied(Guid copiedByUserId)
    {
        LastCopiedByUserId = copiedByUserId;
        LastCopiedAtUtc = DateTime.UtcNow;
    }
}
