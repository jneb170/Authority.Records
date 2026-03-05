using MediatR;

namespace Modules.Records.Application.Citations.Commands.CreateCitation;

public sealed record CreateCitationCommand(
    Guid IncidentId,
    string Description,
    DateTime IssueDate) : IRequest<Guid>;
