using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IArrestService
{
    Task<ArrestDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<ArrestDto>> GetByIncidentAsync(Guid incidentId);
    Task<Guid> CreateAsync(Guid incidentId, string suspectName, DateTime arrestedAt);
    Task OpenAsync(Guid id);
    Task CloseAsync(Guid id);
    Task ArchiveAsync(Guid id);
    Task FinalizeAsync(Guid id);
    Task UpdateDetailsAsync(Guid id, string suspectName, DateTime arrestedAt);
    Task AcquireLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
