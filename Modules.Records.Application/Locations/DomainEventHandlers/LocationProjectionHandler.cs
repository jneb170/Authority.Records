using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Locations.DomainEventHandlers;

public sealed class LocationProjectionHandler :
    INotificationHandler<LocationCreatedDomainEvent>,
    INotificationHandler<LocationDetailsUpdatedDomainEvent>,
    INotificationHandler<LocationSoftDeletedDomainEvent>,
    INotificationHandler<LocationRestoredDomainEvent>,
    INotificationHandler<LockAcquiredDomainEvent<Location>>,
    INotificationHandler<LockReleasedDomainEvent<Location>>
{
    private readonly IApplicationDbContext _dbContext;

    public LocationProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(LocationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.LocationReadModels
            .AnyAsync(l => l.Id == notification.LocationId, cancellationToken);
        if (exists) return;

        var location = await _dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == notification.LocationId, cancellationToken);

        if (location is null) return;

        var readModel = LocationReadModel.Create(
            id:             notification.LocationId,
            recordNumber:   location.RecordNumber,
            jurisdictionId: notification.JurisdictionId,
            streetNumber:   location.StreetNumber,
            preDirectionId: location.PreDirectionId,
            streetAddress:  notification.StreetAddress,
            streetTypeId:   location.StreetTypeId,
            postDirectionId: location.PostDirectionId,
            city:           notification.City,
            stateId:        notification.StateId,
            countryId:      location.CountryId,
            zip:            location.Zip,
            aptSuite:       location.AptSuite,
            coordinates:    location.Coordinates,
            commonPlaceName: notification.CommonPlaceName,
            comments:       location.Comments,
            address:        location.Address,
            createdAtUtc:   notification.OccurredOnUtc,
            createdBy:      location.CreatedBy);

        _dbContext.LocationReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LocationDetailsUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.LocationReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.LocationId, cancellationToken);
        if (readModel is null) return;

        var location = await _dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == notification.LocationId, cancellationToken);

        readModel.ApplyDetailsChanged(
            notification.StreetNumber,
            notification.PreDirectionId,
            notification.StreetAddress,
            notification.StreetTypeId,
            notification.PostDirectionId,
            notification.City,
            notification.StateId,
            notification.CountryId,
            notification.Zip,
            notification.AptSuite,
            notification.Coordinates,
            notification.CommonPlaceName,
            notification.Comments,
            notification.Address);

        readModel.ApplyModifiedAudit(location?.ModifiedBy, location?.ModifiedAt);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LocationSoftDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.LocationReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.LocationId, cancellationToken);
        if (readModel is null) return;

        _dbContext.LocationReadModels.Remove(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LocationRestoredDomainEvent notification, CancellationToken cancellationToken)
    {
        // SoftDelete removed the read-model row (LocationReadModel has no IsDeleted flag), so restore
        // must rebuild it from the aggregate or the location stays invisible in the MLI despite being live.
        var exists = await _dbContext.LocationReadModels
            .AnyAsync(l => l.Id == notification.LocationId, cancellationToken);
        if (exists) return;

        var location = await _dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == notification.LocationId, cancellationToken);
        if (location is null) return;

        var readModel = LocationReadModel.Create(
            id:             location.Id,
            recordNumber:   location.RecordNumber,
            jurisdictionId: location.JurisdictionId,
            streetNumber:   location.StreetNumber,
            preDirectionId: location.PreDirectionId,
            streetAddress:  location.StreetAddress,
            streetTypeId:   location.StreetTypeId,
            postDirectionId: location.PostDirectionId,
            city:           location.City,
            stateId:        location.StateId,
            countryId:      location.CountryId,
            zip:            location.Zip,
            aptSuite:       location.AptSuite,
            coordinates:    location.Coordinates,
            commonPlaceName: location.CommonPlaceName,
            comments:       location.Comments,
            address:        location.Address,
            createdAtUtc:   location.CreatedAt,
            createdBy:      location.CreatedBy);

        if (location.IsLocked && location.LockedByUserId is Guid lockedBy)
            readModel.ApplyLockAcquired(lockedBy);
        readModel.ApplyModifiedAudit(location.ModifiedBy, location.ModifiedAt, location.CreatedAt);

        _dbContext.LocationReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockAcquiredDomainEvent<Location> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.LocationReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.AggregateId, cancellationToken);
        if (readModel is null) return;

        readModel.ApplyLockAcquired(notification.UserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockReleasedDomainEvent<Location> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.LocationReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.AggregateId, cancellationToken);
        if (readModel is null) return;

        readModel.ApplyLockReleased();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
