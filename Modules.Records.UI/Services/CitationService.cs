using MediatR;
using Modules.Records.Application.Citations.Commands.AcquireCitationLock;
using Modules.Records.Application.Citations.Commands.CreateCitation;
using Modules.Records.Application.Citations.Commands.IssueCitation;
using Modules.Records.Application.Citations.Commands.ReleaseCitationLock;
using Modules.Records.Application.Citations.Commands.RestoreCitation;
using Modules.Records.Application.Citations.Commands.SoftDeleteCitation;
using Modules.Records.Application.Citations.Commands.UpdateCitationDetails;
using Modules.Records.Application.Citations.Queries.GetCitationById;
using Modules.Records.Application.Citations.Queries.GetCitationsByIncident;
using Modules.Records.Application.DTOs;

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

    public Task<IReadOnlyList<CitationDto>> GetByIncidentAsync(Guid incidentId) =>
        _sender.Send(new GetCitationsByIncidentQuery(incidentId));

    public Task<Guid> CreateAsync(Guid incidentId, string description, DateTime issueDate) =>
        _sender.Send(new CreateCitationCommand(incidentId, description, issueDate));

    public Task IssueAsync(Guid id) =>
        _sender.Send(new IssueCitationCommand(id));

    public Task UpdateDetailsAsync(Guid id, string description, DateTime issueDate) =>
        _sender.Send(new UpdateCitationDetailsCommand(id, description, issueDate));

    public Task AcquireLockAsync(Guid id) =>
        _sender.Send(new AcquireCitationLockCommand(id));

    public Task ReleaseLockAsync(Guid id) =>
        _sender.Send(new ReleaseCitationLockCommand(id));

    public Task SoftDeleteAsync(Guid id) =>
        _sender.Send(new SoftDeleteCitationCommand(id));

    public Task RestoreAsync(Guid id) =>
        _sender.Send(new RestoreCitationCommand(id));
}
