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

    /// <summary>DB-generated auto-increment. Use in URLs and display; the GUID is for internal identity.</summary>
    public long RecordNumber { get; private set; }

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
        Guid? eyeColorId)
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

        AddDomainEvent(new NameDetailsUpdatedDomainEvent(
            Id, NameType, LastOrBusinessName, FirstName, MiddleName,
            SexId, RaceId, DateOfBirth, DriversLicenseNumber, DriversLicenseStateId,
            HeightInches, WeightLbs, HairColorId, EyeColorId));
    }

    public override void SoftDelete(Guid userId)
    {
        base.SoftDelete(userId);
        AddDomainEvent(new NameSoftDeletedDomainEvent(Id, userId));
    }

    public override void Restore(Guid userId)
    {
        base.Restore(userId);
        AddDomainEvent(new NameRestoredDomainEvent(Id, userId));
    }
}
