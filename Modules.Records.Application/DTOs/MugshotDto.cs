namespace Modules.Records.Application.DTOs;

public sealed record MugshotDto(
    Guid Id,
    string OwnerType,
    Guid OwnerId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string PublicUrl,
    DateTime CapturedAtUtc,
    DateTime LinkedAtUtc,
    bool IsPrimary,
    int DisplayOrder);
