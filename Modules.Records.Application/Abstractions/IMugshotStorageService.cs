namespace Modules.Records.Application.Abstractions;

public interface IMugshotStorageService
{
    Task<MugshotStorageSaveResult> SaveAsync(
        Guid jurisdictionId,
        byte[] content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}
