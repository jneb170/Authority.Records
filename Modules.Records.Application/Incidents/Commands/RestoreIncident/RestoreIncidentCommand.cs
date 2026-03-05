using MediatR;

namespace Modules.Records.Application.Incidents.Commands.RestoreIncident;

public sealed record RestoreIncidentCommand(Guid IncidentId) : IRequest;
