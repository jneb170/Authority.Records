using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IArrestService
{
    Task<ArrestDto?> GetByIdAsync(Guid id);
    Task<ArrestDto?> GetByRecordNumberAsync(long recordNumber);
    Task<IReadOnlyList<ArrestDto>> GetByJurisdictionAsync();
    Task<IReadOnlyList<ArrestDto>> GetByIncidentAsync(Guid incidentId);
    Task<IReadOnlyList<IncidentArrestLinkDto>> GetLinkedIncidentsAsync(Guid arrestId);
    Task<long> CreateAsync(Guid nameId, DateTime arrestedAt, IReadOnlyList<long> incidentRecordNumbers, string arrestNum = "", Guid? primaryIncidentId = null);
    Task<long> CreateAsync(Guid incidentId, Guid nameId, DateTime arrestedAt);
    Task LinkToIncidentAsync(Guid arrestId, Guid incidentId);
    Task UnlinkFromIncidentAsync(Guid arrestId, Guid incidentId);
    Task OpenAsync(Guid id);
    Task CloseAsync(Guid id);
    Task ArchiveAsync(Guid id);
    Task FinalizeAsync(Guid id);
    Task UpdateDetailsAsync(Guid id, Guid? nameId, DateTime arrestedAt, Guid? arrestTypeId = null, string arrestNum = "", Guid? locationId = null, Guid? primaryIncidentId = null);
    Task AcquireLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
