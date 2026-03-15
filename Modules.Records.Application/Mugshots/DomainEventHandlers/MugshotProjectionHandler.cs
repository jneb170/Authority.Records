using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Mugshots.DomainEventHandlers;

public sealed class MugshotProjectionHandler :
    INotificationHandler<MugshotCreatedDomainEvent>,
    INotificationHandler<MugshotSoftDeletedDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public MugshotProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(MugshotCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.MugshotReadModels
            .AnyAsync(m => m.Id == notification.MugshotId, cancellationToken);

        if (exists)
        {
            return;
        }

        var mugshot = await _dbContext.Mugshots
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == notification.MugshotId, cancellationToken);

        if (mugshot is null)
        {
            return;
        }

        _dbContext.MugshotReadModels.Add(MugshotReadModel.Create(
            mugshot.Id,
            mugshot.JurisdictionId,
            mugshot.AgencyId,
            mugshot.FileName,
            mugshot.ContentType,
            mugshot.FileSizeBytes,
            mugshot.StoragePath,
            mugshot.PublicUrl,
            mugshot.CapturedAtUtc,
            mugshot.CreatedBy,
            mugshot.CreatedAt));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(MugshotSoftDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = await _dbContext.MugshotReadModels
            .FirstOrDefaultAsync(m => m.Id == notification.MugshotId, cancellationToken);

        if (readModel is null)
        {
            return;
        }

        _dbContext.MugshotReadModels.Remove(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
