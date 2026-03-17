using MediatR;
using Modules.Records.Application.Arrests.Commands.AcquireArrestLock;
using Modules.Records.Application.Arrests.Commands.ArchiveArrest;
using Modules.Records.Application.Arrests.Commands.CloseArrest;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Arrests.Commands.FinalizeArrest;
using Modules.Records.Application.Arrests.Commands.LinkArrestToIncident;
using Modules.Records.Application.Arrests.Commands.OpenArrest;
using Modules.Records.Application.Arrests.Commands.ReleaseArrestLock;
using Modules.Records.Application.Arrests.Commands.RestoreArrest;
using Modules.Records.Application.Arrests.Commands.SoftDeleteArrest;
using Modules.Records.Application.Arrests.Commands.UnlinkArrestFromIncident;
using Modules.Records.Application.Arrests.Commands.UpdateArrestDetails;
using Modules.Records.Application.Arrests.Queries.GetArrestById;
using Modules.Records.Application.Arrests.Queries.GetArrestByRecordNumber;
using Modules.Records.Application.Arrests.Queries.GetArrestsByIncident;
using Modules.Records.Application.Arrests.Queries.GetArrestsByJurisdiction;
using Modules.Records.Application.Arrests.Queries.GetIncidentsByArrest;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Incidents.Queries.GetIncidentById;

namespace Modules.Records.UI.Services;

public sealed class ArrestService : IArrestService
{
    private readonly ISender _sender;

    public ArrestService(ISender sender)
    {
        _sender = sender;
    }

    public Task<ArrestDto?> GetByIdAsync(Guid id) =>
        _sender.Send(new GetArrestByIdQuery(id));

    public Task<ArrestDto?> GetByRecordNumberAsync(long recordNumber) =>
        _sender.Send(new GetArrestByRecordNumberQuery(recordNumber));

    public Task<IReadOnlyList<ArrestDto>> GetByJurisdictionAsync() =>
        _sender.Send(new GetArrestsByJurisdictionQuery());

    public Task<IReadOnlyList<ArrestDto>> GetByIncidentAsync(Guid incidentId) =>
        _sender.Send(new GetArrestsByIncidentQuery(incidentId));

    public Task<IReadOnlyList<IncidentArrestLinkDto>> GetLinkedIncidentsAsync(Guid arrestId) =>
        _sender.Send(new GetIncidentsByArrestQuery(arrestId));

    public Task<long> CreateAsync(Guid nameId, DateTime arrestedAt, IReadOnlyList<long> incidentRecordNumbers, string arrestNum = "", Guid? primaryIncidentId = null) =>
        _sender.Send(new CreateArrestCommand(nameId, arrestedAt, incidentRecordNumbers, arrestNum, primaryIncidentId));

    public async Task<long> CreateAsync(Guid incidentId, Guid nameId, DateTime arrestedAt)
    {
        var incident = await _sender.Send(new GetIncidentByIdQuery(incidentId));
        var recordNumbers = incident is not null
            ? new List<long> { incident.RecordNumber } as IReadOnlyList<long>
            : new List<long>() as IReadOnlyList<long>;
        return await _sender.Send(new CreateArrestCommand(nameId, arrestedAt, recordNumbers, PrimaryIncidentId: incidentId));
    }

    public Task LinkToIncidentAsync(Guid arrestId, Guid incidentId) =>
        _sender.Send(new LinkArrestToIncidentCommand(arrestId, incidentId));

    public Task UnlinkFromIncidentAsync(Guid arrestId, Guid incidentId) =>
        _sender.Send(new UnlinkArrestFromIncidentCommand(arrestId, incidentId));

    public Task OpenAsync(Guid id) =>
        _sender.Send(new OpenArrestCommand(id));

    public Task CloseAsync(Guid id) =>
        _sender.Send(new CloseArrestCommand(id));

    public Task ArchiveAsync(Guid id) =>
        _sender.Send(new ArchiveArrestCommand(id));

    public Task FinalizeAsync(Guid id) =>
        _sender.Send(new FinalizeArrestCommand(id));

    public Task UpdateDetailsAsync(Guid id, Guid nameId, DateTime arrestedAt, Guid? arrestTypeId = null, string arrestNum = "", Guid? locationId = null, Guid? primaryIncidentId = null) =>
        _sender.Send(new UpdateArrestDetailsCommand(id, nameId, arrestedAt, arrestTypeId, arrestNum, locationId, primaryIncidentId));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireArrestLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseArrestLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteArrestCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreArrestCommand(id));
}
