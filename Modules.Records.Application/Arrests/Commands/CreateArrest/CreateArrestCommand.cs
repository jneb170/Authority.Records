using MediatR;

namespace Modules.Records.Application.Arrests.Commands.CreateArrest;

public sealed record CreateArrestCommand(
    Guid IncidentId,
    string SuspectName,
    DateTime ArrestedAt) : IRequest<Guid>;
