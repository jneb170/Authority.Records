using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Narratives.Queries.GetNarrativesByOwner;

/// <summary>Lists the narratives linked to a given owner (e.g. an Incident), ordered for display.</summary>
public sealed record GetNarrativesByOwnerQuery(string OwnerType, Guid OwnerId)
    : IRequest<IReadOnlyList<NarrativeDto>>;
