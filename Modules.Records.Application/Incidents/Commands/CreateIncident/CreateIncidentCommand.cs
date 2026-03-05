using MediatR;

namespace Modules.Records.Application.Incidents.Commands.CreateIncident;

public sealed record CreateIncidentCommand(Guid AgencyId, string Description) : IRequest<Guid>;
