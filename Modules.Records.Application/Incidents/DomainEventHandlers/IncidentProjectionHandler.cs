using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Incidents.DomainEventHandlers;

public sealed class IncidentProjectionHandler :
    INotificationHandler<IncidentCreatedDomainEvent>,
    INotificationHandler<IncidentOpenedDomainEvent>,
    INotificationHandler<IncidentClosedDomainEvent>,
    INotificationHandler<IncidentArchivedDomainEvent>,
    INotificationHandler<IncidentSoftDeletedDomainEvent>,
    INotificationHandler<IncidentRestoredDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public IncidentProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(IncidentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        if (incident is null)
            return;

        var readModel = IncidentReadModel.Create(
            id: incident.Id,
            jurisdictionId: incident.JurisdictionId,
            agencyId: incident.AgencyId,
            description: incident.Description,
            status: incident.Status,
            createdAtUtc: notification.OccurredOnUtc);

        _dbContext.IncidentReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(IncidentOpenedDomainEvent notification, CancellationToken cancellationToken)
        => await ApplyStatusChange(notification.IncidentId, "Open", cancellationToken);

    public async Task Handle(IncidentClosedDomainEvent notification, CancellationToken cancellationToken)
        => await ApplyStatusChange(notification.IncidentId, "Closed", cancellationToken);

    public async Task Handle(IncidentArchivedDomainEvent notification, CancellationToken cancellationToken)
        => await ApplyStatusChange(notification.IncidentId, "Archived", cancellationToken);

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
}
