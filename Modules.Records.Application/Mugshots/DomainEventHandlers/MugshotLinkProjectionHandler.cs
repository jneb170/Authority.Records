using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Mugshots.DomainEventHandlers;

public sealed class MugshotLinkProjectionHandler :
    INotificationHandler<MugshotLinkedToOwnerDomainEvent>,
    INotificationHandler<MugshotOwnerPrimaryChangedDomainEvent>,
    INotificationHandler<MugshotUnlinkedFromOwnerDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public MugshotLinkProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(MugshotLinkedToOwnerDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.MugshotLinkReadModels
            .AnyAsync(l => l.Id == notification.LinkId, cancellationToken);

        if (!exists)
        {
            _dbContext.MugshotLinkReadModels.Add(MugshotLinkReadModel.Create(
                notification.LinkId,
                notification.JurisdictionId,
                notification.MugshotId,
                notification.OwnerType,
                notification.OwnerId,
                notification.IsPrimary,
                notification.DisplayOrder,
                notification.OccurredOnUtc));

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await UpdateOwnerPreviewAsync(notification.JurisdictionId, notification.OwnerType, notification.OwnerId, cancellationToken);
    }

    public async Task Handle(MugshotOwnerPrimaryChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.MugshotLinkReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.LinkId, cancellationToken);

        if (readModel is null)
        {
            return;
        }

        readModel.ApplyPrimaryChanged(notification.IsPrimary);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await UpdateOwnerPreviewAsync(notification.JurisdictionId, notification.OwnerType, notification.OwnerId, cancellationToken);
    }

    public async Task Handle(MugshotUnlinkedFromOwnerDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.MugshotLinkReadModels
            .FirstOrDefaultAsync(l => l.Id == notification.LinkId, cancellationToken);

        if (readModel is not null)
        {
            _dbContext.MugshotLinkReadModels.Remove(readModel);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await UpdateOwnerPreviewAsync(notification.JurisdictionId, notification.OwnerType, notification.OwnerId, cancellationToken);
    }

    private async Task UpdateOwnerPreviewAsync(Guid jurisdictionId, string ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        var primaryLink = await _dbContext.MugshotLinkReadModels
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jurisdictionId && l.OwnerType == ownerType && l.OwnerId == ownerId)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.DisplayOrder)
            .ThenBy(l => l.LinkedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        string? primaryUrl = null;

        if (primaryLink is not null)
        {
            primaryUrl = await _dbContext.MugshotReadModels
                .AsNoTracking()
                .Where(m => m.Id == primaryLink.MugshotId && m.JurisdictionId == jurisdictionId)
                .Select(m => m.PublicUrl)
                .FirstOrDefaultAsync(cancellationToken);
        }

        switch (ownerType)
        {
            case MugshotOwnerTypes.Name:
                var name = await _dbContext.NameReadModels
                    .FirstOrDefaultAsync(n => n.Id == ownerId, cancellationToken);
                if (name is null)
                {
                    return;
                }

                name.ApplyPrimaryMugshot(primaryUrl);
                await _dbContext.SaveChangesAsync(cancellationToken);
                break;

            case MugshotOwnerTypes.Arrest:
                var arrest = await _dbContext.ArrestReadModels
                    .FirstOrDefaultAsync(a => a.Id == ownerId, cancellationToken);
                if (arrest is null)
                {
                    return;
                }

                arrest.ApplyPrimaryMugshot(primaryUrl);
                await _dbContext.SaveChangesAsync(cancellationToken);
                break;
        }
    }
}
