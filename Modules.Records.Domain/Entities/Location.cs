using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// Master Location Index (MLI) record. Represents a physical address shared across all agencies
/// within a jurisdiction. Used by Incidents, Arrests, Citations, and Names.
/// </summary>
public sealed class Location : LockableAggregateRoot<Location>, IMultiTenant
{
    /// <summary>Jurisdiction that owns this record. Locations are shared across all agencies in the jurisdiction.</summary>
    public Guid JurisdictionId { get; private set; }

    /// <summary>DB-generated auto-increment. Use in URLs and display; the GUID is for internal identity.</summary>
    public long RecordNumber { get; private set; }

    // --- Address fields ---

    /// <summary>House/building number (e.g. "123"). Optional — some locations have no street number.</summary>
    public string? StreetNumber { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.Direction) — pre-directional (e.g. "N", "SW").</summary>
    public Guid? PreDirectionId { get; private set; }

    /// <summary>Street name (e.g. "Main"). Required.</summary>
    public string StreetAddress { get; private set; } = string.Empty;

    /// <summary>Optional picklist FK (PicklistTypes.StreetType) — e.g. "St", "Ave", "Blvd".</summary>
    public Guid? StreetTypeId { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.Direction) — post-directional (e.g. "NW").</summary>
    public Guid? PostDirectionId { get; private set; }

    /// <summary>City name. Required.</summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>Optional picklist FK (PicklistTypes.State).</summary>
    public Guid? StateId { get; private set; }

    /// <summary>Optional picklist FK (PicklistTypes.Country). Defaults to US if omitted.</summary>
    public Guid? CountryId { get; private set; }

    public string? Zip { get; private set; }

    /// <summary>Apartment, suite, or unit number.</summary>
    public string? AptSuite { get; private set; }

    /// <summary>Latitude/longitude or other coordinate string.</summary>
    public string? Coordinates { get; private set; }

    /// <summary>Informal name for the location (e.g. "City Hall", "Riverside Park").</summary>
    public string? CommonPlaceName { get; private set; }

    public string? Comments { get; private set; }

    /// <summary>Full formatted address as returned by Google Maps (e.g. "3001 E Plano Pkwy #400, Plano, TX 75074, USA").</summary>
    public string? Address { get; private set; }

    /// <summary>User ID who soft-deleted this record, if applicable.</summary>
    public Guid? DeletedBy { get; private set; }

    /// <summary>UTC timestamp when this record was soft-deleted, if applicable.</summary>
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>
    /// Transient lock-state field: the agency the lock owner held the lock under. A Location is the
    /// Master Location Index — shared across all agencies in its jurisdiction — so it has no permanent
    /// AgencyId. But the record-lock timeout is configured per agency, so cleanup needs to know which
    /// agency's timeout governs an outstanding lock. Set on <see cref="AcquireLock(IModificationContext, TimeSpan, Guid)"/>
    /// from the locking user's agency and cleared when the lock is released. Null when not locked.
    /// </summary>
    public Guid? LockedByAgencyId { get; private set; }

    // --- Policy wiring ---
    private static readonly LocationAuthorizationPolicy _authorizationPolicy = new();
    protected override IAuthorizationPolicy<Location> AuthorizationPolicy => _authorizationPolicy;

    private static readonly TimeoutLockExpirationStrategy<Location> _lockExpirationStrategy = new();
    protected override ILockExpirationStrategy<Location> LockExpirationStrategy => _lockExpirationStrategy;

    private static readonly SystemClock _clock = new();
    protected override IClock Clock => _clock;

    private Location() { } // EF Core materialization

    public Location(
        Guid jurisdictionId,
        string streetAddress,
        string city,
        string? streetNumber = null,
        Guid? preDirectionId = null,
        Guid? streetTypeId = null,
        Guid? postDirectionId = null,
        Guid? stateId = null,
        Guid? countryId = null,
        string? zip = null,
        string? aptSuite = null,
        string? coordinates = null,
        string? commonPlaceName = null,
        string? comments = null,
        string? address = null)
    {
        Id             = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        StreetAddress  = streetAddress;
        City           = city;
        StreetNumber   = streetNumber;
        PreDirectionId = preDirectionId;
        StreetTypeId   = streetTypeId;
        PostDirectionId = postDirectionId;
        StateId        = stateId;
        CountryId      = countryId;
        Zip            = zip;
        AptSuite       = aptSuite;
        Coordinates    = coordinates;
        CommonPlaceName = commonPlaceName;
        Comments       = comments;
        Address        = address;

        AddDomainEvent(new LocationCreatedDomainEvent(
            Id, JurisdictionId, CommonPlaceName, StreetAddress, City, StateId));
    }

    public void UpdateDetails(
        string streetAddress,
        string city,
        string? streetNumber,
        Guid? preDirectionId,
        Guid? streetTypeId,
        Guid? postDirectionId,
        Guid? stateId,
        Guid? countryId,
        string? zip,
        string? aptSuite,
        string? coordinates,
        string? commonPlaceName,
        string? comments,
        string? address,
        IModificationContext context)
    {
        EnsureCanModify(context);

        StreetAddress   = streetAddress;
        City            = city;
        StreetNumber    = streetNumber;
        PreDirectionId  = preDirectionId;
        StreetTypeId    = streetTypeId;
        PostDirectionId = postDirectionId;
        StateId         = stateId;
        CountryId       = countryId;
        Zip             = zip;
        AptSuite        = aptSuite;
        Coordinates     = coordinates;
        CommonPlaceName = commonPlaceName;
        Comments        = comments;
        Address         = address;

        AddDomainEvent(new LocationDetailsUpdatedDomainEvent(
            Id, StreetNumber, PreDirectionId, StreetAddress, StreetTypeId, PostDirectionId,
            City, StateId, CountryId, Zip, AptSuite, Coordinates, CommonPlaceName, Comments, Address));
    }

    /// <summary>
    /// Acquires the modify lock and records the agency whose configured timeout governs it.
    /// Locations carry no permanent AgencyId (see <see cref="LockedByAgencyId"/>), so the locking
    /// agency must be supplied by the caller (the acquiring user's current agency).
    /// </summary>
    public void AcquireLock(IModificationContext context, TimeSpan lockTimeout, Guid lockingAgencyId)
    {
        base.AcquireLock(context, lockTimeout);
        LockedByAgencyId = lockingAgencyId;
    }

    public override void ReleaseLock(IModificationContext context)
    {
        base.ReleaseLock(context);
        LockedByAgencyId = null;
    }

    public override void SoftDelete(Guid userId)
    {
        base.SoftDelete(userId);
        DeletedBy    = userId;
        DeletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new LocationSoftDeletedDomainEvent(Id, userId));
    }

    public override void Restore(Guid userId)
    {
        base.Restore(userId);
        DeletedBy    = null;
        DeletedAtUtc = null;
        AddDomainEvent(new LocationRestoredDomainEvent(Id, userId));
    }
}
