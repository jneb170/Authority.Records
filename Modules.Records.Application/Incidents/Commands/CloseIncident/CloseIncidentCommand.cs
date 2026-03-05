using MediatR;

namespace Modules.Records.Application.Incidents.Commands.CloseIncident;

public sealed record CloseIncidentCommand(Guid IncidentId, bool Force = false) : IRequest;
