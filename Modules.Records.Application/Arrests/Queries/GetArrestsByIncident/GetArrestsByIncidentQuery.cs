using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetArrestsByIncident;

public sealed record GetArrestsByIncidentQuery(Guid IncidentId) : IRequest<IReadOnlyList<ArrestDto>>;
