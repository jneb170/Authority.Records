using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IArrestService
{
    Task<ArrestDto?> GetByIdAsync(Guid id);
    Task<ArrestDto?> GetByRecordNumberAsync(long recordNumber);
    Task<IReadOnlyList<ArrestDto>> GetByJurisdictionAsync();
    Task<IReadOnlyList<ArrestDto>> GetByIncidentAsync(Guid incidentId);
    Task<IReadOnlyList<IncidentArrestLinkDto>> GetLinkedIncidentsAsync(Guid arrestId);
    Task<long> CreateAsync(string suspectName, DateTime arrestedAt, IReadOnlyList<long> incidentRecordNumbers, string arrestNum = "");
    Task<long> CreateAsync(Guid incidentId, string suspectName, DateTime arrestedAt);
    Task LinkToIncidentAsync(Guid arrestId, Guid incidentId);
    Task UnlinkFromIncidentAsync(Guid arrestId, Guid incidentId);
    Task OpenAsync(Guid id);
    Task CloseAsync(Guid id);
    Task ArchiveAsync(Guid id);
    Task FinalizeAsync(Guid id);
    Task UpdateDetailsAsync(Guid id, string suspectName, DateTime arrestedAt, Guid? arrestTypeId = null, string arrestNum = "");
    Task AcquireLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
