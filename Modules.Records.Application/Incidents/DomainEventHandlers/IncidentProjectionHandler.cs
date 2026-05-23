using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Incidents.DomainEventHandlers;

public sealed class IncidentProjectionHandler :
    INotificationHandler<IncidentCreatedDomainEvent>,
    INotificationHandler<LifecycleStatusChangedDomainEvent<Incident>>,
    INotificationHandler<IncidentSoftDeletedDomainEvent>,
    INotificationHandler<IncidentRestoredDomainEvent>,
    INotificationHandler<IncidentDetailsUpdatedDomainEvent>,
    INotificationHandler<LockAcquiredDomainEvent<Incident>>,
    INotificationHandler<LockReleasedDomainEvent<Incident>>
{
    private readonly IApplicationDbContext _dbContext;

    public IncidentProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(IncidentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Idempotency: skip if already projected (handles outbox retry / double-dispatch)
        var exists = await _dbContext.IncidentReadModels
            .AnyAsync(i => i.Id == notification.IncidentId, cancellationToken);
        if (exists)
            return;

        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        if (incident is null)
            return;

        var readModel = IncidentReadModel.Create(
            id: incident.Id,
            recordNumber: incident.RecordNumber,
            jurisdictionId: incident.JurisdictionId,
            agencyId: incident.AgencyId,
            details: incident.Details,
            status: incident.Status,
            createdAtUtc: notification.OccurredOnUtc,
            createdBy: incident.CreatedBy);

        _dbContext.IncidentReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(IncidentDetailsUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyDetailsChanged(notification.Details);
        readModel.ApplyOccurredOnChanged(notification.OccurredOn);
        readModel.ApplyLocationChanged(notification.LocationId);
        readModel.ApplyModifiedAudit(notification.ModifiedBy, notification.OccurredOnUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LifecycleStatusChangedDomainEvent<Incident> notification, CancellationToken cancellationToken)
        => await ApplyStatusChange(notification.AggregateId, notification.NewStatus.ToString(), cancellationToken);

    public async Task Handle(IncidentSoftDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(IncidentRestoredDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyRestored();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyStatusChange(Guid incidentId, string status, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == incidentId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyStatusChange(status);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockAcquiredDomainEvent<Incident> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.AggregateId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyLockAcquired(notification.UserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockReleasedDomainEvent<Incident> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.AggregateId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyLockReleased();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
