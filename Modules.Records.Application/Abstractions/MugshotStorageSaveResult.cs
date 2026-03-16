namespace Modules.Records.Application.Abstractions;

public sealed record MugshotStorageSaveResult(
    string StoragePath,
    string PublicUrl,
    long FileSizeBytes);
