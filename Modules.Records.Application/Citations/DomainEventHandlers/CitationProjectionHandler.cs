using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.DomainEventHandlers;

public sealed class CitationProjectionHandler :
    INotificationHandler<CitationCreatedDomainEvent>,
    INotificationHandler<CitationDetailsUpdatedDomainEvent>,
    INotificationHandler<CitationIssuedDomainEvent>,
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
        var exists = await _dbContext.CitationReadModels
            .AnyAsync(c => c.Id == notification.CitationId, cancellationToken);
        if (exists)
            return;

        var citation = await _dbContext.Citations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == notification.CitationId, cancellationToken);

        var readModel = CitationReadModel.Create(
            id: notification.CitationId,
            recordNumber: citation?.RecordNumber ?? 0,
            jurisdictionId: notification.JurisdictionId,
            agencyId: citation?.AgencyId ?? Guid.Empty,
            description: notification.Description,
            issueDate: notification.IssueDate,
            createdAtUtc: notification.OccurredOnUtc,
            createdBy: citation?.CreatedBy ?? Guid.Empty,
            citationNum: notification.CitationNum);

        _dbContext.CitationReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(CitationDetailsUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.CitationReadModels
            .FirstOrDefaultAsync(c => c.Id == notification.CitationId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyDetailsChanged(notification.Description, notification.IssueDate, notification.CourtId, notification.CitationNum);
        readModel.ApplyLocationChanged(notification.LocationId);
        readModel.ApplyModifiedAudit(notification.ModifiedBy, notification.OccurredOnUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(CitationIssuedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.CitationReadModels
            .FirstOrDefaultAsync(c => c.Id == notification.CitationId, cancellationToken);

        if (readModel is null)
            return;

        readModel.ApplyIssued();
        readModel.ApplyModifiedAudit(notification.IssuedByUserId, notification.OccurredOnUtc);
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
