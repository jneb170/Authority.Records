using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.DomainEventHandlers;

public sealed class CitationProjectionHandler :
    INotificationHandler<CitationCreatedDomainEvent>,
    INotificationHandler<LockAcquiredDomainEvent<Citation>>,
    INotificationHandler<LockReleasedDomainEvent<Citation>>
{
    private readonly IApplicationDbContext _dbContext;

    public CitationProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CitationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Idempotency: skip if already projected (handles outbox retry / double-dispatch)
        var exists = await _dbContext.CitationReadModels
            .AnyAsync(c => c.Id == notification.CitationId, cancellationToken);
        if (exists)
            return;

        var readModel = CitationReadModel.Create(
            id: notification.CitationId,
            jurisdictionId: notification.JurisdictionId,
            agencyId: notification.AgencyId,
            incidentId: notification.IncidentId,
            description: notification.Description,
            issueDate: notification.IssueDate,
            createdAtUtc: notification.OccurredOnUtc);

        _dbContext.CitationReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockAcquiredDomainEvent<Citation> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.CitationReadModels
            .FirstOrDefaultAsync(c => c.Id == notification.AggregateId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyLockAcquired(notification.UserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockReleasedDomainEvent<Citation> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.CitationReadModels
            .FirstOrDefaultAsync(c => c.Id == notification.AggregateId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyLockReleased();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
