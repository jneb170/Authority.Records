using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// Master Name Index (MNI) record. Represents either an individual person or a business entity.
/// </summary>
public sealed class Name : LockableAggregateRoot<Name>, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }

    /// <summary>"Person" or "Business". See <see cref="NameTypes"/>.</summary>
    public string NameType { get; private set; } = string.Empty;

    /// <summary>Last name for persons; business name for businesses. Primary sort/search field.</summary>
    public string LastOrBusinessName { get; private set; } = string.Empty;

    // --- Person-only fields ---
    public string? FirstName { get; private set; }
    public string? MiddleName { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.Sex) — Person only.</summary>
    public Guid? SexId { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.Race) — Person only.</summary>
    public Guid? RaceId { get; private set; }

    public DateTime? DateOfBirth { get; private set; }
    public string? DriversLicenseNumber { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.State) — Person only.</summary>
    public Guid? DriversLicenseStateId { get; private set; }

    /// <summary>Height in total inches — Person only. Display as ft/in in UI.</summary>
    public int? HeightInches { get; private set; }

    /// <summary>Weight in pounds — Person only.</summary>
    public int? WeightLbs { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.HairColor) — Person only.</summary>
    public Guid? HairColorId { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.EyeColor) — Person only.</summary>
    public Guid? EyeColorId { get; private set; }

    // --- Extended person-only fields ---
    /// <summary>Optional picklist FK (PicklistTypes.Suffix) — Person only. e.g. JR, SR, MD.</summary>
    public Guid? SuffixId { get; private set; }

    /// <summary>City/state/country of birth — Person only.</summary>
    public string? PlaceOfBirth { get; private set; }

    /// <summary>FBI identification number — Person only.</summary>
    public string? FbiNumber { get; private set; }

    /// <summary>Local agency-assigned identifier — Person only.</summary>
    public string? LocalNumber { get; private set; }

    /// <summary>Social Security Number stored as formatted string (XXX-XX-XXXX) — Person only.</summary>
    public string? SocialSecurityNumber { get; private set; }

    /// <summary>US Citizen flag — Person only. Defaults to false.</summary>
    public bool IsCitizen { get; private set; }

    /// <summary>Date of death when known — Person only.</summary>
    public DateTime? DeceasedDate { get; private set; }

    /// <summary>DB-generated auto-increment. Use in URLs and display; the GUID is for internal identity.</summary>
    public long RecordNumber { get; private set; }

    /// <summary>User ID who soft-deleted this record, if applicable.</summary>
    public Guid? DeletedBy { get; private set; }
    
    /// <summary>UTC timestamp when this record was soft-deleted, if applicable.</summary>
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>Optional reference to a Master Location Index record for the primary address.</summary>
    public Guid? PrimaryLocationId { get; private set; }

    /// <summary>Optional reference to a Master Location Index record for the secondary address.</summary>
    public Guid? SecondaryLocationId { get; private set; }

    // --- Policy wiring ---
    private static readonly NameAuthorizationPolicy _authorizationPolicy = new();
    protected override IAuthorizationPolicy<Name> AuthorizationPolicy => _authorizationPolicy;

    private static readonly TimeoutLockExpirationStrategy<Name> _lockExpirationStrategy = new();
    protected override ILockExpirationStrategy<Name> LockExpirationStrategy => _lockExpirationStrategy;

    private static readonly SystemClock _clock = new();
    protected override IClock Clock => _clock;

    private Name() { } // EF Core materialization

    public Name(
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
        DateTime? deceasedDate)
    {
        Id               = Guid.NewGuid();
        JurisdictionId   = jurisdictionId;
        AgencyId         = agencyId;
        NameType         = nameType;
        LastOrBusinessName      = lastOrBusinessName;
        FirstName        = firstName;
        MiddleName       = middleName;
        SexId            = sexId;
        RaceId           = raceId;
        DateOfBirth      = dateOfBirth;
        DriversLicenseNumber    = driversLicenseNumber;
        DriversLicenseStateId   = driversLicenseStateId;
        HeightInches     = heightInches;
        WeightLbs        = weightLbs;
        HairColorId      = hairColorId;
        EyeColorId       = eyeColorId;
        SuffixId         = suffixId;
        PlaceOfBirth     = placeOfBirth;
        FbiNumber        = fbiNumber;
        LocalNumber      = localNumber;
        SocialSecurityNumber = socialSecurityNumber;
        IsCitizen        = isCitizen;
        DeceasedDate     = deceasedDate;

        AddDomainEvent(new NameCreatedDomainEvent(
            Id, JurisdictionId, NameType, LastOrBusinessName, FirstName, MiddleName));
    }

    public void UpdateDetails(
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
        IModificationContext context)
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

        AddDomainEvent(new NameDetailsUpdatedDomainEvent(
            Id, NameType, LastOrBusinessName, FirstName, MiddleName,
            SexId, RaceId, DateOfBirth, DriversLicenseNumber, DriversLicenseStateId,
            HeightInches, WeightLbs, HairColorId, EyeColorId,
            SuffixId, PlaceOfBirth, FbiNumber, LocalNumber, SocialSecurityNumber, IsCitizen, DeceasedDate));
    }

    /// <summary>Sets or clears the primary and/or secondary address from the Master Location Index.</summary>
    public void SetLocations(Guid? primaryLocationId, Guid? secondaryLocationId, IModificationContext context)
    {
        PrimaryLocationId   = primaryLocationId;
        SecondaryLocationId = secondaryLocationId;
    }

    public override void SoftDelete(Guid userId)
    {
        base.SoftDelete(userId);
        DeletedBy = userId;
        DeletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new NameSoftDeletedDomainEvent(Id, userId));
    }

    public override void Restore(Guid userId)
    {
        base.Restore(userId);
        DeletedBy = null;
        DeletedAtUtc = null;
        AddDomainEvent(new NameRestoredDomainEvent(Id, userId));
    }
}
