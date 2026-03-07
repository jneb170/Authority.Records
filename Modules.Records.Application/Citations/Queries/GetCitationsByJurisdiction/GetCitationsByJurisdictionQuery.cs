using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetCitationsByJurisdiction;

public sealed record GetCitationsByJurisdictionQuery : IRequest<IReadOnlyList<CitationDto>>;
