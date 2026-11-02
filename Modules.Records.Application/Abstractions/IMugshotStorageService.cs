namespace Modules.Records.Application.Abstractions;

public interface IMugshotStorageService
{
    /// <summary>
    /// Stores a mugshot image and returns the key it is stored under. The file is not
    /// publicly reachable; it is served through the authorized mugshot endpoint.
    /// </summary>
    Task<MugshotStorageSaveResult> SaveAsync(
        Guid jurisdictionId,
        Guid mugshotId,
        byte[] content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>Opens a stored image for reading, or returns null if the file is missing.</summary>
    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}
