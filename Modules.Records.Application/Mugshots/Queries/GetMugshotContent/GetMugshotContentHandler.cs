using Modules.Records.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Mugshots.Queries.GetMugshotContent;

public sealed class GetMugshotContentHandler : IRequestHandler<GetMugshotContentQuery, MugshotContent?>
{
    // Only these types are accepted at upload (UploadMugshotValidator). Anything else in
    // the database is served as an opaque download rather than trusted as-is.
    private static readonly HashSet<string> ImageContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private readonly IApplicationDbContext _dbContext;
    private readonly IMugshotStorageService _storageService;

    public GetMugshotContentHandler(IApplicationDbContext dbContext, IMugshotStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<MugshotContent?> Handle(GetMugshotContentQuery request, CancellationToken cancellationToken)
    {
        // Tenant isolation and soft delete come from the global query filters: a mugshot
        // in another jurisdiction is indistinguishable from one that does not exist.
        var mugshot = await _dbContext.Mugshots
            .AsNoTracking()
            .Where(m => m.Id == request.MugshotId)
            .Select(m => new { m.StoragePath, m.ContentType })
            .FirstOrDefaultAsync(cancellationToken);

        if (mugshot is null)
            return null;

        // An arrest mugshot belongs to the arrest's agency; a name mugshot is shared (#118).
        if (!await AgencyRecordAccess.CanAccessMugshotAsync(_dbContext, request.MugshotId, cancellationToken))
            return null;

        var content = await _storageService.OpenReadAsync(mugshot.StoragePath, cancellationToken);
        if (content is null)
            return null;

        var contentType = ImageContentTypes.Contains(mugshot.ContentType)
            ? mugshot.ContentType
            : "application/octet-stream";

        return new MugshotContent(content, contentType);
    }
}
