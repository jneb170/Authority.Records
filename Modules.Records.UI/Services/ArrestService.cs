using MediatR;
using Modules.Records.Application.Arrests.Commands.AcquireArrestLock;
using Modules.Records.Application.Arrests.Commands.ArchiveArrest;
using Modules.Records.Application.Arrests.Commands.CloseArrest;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Arrests.Commands.FinalizeArrest;
using Modules.Records.Application.Arrests.Commands.OpenArrest;
using Modules.Records.Application.Arrests.Commands.ReleaseArrestLock;
using Modules.Records.Application.Arrests.Commands.RestoreArrest;
using Modules.Records.Application.Arrests.Commands.SoftDeleteArrest;
using Modules.Records.Application.Arrests.Queries.GetArrestById;
using Modules.Records.Application.Arrests.Queries.GetArrestsByIncident;
using Modules.Records.Application.DTOs;

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

    public Task<IReadOnlyList<ArrestDto>> GetByIncidentAsync(Guid incidentId) =>
        _sender.Send(new GetArrestsByIncidentQuery(incidentId));

    public Task<Guid> CreateAsync(Guid incidentId, string suspectName, DateTime arrestedAt) =>
        _sender.Send(new CreateArrestCommand(incidentId, suspectName, arrestedAt));

    public Task OpenAsync(Guid id) =>
        _sender.Send(new OpenArrestCommand(id));

    public Task CloseAsync(Guid id) =>
        _sender.Send(new CloseArrestCommand(id));

    public Task ArchiveAsync(Guid id) =>
        _sender.Send(new ArchiveArrestCommand(id));

    public Task FinalizeAsync(Guid id) =>
        _sender.Send(new FinalizeArrestCommand(id));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireArrestLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseArrestLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteArrestCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreArrestCommand(id));
}
