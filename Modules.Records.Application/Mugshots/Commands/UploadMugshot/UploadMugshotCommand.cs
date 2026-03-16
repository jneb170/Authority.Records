using MediatR;

namespace Modules.Records.Application.Mugshots.Commands.UploadMugshot;

public sealed record UploadMugshotCommand(
    string OwnerType,
    Guid OwnerId,
    string FileName,
    string ContentType,
    byte[] Content,
    bool MakePrimary = false,
    DateTime? CapturedAtUtc = null) : IRequest<Guid>;
