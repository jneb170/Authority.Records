using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface ICitationService
{
    Task<CitationDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<CitationDto>> GetByIncidentAsync(Guid incidentId);
    Task<Guid> CreateAsync(Guid incidentId, string description, DateTime issueDate);
    Task IssueAsync(Guid id);
    Task UpdateDetailsAsync(Guid id, string description, DateTime issueDate);
    Task AcquireLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
