using Modules.Records.Application.Charges.Commands.SeedCharges;
using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public interface IChargeService
{
    Task<IReadOnlyList<ChargeDto>> SearchAsync(string? term = null, bool includeInactive = false, bool citationEligibleOnly = false);
    Task<Guid> CreateAsync(string offenseName, string ucrCategory, string nibrsGroup, string crimeAgainst, string ucrCode, string chargeLevel, string? stateClass, bool isCitationEligible, bool isActive = true);
    Task UpdateAsync(Guid chargeId, string offenseName, string ucrCategory, string nibrsGroup, string crimeAgainst, string ucrCode, string chargeLevel, string? stateClass, bool isCitationEligible, bool isActive);
    Task ActivateAsync(Guid chargeId);
    Task DeactivateAsync(Guid chargeId);
    Task DeleteAsync(Guid chargeId);
    Task<ChargeSeedResult> SeedFromJsonAsync(string jsonContent);
    Task<IReadOnlyList<RecordChargeDto>> GetByIncidentAsync(Guid incidentId);
    Task<IReadOnlyList<RecordChargeDto>> GetByArrestAsync(Guid arrestId);
    Task<IReadOnlyList<RecordChargeDto>> GetByCitationAsync(Guid citationId);
    Task LinkToIncidentAsync(Guid incidentId, Guid chargeId);
    Task UnlinkFromIncidentAsync(Guid incidentId, Guid chargeId);
    Task LinkToArrestAsync(Guid arrestId, Guid chargeId);
    Task UnlinkFromArrestAsync(Guid arrestId, Guid chargeId);
    Task LinkToCitationAsync(Guid citationId, Guid chargeId);
    Task UnlinkFromCitationAsync(Guid citationId, Guid chargeId);
}
