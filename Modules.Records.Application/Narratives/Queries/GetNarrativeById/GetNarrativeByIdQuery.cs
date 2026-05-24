using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Narratives.Queries.GetNarrativeById;

public sealed record GetNarrativeByIdQuery(Guid Id) : IRequest<NarrativeDto?>;
