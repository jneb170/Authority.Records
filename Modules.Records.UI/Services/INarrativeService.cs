using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface INarrativeService
{
    Task<IReadOnlyList<NarrativeDto>> GetByOwnerAsync(string ownerType, Guid ownerId);
    Task<NarrativeDto?> GetByIdAsync(Guid id);
    Task<long> CreateAsync(string ownerType, Guid ownerId, string title, string content);
    Task UpdateContentAsync(Guid id, string title, string content);
    Task AcquireLockAsync(Guid id);
    Task RenewLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
