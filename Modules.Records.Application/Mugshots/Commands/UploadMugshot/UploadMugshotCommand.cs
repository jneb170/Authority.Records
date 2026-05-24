using MediatR;
using Modules.Records.Application.Common;

namespace Modules.Records.Application.Mugshots.Commands.UploadMugshot;

// Counts against the demo creation-rate cap, but exempt from the demo per-write
// size cap: it carries image bytes (its validator enforces a 5 MB / image-type
// limit), which the text-calibrated 64 KB cap would otherwise reject.
public sealed record UploadMugshotCommand(
    string OwnerType,
    Guid OwnerId,
    string FileName,
    string ContentType,
    byte[] Content,
    bool MakePrimary = false,
    DateTime? CapturedAtUtc = null) : IRequest<Guid>, IRateLimitedCommand, IExemptFromDemoWriteSizeLimit;
