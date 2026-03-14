using MediatR;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Incidents.Commands.UpdateIncidentDetails;

public sealed record UpdateIncidentDetailsCommand(
    Guid            IncidentId,
    IncidentDetails Details,
    Guid?           LocationId  = null,
    DateTime?       OccurredOn  = null) : IRequest;
