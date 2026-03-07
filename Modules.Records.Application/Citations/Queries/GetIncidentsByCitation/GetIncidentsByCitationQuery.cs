using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetIncidentsByCitation;

public sealed record GetIncidentsByCitationQuery(Guid CitationId) : IRequest<IReadOnlyList<IncidentCitationLinkDto>>;
