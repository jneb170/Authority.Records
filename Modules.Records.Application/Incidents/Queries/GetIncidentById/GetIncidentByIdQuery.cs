using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentById;

public sealed record GetIncidentByIdQuery(Guid IncidentId) : IRequest<IncidentDto?>;
