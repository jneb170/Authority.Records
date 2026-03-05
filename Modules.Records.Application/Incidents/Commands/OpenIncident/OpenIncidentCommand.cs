using MediatR;

namespace Modules.Records.Application.Incidents.Commands.OpenIncident;

public sealed record OpenIncidentCommand(Guid IncidentId) : IRequest;
