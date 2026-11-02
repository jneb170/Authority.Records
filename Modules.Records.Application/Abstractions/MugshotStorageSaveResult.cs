namespace Modules.Records.Application.Abstractions;

public sealed record MugshotStorageSaveResult(
    string StoragePath,
    long FileSizeBytes);
