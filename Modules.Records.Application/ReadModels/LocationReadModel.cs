using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.ReadModels;

public sealed class LocationReadModel
{
    public Guid Id { get; private set; }
    public long RecordNumber { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public string? StreetNumber { get; private set; }
    public Guid? PreDirectionId { get; private set; }
    public string StreetAddress { get; private set; } = string.Empty;
    public Guid? StreetTypeId { get; private set; }
    public Guid? PostDirectionId { get; private set; }
    public string City { get; private set; } = string.Empty;
    public Guid? StateId { get; private set; }
    public Guid? CountryId { get; private set; }
    public string? Zip { get; private set; }
    public string? AptSuite { get; private set; }
    public string? Coordinates { get; private set; }
    public string? CommonPlaceName { get; private set; }
    public string? Comments { get; private set; }
    public string? Address { get; private set; }
    public bool IsLocked { get; private set; }
    public Guid? LockedByUserId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private LocationReadModel() { } // EF Core materialization

    public static LocationReadModel Create(
        Guid id,
        long recordNumber,
        Guid jurisdictionId,
        string? streetNumber,
        Guid? preDirectionId,
        string streetAddress,
        Guid? streetTypeId,
        Guid? postDirectionId,
        string city,
        Guid? stateId,
        Guid? countryId,
        string? zip,
        string? aptSuite,
        string? coordinates,
        string? commonPlaceName,
        string? comments,
        string? address,
        DateTime createdAtUtc,
        Guid createdBy)
    {
        return new LocationReadModel
        {
            Id             = id,
            RecordNumber   = recordNumber,
            JurisdictionId = jurisdictionId,
            StreetNumber   = streetNumber,
            PreDirectionId = preDirectionId,
            StreetAddress  = streetAddress,
            StreetTypeId   = streetTypeId,
            PostDirectionId = postDirectionId,
            City           = city,
            StateId        = stateId,
            CountryId      = countryId,
            Zip            = zip,
            AptSuite       = aptSuite,
            Coordinates    = coordinates,
            CommonPlaceName = commonPlaceName,
            Comments       = comments,
            Address        = address,
            IsLocked       = false,
            CreatedBy      = createdBy,
            CreatedAtUtc   = createdAtUtc,
            UpdatedAtUtc   = createdAtUtc,
        };
    }

    public void ApplyDetailsChanged(
        string? streetNumber,
        Guid? preDirectionId,
        string streetAddress,
        Guid? streetTypeId,
        Guid? postDirectionId,
        string city,
        Guid? stateId,
        Guid? countryId,
        string? zip,
        string? aptSuite,
        string? coordinates,
        string? commonPlaceName,
        string? comments,
        string? address)
    {
        StreetNumber    = streetNumber;
        PreDirectionId  = preDirectionId;
        StreetAddress   = streetAddress;
        StreetTypeId    = streetTypeId;
        PostDirectionId = postDirectionId;
        City            = city;
        StateId         = stateId;
        CountryId       = countryId;
        Zip             = zip;
        AptSuite        = aptSuite;
        Coordinates     = coordinates;
        CommonPlaceName = commonPlaceName;
        Comments        = comments;
        Address         = address;
        UpdatedAtUtc    = DateTime.UtcNow;
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

    public void ApplyModifiedAudit(Guid? modifiedBy, DateTime? modifiedAt)
    {
        ModifiedBy   = modifiedBy;
        UpdatedAtUtc = modifiedAt ?? UpdatedAtUtc;
    }

    public LocationDto ToDto() => new(
        Id, RecordNumber, JurisdictionId,
        StreetNumber, PreDirectionId, StreetAddress, StreetTypeId, PostDirectionId,
        City, StateId, CountryId, Zip, AptSuite, Coordinates, CommonPlaceName, Comments,
        Address, IsLocked, LockedByUserId, CreatedBy, ModifiedBy, CreatedAtUtc, UpdatedAtUtc);
}
