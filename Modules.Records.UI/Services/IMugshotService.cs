using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IMugshotService
{
    Task<IReadOnlyList<MugshotDto>> GetForNameAsync(Guid nameId);
    Task<IReadOnlyList<MugshotDto>> GetForArrestAsync(Guid arrestId);

    Task<Guid> UploadForNameAsync(
        Guid nameId,
        string fileName,
        string contentType,
        byte[] content,
        bool makePrimary = false,
        DateTime? capturedAtUtc = null);

    Task<Guid> UploadForArrestAsync(
        Guid arrestId,
        string fileName,
        string contentType,
        byte[] content,
        bool makePrimary = false,
        DateTime? capturedAtUtc = null);

    Task SetPrimaryForNameAsync(Guid nameId, Guid mugshotId);
    Task SetPrimaryForArrestAsync(Guid arrestId, Guid mugshotId);
    Task RemoveFromNameAsync(Guid nameId, Guid mugshotId);
    Task RemoveFromArrestAsync(Guid arrestId, Guid mugshotId);
}
