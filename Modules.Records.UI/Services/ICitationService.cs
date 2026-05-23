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
    Task UpdateDetailsAsync(Guid id, Guid? defendantNameId, string description, DateTime issueDate, Guid? courtId = null, string citationNum = "", Guid? locationId = null, NameSnapshotInput? atTimeOfName = null, CitationOfficerProfileInput? officerProfile = null, CitationTexasDetailsInput? texasDetails = null, CitationVehicleInput? vehicle = null);
    Task SavePageAsync(
        Guid id,
        Guid? defendantNameId,
        string description,
        DateTime issueDate,
        Guid? courtId = null,
        string citationNum = "",
        Guid? locationId = null,
        NameSnapshotInput? atTimeOfName = null,
        CitationOfficerProfileInput? officerProfile = null,
        CitationTexasDetailsInput? texasDetails = null,
        CitationVehicleInput? vehicle = null,
        IReadOnlyCollection<Guid>? incidentIdsToAdd = null,
        IReadOnlyCollection<Guid>? incidentIdsToRemove = null,
        IReadOnlyCollection<Guid>? chargeIdsToAdd = null,
        IReadOnlyCollection<Guid>? chargeIdsToRemove = null);
    Task AcquireLockAsync(Guid id);
    Task RenewLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
