using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Picklists.Commands.DeactivatePicklistItem;

public sealed class DeactivatePicklistItemHandler : IRequestHandler<DeactivatePicklistItemCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public DeactivatePicklistItemHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(DeactivatePicklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.PicklistItems
            .FirstOrDefaultAsync(p =>
                p.Id == request.ItemId &&
                p.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Picklist item not found.");

        item.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
