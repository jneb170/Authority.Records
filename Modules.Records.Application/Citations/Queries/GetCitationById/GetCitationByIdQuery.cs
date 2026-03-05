using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetCitationById;

public sealed record GetCitationByIdQuery(Guid CitationId) : IRequest<CitationDto?>;
