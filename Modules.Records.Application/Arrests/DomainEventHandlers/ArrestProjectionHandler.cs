using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.DomainEventHandlers;

public sealed class ArrestProjectionHandler :
    INotificationHandler<ArrestCreatedDomainEvent>,
    INotificationHandler<ArrestDetailsUpdatedDomainEvent>,
    INotificationHandler<LifecycleStatusChangedDomainEvent<Arrest>>,
    INotificationHandler<ArrestSoftDeletedDomainEvent>,
    INotificationHandler<ArrestRestoredDomainEvent>,
    INotificationHandler<LockAcquiredDomainEvent<Arrest>>,
    INotificationHandler<LockReleasedDomainEvent<Arrest>>
{
    private readonly IApplicationDbContext _dbContext;

    public ArrestProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ArrestCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ArrestReadModels
            .AnyAsync(a => a.Id == notification.ArrestId, cancellationToken);
        if (exists)
            return;

        var arrest = await _dbContext.Arrests
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == notification.ArrestId, cancellationToken);

        var readModel = ArrestReadModel.Create(
            id: notification.ArrestId,
            recordNumber: arrest?.RecordNumber ?? 0,
            jurisdictionId: notification.JurisdictionId,
            agencyId: arrest?.AgencyId ?? Guid.Empty,
            nameId: notification.NameId,
            arrestedAt: notification.ArrestedAt,
            createdAtUtc: notification.OccurredOnUtc,
            createdBy: arrest?.CreatedBy ?? Guid.Empty,
            arrestNum: notification.ArrestNum,
            primaryIncidentId: notification.PrimaryIncidentId);

        readModel.ApplyLocationChanged(arrest?.LocationId);
        readModel.UpdatedAtUtc = notification.OccurredOnUtc;

        _dbContext.ArrestReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ArrestDetailsUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.ArrestReadModels
            .FirstOrDefaultAsync(a => a.Id == notification.ArrestId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyDetailsChanged(notification.NameId, notification.ArrestedAt, notification.ArrestTypeId, notification.ArrestNum, notification.PrimaryIncidentId);
        readModel.ApplyLocationChanged(notification.LocationId);
        readModel.ApplyModifiedAudit(notification.ModifiedBy, notification.OccurredOnUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LifecycleStatusChangedDomainEvent<Arrest> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.ArrestReadModels
            .FirstOrDefaultAsync(a => a.Id == notification.AggregateId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyStatusChange(notification.NewStatus.ToString());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ArrestSoftDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        // ArrestReadModel carries no IsDeleted flag and the arrest read queries don't filter one,
        // so a soft-deleted arrest must be removed from the projection (mirrors Citation/Mugshot/
        // Narrative) — otherwise it keeps appearing in lists and stays fetchable by record number.
        var readModel = await _dbContext.ArrestReadModels
            .FirstOrDefaultAsync(a => a.Id == notification.ArrestId, cancellationToken);

        if (readModel is null)
            return;

        _dbContext.ArrestReadModels.Remove(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ArrestRestoredDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ArrestReadModels
            .AnyAsync(a => a.Id == notification.ArrestId, cancellationToken);
        if (exists)
            return;

        // Rebuild the projection from the aggregate's current state. By the time this runs the
        // aggregate's IsDeleted flag is already cleared, so the global query filter includes it.
        var arrest = await _dbContext.Arrests
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == notification.ArrestId, cancellationToken);

        if (arrest is null)
            return;

        var readModel = ArrestReadModel.Create(
            id: arrest.Id,
            recordNumber: arrest.RecordNumber,
            jurisdictionId: arrest.JurisdictionId,
            agencyId: arrest.AgencyId,
            nameId: arrest.NameId,
            arrestedAt: arrest.ArrestedAt,
            createdAtUtc: arrest.CreatedAt,
            createdBy: arrest.CreatedBy,
            arrestNum: arrest.ArrestNum,
            primaryIncidentId: arrest.PrimaryIncidentId);

        readModel.ApplyDetailsChanged(arrest.NameId, arrest.ArrestedAt, arrest.ArrestTypeId, arrest.ArrestNum, arrest.PrimaryIncidentId);
        readModel.ApplyLocationChanged(arrest.LocationId);
        readModel.ApplyStatusChange(arrest.Status.ToString());
        if (arrest.IsLocked && arrest.LockedByUserId is Guid lockedBy)
            readModel.ApplyLockAcquired(lockedBy);
        readModel.ApplyModifiedAudit(arrest.ModifiedBy, arrest.ModifiedAt, arrest.CreatedAt);

        _dbContext.ArrestReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockAcquiredDomainEvent<Arrest> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.ArrestReadModels
            .FirstOrDefaultAsync(a => a.Id == notification.AggregateId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyLockAcquired(notification.UserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockReleasedDomainEvent<Arrest> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.ArrestReadModels
            .FirstOrDefaultAsync(a => a.Id == notification.AggregateId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyLockReleased();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
