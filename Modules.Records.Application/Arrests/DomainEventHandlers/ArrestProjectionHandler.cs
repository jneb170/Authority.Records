using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.DomainEventHandlers;

public sealed class ArrestProjectionHandler :
    INotificationHandler<ArrestCreatedDomainEvent>,
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
        // Idempotency: skip if already projected (handles outbox retry / double-dispatch)
        var exists = await _dbContext.ArrestReadModels
            .AnyAsync(a => a.Id == notification.ArrestId, cancellationToken);
        if (exists)
            return;

        var readModel = ArrestReadModel.Create(
            id: notification.ArrestId,
            jurisdictionId: notification.JurisdictionId,
            agencyId: notification.AgencyId,
            incidentId: notification.IncidentId,
            suspectName: notification.SuspectName,
            arrestedAt: notification.ArrestedAt,
            createdAtUtc: notification.OccurredOnUtc);

        _dbContext.ArrestReadModels.Add(readModel);

        var incidentReadModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        incidentReadModel?.IncrementArrestCount();

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
