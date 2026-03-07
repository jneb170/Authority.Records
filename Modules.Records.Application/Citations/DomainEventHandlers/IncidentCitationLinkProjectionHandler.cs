using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Citations.DomainEventHandlers;

public sealed class IncidentCitationLinkProjectionHandler :
    INotificationHandler<CitationLinkedToIncidentDomainEvent>,
    INotificationHandler<CitationUnlinkedFromIncidentDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public IncidentCitationLinkProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CitationLinkedToIncidentDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.IncidentCitationLinkReadModels
            .AnyAsync(l => l.Id == notification.LinkId, cancellationToken);
        if (exists)
            return;

        var incident = await _dbContext.IncidentReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

        var readModel = IncidentCitationLinkReadModel.Create(
            id: notification.LinkId,
            jurisdictionId: notification.JurisdictionId,
            incidentId: notification.IncidentId,
            incidentRecordNumber: incident?.RecordNumber ?? 0,
            incidentNum: incident?.IncidentNum ?? string.Empty,
            citationId: notification.CitationId,
            linkedAtUtc: notification.OccurredOnUtc);

        _dbContext.IncidentCitationLinkReadModels.Add(readModel);

        var incidentReadModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);
        incidentReadModel?.IncrementCitationCount();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(CitationUnlinkedFromIncidentDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.IncidentCitationLinkReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.LinkId, cancellationToken);

        if (readModel is null)
            return;

        _dbContext.IncidentCitationLinkReadModels.Remove(readModel);

        var incidentReadModel = await _dbContext.IncidentReadModels
            .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);
        incidentReadModel?.DecrementCitationCount();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
