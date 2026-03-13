using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface ICitationService
{
    Task<CitationDto?> GetByIdAsync(Guid id);
    Task<CitationDto?> GetByRecordNumberAsync(long recordNumber);
    Task<IReadOnlyList<CitationDto>> GetByJurisdictionAsync();
    Task<IReadOnlyList<CitationDto>> GetByIncidentAsync(Guid incidentId);
    Task<IReadOnlyList<IncidentCitationLinkDto>> GetLinkedIncidentsAsync(Guid citationId);
    Task<long> CreateAsync(string description, DateTime issueDate, IReadOnlyList<long> incidentRecordNumbers, string citationNum = "");
    Task<long> CreateAsync(Guid incidentId, string description, DateTime issueDate);
    Task LinkToIncidentAsync(Guid citationId, Guid incidentId);
    Task UnlinkFromIncidentAsync(Guid citationId, Guid incidentId);
    Task IssueAsync(Guid id);
    Task UpdateDetailsAsync(Guid id, string description, DateTime issueDate, Guid? courtId = null, string citationNum = "", Guid? locationId = null);
    Task AcquireLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
