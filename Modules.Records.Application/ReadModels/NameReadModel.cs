using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.ReadModels;

public sealed class NameReadModel
{
    public Guid Id { get; private set; }
    public long RecordNumber { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
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
    public string? SocialSecurityNumber { get; private set; }
    public bool IsCitizen { get; private set; }
    public DateTime? DeceasedDate { get; private set; }
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? PrimaryLocationId { get; private set; }
    public Guid? SecondaryLocationId { get; private set; }
    public string? PrimaryMugshotUrl { get; private set; }

    private NameReadModel() { } // EF

    public static NameReadModel Create(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        Guid agencyId,
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
        string? socialSecurityNumber,
        bool isCitizen,
        DateTime? deceasedDate,
        DateTime createdAtUtc,
        Guid createdBy)
    {
        return new NameReadModel
        {
            Id                    = id,
            RecordNumber          = recordNumber,
            JurisdictionId        = jurisdictionId,
            AgencyId              = agencyId,
            NameType              = nameType,
            LastOrBusinessName    = lastOrBusinessName,
            FirstName             = firstName,
            MiddleName            = middleName,
            SexId                 = sexId,
            RaceId                = raceId,
            DateOfBirth           = dateOfBirth,
            DriversLicenseNumber  = driversLicenseNumber,
            DriversLicenseStateId = driversLicenseStateId,
            HeightInches          = heightInches,
            WeightLbs             = weightLbs,
            HairColorId           = hairColorId,
            EyeColorId            = eyeColorId,
            SuffixId              = suffixId,
            PlaceOfBirth          = placeOfBirth,
            FbiNumber             = fbiNumber,
            LocalNumber           = localNumber,
            SocialSecurityNumber  = socialSecurityNumber,
            IsCitizen             = isCitizen,
            DeceasedDate          = deceasedDate,
            IsLocked              = false,
            CreatedBy             = createdBy,
            CreatedAtUtc          = createdAtUtc,
            UpdatedAtUtc          = createdAtUtc,
        };
    }

    public void ApplyDetailsChanged(
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
        string? socialSecurityNumber,
        bool isCitizen,
        DateTime? deceasedDate)
    {
        NameType              = nameType;
        LastOrBusinessName    = lastOrBusinessName;
        FirstName             = firstName;
        MiddleName            = middleName;
        SexId                 = sexId;
        RaceId                = raceId;
        DateOfBirth           = dateOfBirth;
        DriversLicenseNumber  = driversLicenseNumber;
        DriversLicenseStateId = driversLicenseStateId;
        HeightInches          = heightInches;
        WeightLbs             = weightLbs;
        HairColorId           = hairColorId;
        EyeColorId            = eyeColorId;
        SuffixId              = suffixId;
        PlaceOfBirth          = placeOfBirth;
        FbiNumber             = fbiNumber;
        LocalNumber           = localNumber;
        SocialSecurityNumber  = socialSecurityNumber;
        IsCitizen             = isCitizen;
        DeceasedDate          = deceasedDate;
        UpdatedAtUtc          = DateTime.UtcNow;
    }

    public void ApplyLockAcquired(Guid userId)
    {
        IsLocked       = true;
        LockedByUserId = userId;
    }

    public void ApplyLockReleased()
    {
        IsLocked       = false;
        LockedByUserId = null;
    }

    public void ApplyModifiedAudit(Guid? modifiedBy, DateTime? modifiedAt, DateTime? createdAtFallback = null)
    {
        ModifiedBy   = modifiedBy;
        UpdatedAtUtc = modifiedAt ?? createdAtFallback ?? UpdatedAtUtc;
    }

    public void ApplyLocationChanged(Guid? primaryLocationId, Guid? secondaryLocationId)
    {
        PrimaryLocationId   = primaryLocationId;
        SecondaryLocationId = secondaryLocationId;
        UpdatedAtUtc        = DateTime.UtcNow;
    }

    public void ApplyPrimaryMugshot(string? primaryMugshotUrl)
    {
        PrimaryMugshotUrl = primaryMugshotUrl;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public NameDto ToDto() => new(
        Id, RecordNumber, JurisdictionId, AgencyId, NameType, LastOrBusinessName,
        IsLocked, LockedByUserId, CreatedBy, ModifiedBy, CreatedAtUtc, UpdatedAtUtc,
        FirstName, MiddleName, SexId, RaceId, DateOfBirth,
        DriversLicenseNumber, DriversLicenseStateId, HeightInches, WeightLbs,
        HairColorId, EyeColorId,
        SuffixId, PlaceOfBirth, FbiNumber, LocalNumber, SocialSecurityNumber, IsCitizen, DeceasedDate,
        PrimaryLocationId, SecondaryLocationId, PrimaryMugshotUrl);
}
