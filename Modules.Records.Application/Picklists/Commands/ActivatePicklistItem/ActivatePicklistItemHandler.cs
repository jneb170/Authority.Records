using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Picklists.Commands.ActivatePicklistItem;

public sealed class ActivatePicklistItemHandler : IRequestHandler<ActivatePicklistItemCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public ActivatePicklistItemHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(ActivatePicklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.PicklistItems
            .FirstOrDefaultAsync(p =>
                p.Id == request.ItemId &&
                p.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Picklist item not found.");

        item.Activate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
