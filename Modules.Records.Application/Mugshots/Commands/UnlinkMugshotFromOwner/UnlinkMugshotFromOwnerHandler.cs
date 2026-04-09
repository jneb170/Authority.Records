using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Mugshots.Commands.UnlinkMugshotFromOwner;

public sealed class UnlinkMugshotFromOwnerHandler : IRequestHandler<UnlinkMugshotFromOwnerCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMugshotStorageService _storageService;

    public UnlinkMugshotFromOwnerHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IMugshotStorageService storageService)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _storageService = storageService;
    }

    public async Task Handle(UnlinkMugshotFromOwnerCommand request, CancellationToken cancellationToken)
    {
        var userId = _tenantProvider.GetUserId();

        var link = await _dbContext.MugshotLinks
            .FirstOrDefaultAsync(l =>
                l.MugshotId == request.MugshotId &&
                l.OwnerType == request.OwnerType &&
                l.OwnerId == request.OwnerId,
                cancellationToken)
            ?? throw new InvalidOperationException("Mugshot link not found.");

        var wasPrimary = link.IsPrimary;
        link.Unlink(userId);
        _dbContext.MugshotLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (wasPrimary)
        {
            var replacement = await _dbContext.MugshotLinks
                .Where(l => l.OwnerType == request.OwnerType && l.OwnerId == request.OwnerId)
                .OrderBy(l => l.DisplayOrder)
                .ThenBy(l => l.LinkedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (replacement is not null)
            {
                replacement.SetPrimary(true);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var hasRemainingLinks = await _dbContext.MugshotLinks
            .AnyAsync(l => l.MugshotId == request.MugshotId, cancellationToken);

        if (hasRemainingLinks)
        {
            return;
        }

        var mugshot = await _dbContext.Mugshots
            .FirstOrDefaultAsync(m => m.Id == request.MugshotId, cancellationToken);

        if (mugshot is null)
        {
            return;
        }

        mugshot.SoftDelete(userId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _storageService.DeleteAsync(mugshot.StoragePath, cancellationToken);
    }
}
