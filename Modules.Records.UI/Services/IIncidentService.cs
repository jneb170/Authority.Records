using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentDto>> GetByJurisdictionAsync();
    Task<IncidentDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(string description);
    Task OpenAsync(Guid id);
    Task CloseAsync(Guid id);
    Task ArchiveAsync(Guid id);
    Task UpdateDescriptionAsync(Guid id, string description);
    Task AcquireLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
