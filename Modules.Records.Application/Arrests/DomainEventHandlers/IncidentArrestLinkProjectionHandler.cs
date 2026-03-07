using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Arrests.DomainEventHandlers;

public sealed class IncidentArrestLinkProjectionHandler :
    INotificationHandler<ArrestLinkedToIncidentDomainEvent>,
    INotificationHandler<ArrestUnlinkedFromIncidentDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public IncidentArrestLinkProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ArrestLinkedToIncidentDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.IncidentArrestLinkReadModels
            .AnyAsync(l => l.Id == notification.LinkId, cancellationToken);
        if (exists)
            return;

        var incident = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        var readModel = IncidentArrestLinkReadModel.Create(
            id: notification.LinkId,
            jurisdictionId: notification.JurisdictionId,
            incidentId: notification.IncidentId,
            incidentRecordNumber: incident?.RecordNumber ?? 0,
            incidentNum: incident?.IncidentNum ?? string.Empty,
            arrestId: notification.ArrestId,
            linkedAtUtc: notification.OccurredOnUtc);

        _dbContext.IncidentArrestLinkReadModels.Add(readModel);

        var incidentReadModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);
        incidentReadModel?.IncrementArrestCount();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ArrestUnlinkedFromIncidentDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentArrestLinkReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.LinkId, cancellationToken);

        if (readModel is null)
            return;

        _dbContext.IncidentArrestLinkReadModels.Remove(readModel);

        var incidentReadModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);
        incidentReadModel?.DecrementArrestCount();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
