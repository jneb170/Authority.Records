using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Arrests.DomainEventHandlers;

public sealed class ArrestProjectionHandler :
    INotificationHandler<ArrestCreatedDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public ArrestProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ArrestCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
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
}
