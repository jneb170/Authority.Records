using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Commands.SoftDeleteName;

public sealed class SoftDeleteNameHandler : IRequestHandler<SoftDeleteNameCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SoftDeleteNameHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(SoftDeleteNameCommand request, CancellationToken cancellationToken)
    {
        var name = await _dbContext.Names
            .FirstOrDefaultAsync(n =>
                n.Id == request.NameId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Name record not found.");

        name.SoftDelete(_tenantProvider.GetUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
