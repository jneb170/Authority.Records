using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Narratives.DomainEventHandlers;

/// <summary>
/// Projects Narrative + NarrativeLink domain events into their read models. Handlers are
/// idempotent (rebuild/replay paths exist). Content is re-read from the aggregate rather than
/// carried on the event, so large narrative bodies don't bloat the outbox/audit payload.
/// </summary>
public sealed class NarrativeProjectionHandler :
    INotificationHandler<NarrativeCreatedDomainEvent>,
    INotificationHandler<NarrativeContentUpdatedDomainEvent>,
    INotificationHandler<NarrativeSoftDeletedDomainEvent>,
    INotificationHandler<NarrativeRestoredDomainEvent>,
    INotificationHandler<NarrativeLinkedToOwnerDomainEvent>,
    INotificationHandler<NarrativeUnlinkedFromOwnerDomainEvent>,
    INotificationHandler<LockAcquiredDomainEvent<Narrative>>,
    INotificationHandler<LockReleasedDomainEvent<Narrative>>
{
    private readonly IApplicationDbContext _dbContext;

    public NarrativeProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(NarrativeCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.NarrativeReadModels
            .AnyAsync(n => n.Id == notification.NarrativeId, cancellationToken);
        if (exists) return;

        var narrative = await LoadAggregate(notification.NarrativeId, cancellationToken);
        if (narrative is null) return;

        _dbContext.NarrativeReadModels.Add(NarrativeReadModel.Create(
            id:             narrative.Id,
            recordNumber:   narrative.RecordNumber,
            jurisdictionId: narrative.JurisdictionId,
            title:          narrative.Title,
            content:        narrative.Content,
            createdAtUtc:   notification.OccurredOnUtc,
            createdBy:      narrative.CreatedBy));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NarrativeContentUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NarrativeReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.NarrativeId, cancellationToken);
        if (readModel is null) return;

        var narrative = await LoadAggregate(notification.NarrativeId, cancellationToken);

        readModel.ApplyContentChanged(notification.Title, narrative?.Content ?? string.Empty);
        readModel.ApplyModifiedAudit(narrative?.ModifiedBy, narrative?.ModifiedAt);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NarrativeSoftDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NarrativeReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.NarrativeId, cancellationToken);
        if (readModel is not null)
            _dbContext.NarrativeReadModels.Remove(readModel);

        var links = await _dbContext.NarrativeLinkReadModels
            .Where(l => l.NarrativeId == notification.NarrativeId)
            .ToListAsync(cancellationToken);
        _dbContext.NarrativeLinkReadModels.RemoveRange(links);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NarrativeRestoredDomainEvent notification, CancellationToken cancellationToken)
    {
        var narrative = await LoadAggregate(notification.NarrativeId, cancellationToken);
        if (narrative is null) return;

        var exists = await _dbContext.NarrativeReadModels
            .AnyAsync(n => n.Id == narrative.Id, cancellationToken);
        if (!exists)
        {
            _dbContext.NarrativeReadModels.Add(NarrativeReadModel.Create(
                narrative.Id, narrative.RecordNumber, narrative.JurisdictionId,
                narrative.Title, narrative.Content, narrative.CreatedAt, narrative.CreatedBy));
        }

        // Rebuild the link read models from the (never-deleted) link aggregates.
        var links = await _dbContext.NarrativeLinks
            .AsNoTracking()
            .Where(l => l.NarrativeId == narrative.Id)
            .ToListAsync(cancellationToken);

        foreach (var link in links)
        {
            var linkExists = await _dbContext.NarrativeLinkReadModels
                .AnyAsync(l => l.Id == link.Id, cancellationToken);
            if (!linkExists)
                _dbContext.NarrativeLinkReadModels.Add(NarrativeLinkReadModel.Create(
                    link.Id, link.JurisdictionId, link.NarrativeId,
                    link.OwnerType, link.OwnerId, link.DisplayOrder, link.LinkedAtUtc));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NarrativeLinkedToOwnerDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.NarrativeLinkReadModels
            .AnyAsync(l => l.Id == notification.LinkId, cancellationToken);
        if (exists) return;

        _dbContext.NarrativeLinkReadModels.Add(NarrativeLinkReadModel.Create(
            notification.LinkId, notification.JurisdictionId, notification.NarrativeId,
            notification.OwnerType, notification.OwnerId, notification.DisplayOrder, notification.OccurredOnUtc));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(NarrativeUnlinkedFromOwnerDomainEvent notification, CancellationToken cancellationToken)
    {
        var link = await _dbContext.NarrativeLinkReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.LinkId, cancellationToken);
        if (link is null) return;

        _dbContext.NarrativeLinkReadModels.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockAcquiredDomainEvent<Narrative> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NarrativeReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.AggregateId, cancellationToken);
        if (readModel is null) return;

        readModel.ApplyLockAcquired(notification.UserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(LockReleasedDomainEvent<Narrative> notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.NarrativeReadModels
            .FirstOrDefaultAsync(n => n.Id == notification.AggregateId, cancellationToken);
        if (readModel is null) return;

        readModel.ApplyLockReleased();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<Narrative?> LoadAggregate(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Narratives
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
}
