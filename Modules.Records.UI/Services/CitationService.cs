using MediatR;
using Modules.Records.Application.Citations.Commands.AcquireCitationLock;
using Modules.Records.Application.Citations.Commands.CreateCitation;
using Modules.Records.Application.Citations.Commands.IssueCitation;
using Modules.Records.Application.Citations.Commands.LinkCitationToIncident;
using Modules.Records.Application.Citations.Commands.ReleaseCitationLock;
using Modules.Records.Application.Citations.Commands.RenewCitationLock;
using Modules.Records.Application.Citations.Commands.RestoreCitation;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;
using Modules.Records.Application.Citations.Commands.SoftDeleteCitation;
using Modules.Records.Application.Citations.Commands.UnlinkCitationFromIncident;
using Modules.Records.Application.Citations.Commands.UpdateCitationDetails;
using Modules.Records.Application.Citations.Queries.GetCitationById;
using Modules.Records.Application.Citations.Queries.GetCitationByRecordNumber;
using Modules.Records.Application.Citations.Queries.GetCitationsByIncident;
using Modules.Records.Application.Citations.Queries.GetCitationsByJurisdiction;
using Modules.Records.Application.Citations.Queries.GetIncidentsByCitation;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Incidents.Queries.GetIncidentById;
using Modules.Records.Domain.Common.Violations;

namespace Modules.Records.UI.Services;

public sealed class CitationService : ICitationService
{
    private readonly ISender _sender;

    public CitationService(ISender sender)
    {
        _sender = sender;
    }

    public Task<CitationDto?> GetByIdAsync(Guid id) =>
        _sender.Send(new GetCitationByIdQuery(id));

    public Task<CitationDto?> GetByRecordNumberAsync(long recordNumber) =>
        _sender.Send(new GetCitationByRecordNumberQuery(recordNumber));

    public Task<IReadOnlyList<CitationDto>> GetByJurisdictionAsync() =>
        _sender.Send(new GetCitationsByJurisdictionQuery());

    public Task<IReadOnlyList<CitationDto>> GetByIncidentAsync(Guid incidentId) =>
        _sender.Send(new GetCitationsByIncidentQuery(incidentId));

    public Task<IReadOnlyList<IncidentCitationLinkDto>> GetLinkedIncidentsAsync(Guid citationId) =>
        _sender.Send(new GetIncidentsByCitationQuery(citationId));

    public Task<long> CreateAsync(string description, DateTime issueDate, IReadOnlyList<long> incidentRecordNumbers, string citationNum = "") =>
        _sender.Send(new CreateCitationCommand(description, issueDate, incidentRecordNumbers, citationNum));

    public async Task<long> CreateAsync(Guid incidentId, string description, DateTime issueDate)
    {
        var incident = await _sender.Send(new GetIncidentByIdQuery(incidentId));
        var recordNumbers = incident is not null
            ? new List<long> { incident.RecordNumber } as IReadOnlyList<long>
            : new List<long>() as IReadOnlyList<long>;
        return await _sender.Send(new CreateCitationCommand(description, issueDate, recordNumbers));
    }

    public Task LinkToIncidentAsync(Guid citationId, Guid incidentId) =>
        _sender.Send(new LinkCitationToIncidentCommand(citationId, incidentId));

    public Task UnlinkFromIncidentAsync(Guid citationId, Guid incidentId) =>
        _sender.Send(new UnlinkCitationFromIncidentCommand(citationId, incidentId));

    public Task IssueAsync(Guid id) =>
        _sender.Send(new IssueCitationCommand(id));

    public Task UpdateDetailsAsync(Guid id, Guid? defendantNameId, string description, DateTime issueDate, Guid? courtId = null, string citationNum = "", Guid? locationId = null, NameSnapshotInput? atTimeOfName = null, CitationOfficerProfileInput? officerProfile = null, CitationTexasDetailsInput? texasDetails = null, CitationVehicleInput? vehicle = null) =>
        _sender.Send(new UpdateCitationDetailsCommand(id, defendantNameId, description, issueDate, courtId, citationNum, locationId, atTimeOfName, officerProfile, texasDetails, vehicle));

    public Task SavePageAsync(
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
        CitationOffenseDetailsInput? offenseDetails = null,
        IReadOnlyCollection<ViolationFlagKey>? violationFlags = null,
        IReadOnlyCollection<Guid>? incidentIdsToAdd = null,
        IReadOnlyCollection<Guid>? incidentIdsToRemove = null,
        IReadOnlyCollection<Guid>? chargeIdsToAdd = null,
        IReadOnlyCollection<Guid>? chargeIdsToRemove = null) =>
        _sender.Send(new SaveCitationPageCommand(
            id,
            defendantNameId,
            description,
            issueDate,
            courtId,
            citationNum,
            locationId,
            atTimeOfName,
            officerProfile,
            texasDetails,
            vehicle,
            offenseDetails,
            violationFlags,
            incidentIdsToAdd,
            incidentIdsToRemove,
            chargeIdsToAdd,
            chargeIdsToRemove));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireCitationLockCommand(id));

    public Task RenewLockAsync(Guid id) =>
        _sender.Send(new RenewCitationLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseCitationLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteCitationCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreCitationCommand(id));
}
