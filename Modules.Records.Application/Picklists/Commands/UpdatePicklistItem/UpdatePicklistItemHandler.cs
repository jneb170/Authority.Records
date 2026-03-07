using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Picklists.Commands.UpdatePicklistItem;

public sealed class UpdatePicklistItemHandler : IRequestHandler<UpdatePicklistItemCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public UpdatePicklistItemHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UpdatePicklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.PicklistItems
            .FirstOrDefaultAsync(p =>
                p.Id == request.ItemId &&
                p.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Picklist item not found.");

        item.UpdateLabel(request.Label);
        item.UpdateSortOrder(request.SortOrder);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
