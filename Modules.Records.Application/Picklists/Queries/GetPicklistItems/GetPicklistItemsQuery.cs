using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistItems;

/// <summary>
/// Returns active picklist items for the given type and current agency.
/// If no items exist, seeds system defaults on-demand before returning.
/// Pass <paramref name="activeOnly"/> = false to include deactivated items (for admin UIs).
/// </summary>
public sealed record GetPicklistItemsQuery(string PicklistType, bool ActiveOnly = true)
    : IRequest<IReadOnlyList<PicklistItemDto>>;
