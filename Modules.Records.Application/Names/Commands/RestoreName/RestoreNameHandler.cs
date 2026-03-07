using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Commands.RestoreName;

public sealed class RestoreNameHandler : IRequestHandler<RestoreNameCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public RestoreNameHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(RestoreNameCommand request, CancellationToken cancellationToken)
    {
        var name = await _dbContext.Names
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(n =>
                n.Id == request.NameId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Name record not found.");

        name.Restore(_tenantProvider.GetUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
