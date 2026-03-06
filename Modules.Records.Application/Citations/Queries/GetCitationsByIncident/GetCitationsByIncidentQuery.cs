using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetCitationsByIncident;

public sealed record GetCitationsByIncidentQuery(Guid IncidentId) : IRequest<IReadOnlyList<CitationDto>>;
