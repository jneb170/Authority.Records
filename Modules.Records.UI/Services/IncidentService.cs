using MediatR;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Incidents.Commands.AcquireIncidentLock;
using Modules.Records.Application.Incidents.Commands.ArchiveIncident;
using Modules.Records.Application.Incidents.Commands.CloseIncident;
using Modules.Records.Application.Incidents.Commands.CreateIncident;
using Modules.Records.Application.Incidents.Commands.OpenIncident;
using Modules.Records.Application.Incidents.Commands.ReleaseIncidentLock;
using Modules.Records.Application.Incidents.Commands.RestoreIncident;
using Modules.Records.Application.Incidents.Commands.SoftDeleteIncident;
using Modules.Records.Application.Incidents.Commands.UpdateIncidentDetails;
using Modules.Records.Application.Incidents.Queries.GetIncidentById;
using Modules.Records.Application.Incidents.Queries.GetIncidentsByJurisdiction;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.UI.Services;

public sealed class IncidentService : IIncidentService
{
    private readonly ISender _sender;
    private readonly ITenantProvider _tenantProvider;

    public IncidentService(ISender sender, ITenantProvider tenantProvider)
    {
        _sender = sender;
        _tenantProvider = tenantProvider;
    }

    public Task<IReadOnlyList<IncidentDto>> GetByJurisdictionAsync() =>
        _sender.Send(new GetIncidentsByJurisdictionQuery(_tenantProvider.GetJurisdictionId()));

    public Task<IncidentDto?> GetByIdAsync(Guid id) =>
        _sender.Send(new GetIncidentByIdQuery(id));

    public Task<Guid> CreateAsync(IncidentDetails details) =>
        _sender.Send(new CreateIncidentCommand(_tenantProvider.GetAgencyId(), details));

    public Task OpenAsync(Guid id) =>
        _sender.Send(new OpenIncidentCommand(id));

    public Task CloseAsync(Guid id) =>
        _sender.Send(new CloseIncidentCommand(id));

    public Task ArchiveAsync(Guid id) =>
        _sender.Send(new ArchiveIncidentCommand(id));

    public Task UpdateDetailsAsync(Guid id, IncidentDetails details) =>
        _sender.Send(new UpdateIncidentDetailsCommand(id, details));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireIncidentLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseIncidentLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteIncidentCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreIncidentCommand(id));
}
