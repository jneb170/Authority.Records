using MediatR;
using Modules.Records.Application.Common;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Incidents.Commands.CreateIncident;

public sealed record CreateIncidentCommand(IncidentDetails Details) : IRequest<long>, IRateLimitedCommand;
