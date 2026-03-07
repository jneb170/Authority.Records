using MediatR;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Incidents.Commands.CreateIncident;

public sealed record CreateIncidentCommand(Guid AgencyId, IncidentDetails Details) : IRequest<long>;
