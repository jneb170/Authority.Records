using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Mugshots.Commands.UploadMugshot;

public sealed class UploadMugshotHandler : IRequestHandler<UploadMugshotCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMugshotStorageService _storageService;

    public UploadMugshotHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IMugshotStorageService storageService)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _storageService = storageService;
    }

    public async Task<Guid> Handle(UploadMugshotCommand request, CancellationToken cancellationToken)
    {
        await EnsureOwnerExistsAsync(request.OwnerType, request.OwnerId, cancellationToken);

        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var saveResult = await _storageService.SaveAsync(
            jurisdictionId,
            request.Content,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var mugshot = new Mugshot(
            jurisdictionId,
            agencyId,
            request.FileName,
            request.ContentType,
            saveResult.FileSizeBytes,
            saveResult.StoragePath,
            saveResult.PublicUrl,
            request.CapturedAtUtc ?? DateTime.UtcNow);

        _dbContext.Mugshots.Add(mugshot);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var existingLinks = await _dbContext.MugshotLinks
            .Where(l => l.OwnerType == request.OwnerType && l.OwnerId == request.OwnerId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(cancellationToken);

        var shouldBePrimary = request.MakePrimary || existingLinks.Count == 0 || existingLinks.All(l => !l.IsPrimary);

        if (shouldBePrimary)
        {
            foreach (var existingLink in existingLinks.Where(l => l.IsPrimary))
            {
                existingLink.SetPrimary(false);
            }
        }

        var link = new MugshotLink(
            jurisdictionId,
            mugshot.Id,
            request.OwnerType,
            request.OwnerId,
            _tenantProvider.GetUserId(),
            shouldBePrimary,
            existingLinks.Count);

        _dbContext.MugshotLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return mugshot.Id;
    }

    private async Task EnsureOwnerExistsAsync(string ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        var exists = ownerType switch
        {
            MugshotOwnerTypes.Name => await _dbContext.Names.AnyAsync(n => n.Id == ownerId, cancellationToken),
            MugshotOwnerTypes.Arrest => await _dbContext.Arrests.AnyAsync(a => a.Id == ownerId, cancellationToken),
            _ => false
        };

        if (!exists)
        {
            throw new InvalidOperationException("The selected record could not be found for mugshot linking.");
        }
    }
}
