using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Picklists.Commands.SeedDefaultPicklistItems;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistItems;

public sealed class GetPicklistItemsHandler : IRequestHandler<GetPicklistItemsQuery, IReadOnlyList<PicklistItemDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMediator _mediator;

    public GetPicklistItemsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<PicklistItemDto>> Handle(
        GetPicklistItemsQuery request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();

        var query = _dbContext.PicklistItems
            .Where(p =>
                p.JurisdictionId == jurisdictionId &&
                p.AgencyId == agencyId &&
                p.PicklistType == request.PicklistType);

        if (request.ActiveOnly)
            query = query.Where(p => p.IsActive);

        var items = await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Label)
            .ToListAsync(cancellationToken);

        // On-demand seeding: seed system defaults if no items exist at all for this agency/type
        if (items.Count == 0 && !request.ActiveOnly)
        {
            // Check without active filter
            var hasAny = await _dbContext.PicklistItems
                .AnyAsync(p =>
                    p.JurisdictionId == jurisdictionId &&
                    p.AgencyId == agencyId &&
                    p.PicklistType == request.PicklistType,
                    cancellationToken);

            if (!hasAny)
            {
                await _mediator.Send(
                    new SeedDefaultPicklistItemsCommand(request.PicklistType),
                    cancellationToken);

                items = await _dbContext.PicklistItems
                    .Where(p =>
                        p.JurisdictionId == jurisdictionId &&
                        p.AgencyId == agencyId &&
                        p.PicklistType == request.PicklistType)
                    .OrderBy(p => p.SortOrder)
                    .ThenBy(p => p.Label)
                    .ToListAsync(cancellationToken);
            }
        }
        else if (items.Count == 0 && request.ActiveOnly)
        {
            // Seed on-demand for active-only requests too (normal select scenario)
            await _mediator.Send(
                new SeedDefaultPicklistItemsCommand(request.PicklistType),
                cancellationToken);

            items = await query
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Label)
                .ToListAsync(cancellationToken);
        }

        return items
            .Select(p => new PicklistItemDto(
                p.Id, p.JurisdictionId, p.AgencyId,
                p.PicklistType, p.Value, p.Label,
                p.SortOrder, p.IsActive, p.IsSystemDefault))
            .ToList();
    }
}
