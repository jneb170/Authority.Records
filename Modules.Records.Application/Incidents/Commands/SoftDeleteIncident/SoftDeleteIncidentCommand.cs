using MediatR;

namespace Modules.Records.Application.Incidents.Commands.SoftDeleteIncident;

public sealed record SoftDeleteIncidentCommand(Guid IncidentId) : IRequest;
