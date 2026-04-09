using MediatR;
using Modules.Records.Application.Charges.Commands.ActivateCharge;
using Modules.Records.Application.Charges.Commands.CreateCharge;
using Modules.Records.Application.Charges.Commands.DeactivateCharge;
using Modules.Records.Application.Charges.Commands.DeleteCharge;
using Modules.Records.Application.Charges.Commands.LinkChargeToArrest;
using Modules.Records.Application.Charges.Commands.LinkChargeToCitation;
using Modules.Records.Application.Charges.Commands.LinkChargeToIncident;
using Modules.Records.Application.Charges.Commands.SeedCharges;
using Modules.Records.Application.Charges.Commands.UnlinkChargeFromArrest;
using Modules.Records.Application.Charges.Commands.UnlinkChargeFromCitation;
using Modules.Records.Application.Charges.Commands.UnlinkChargeFromIncident;
using Modules.Records.Application.Charges.Commands.UpdateCharge;
using Modules.Records.Application.Charges.Queries.CountCharges;
using Modules.Records.Application.Charges.Queries.GetChargesByArrest;
using Modules.Records.Application.Charges.Queries.GetChargesByCitation;
using Modules.Records.Application.Charges.Queries.GetChargesByIncident;
using Modules.Records.Application.Charges.Queries.SearchCharges;
using Modules.Records.Application.DTOs;

namespace Modules.Records.UI.Services;

public sealed class ChargeService : IChargeService
{
    private readonly ISender _sender;

    public ChargeService(ISender sender)
    {
        _sender = sender;
    }

    public Task<IReadOnlyList<ChargeDto>> SearchAsync(string? term = null, bool includeInactive = false, bool citationEligibleOnly = false) =>
        _sender.Send(new SearchChargesQuery(term, includeInactive, citationEligibleOnly));

    public Task<int> CountAsync(bool includeInactive = false) =>
        _sender.Send(new CountChargesQuery(includeInactive));

    public Task<Guid> CreateAsync(string offenseName, string ucrCategory, string nibrsGroup, string crimeAgainst, string ucrCode, string chargeLevel, string? stateClass, bool isCitationEligible, bool isActive = true) =>
        _sender.Send(new CreateChargeCommand(offenseName, ucrCategory, nibrsGroup, crimeAgainst, ucrCode, chargeLevel, stateClass, isCitationEligible, isActive));

    public Task UpdateAsync(Guid chargeId, string offenseName, string ucrCategory, string nibrsGroup, string crimeAgainst, string ucrCode, string chargeLevel, string? stateClass, bool isCitationEligible, bool isActive) =>
        _sender.Send(new UpdateChargeCommand(chargeId, offenseName, ucrCategory, nibrsGroup, crimeAgainst, ucrCode, chargeLevel, stateClass, isCitationEligible, isActive));

    public Task ActivateAsync(Guid chargeId) =>
        _sender.Send(new ActivateChargeCommand(chargeId));

    public Task DeactivateAsync(Guid chargeId) =>
        _sender.Send(new DeactivateChargeCommand(chargeId));

    public Task DeleteAsync(Guid chargeId) =>
        _sender.Send(new DeleteChargeCommand(chargeId));

    public Task<ChargeSeedResult> SeedFromJsonAsync(string jsonContent) =>
        _sender.Send(new SeedChargesCommand(jsonContent));

    public Task<IReadOnlyList<RecordChargeDto>> GetByIncidentAsync(Guid incidentId) =>
        _sender.Send(new GetChargesByIncidentQuery(incidentId));

    public Task<IReadOnlyList<RecordChargeDto>> GetByArrestAsync(Guid arrestId) =>
        _sender.Send(new GetChargesByArrestQuery(arrestId));

    public Task<IReadOnlyList<RecordChargeDto>> GetByCitationAsync(Guid citationId) =>
        _sender.Send(new GetChargesByCitationQuery(citationId));

    public Task LinkToIncidentAsync(Guid incidentId, Guid chargeId) =>
        _sender.Send(new LinkChargeToIncidentCommand(incidentId, chargeId));

    public Task UnlinkFromIncidentAsync(Guid incidentId, Guid chargeId) =>
        _sender.Send(new UnlinkChargeFromIncidentCommand(incidentId, chargeId));

    public Task LinkToArrestAsync(Guid arrestId, Guid chargeId) =>
        _sender.Send(new LinkChargeToArrestCommand(arrestId, chargeId));

    public Task UnlinkFromArrestAsync(Guid arrestId, Guid chargeId) =>
        _sender.Send(new UnlinkChargeFromArrestCommand(arrestId, chargeId));

    public Task LinkToCitationAsync(Guid citationId, Guid chargeId) =>
        _sender.Send(new LinkChargeToCitationCommand(citationId, chargeId));

    public Task UnlinkFromCitationAsync(Guid citationId, Guid chargeId) =>
        _sender.Send(new UnlinkChargeFromCitationCommand(citationId, chargeId));
}
