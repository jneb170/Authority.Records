using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistItemsByIds;

/// <summary>Efficiently fetches a dictionary of Id → Label for a set of known picklist item IDs.</summary>
public sealed record GetPicklistItemsByIdsQuery(IReadOnlyList<Guid> Ids)
    : IRequest<Dictionary<Guid, string>>;
