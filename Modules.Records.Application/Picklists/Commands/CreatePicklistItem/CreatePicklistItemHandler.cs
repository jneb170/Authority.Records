using MediatR;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Picklists.Commands.CreatePicklistItem;

public sealed class CreatePicklistItemHandler : IRequestHandler<CreatePicklistItemCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreatePicklistItemHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreatePicklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = new PicklistItem(
            _tenantProvider.GetJurisdictionId(),
            _tenantProvider.GetAgencyId(),
            request.PicklistType,
            request.Value,
            request.Label,
            request.SortOrder);

        _dbContext.PicklistItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
