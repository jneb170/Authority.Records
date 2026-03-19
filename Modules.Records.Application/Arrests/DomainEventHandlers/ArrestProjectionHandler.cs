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
