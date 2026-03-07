using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Queries.GetNameById;

public sealed class GetNameByIdHandler : IRequestHandler<GetNameByIdQuery, NameDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetNameByIdHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<NameDto?> Handle(GetNameByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.NameReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(n =>
                n.Id == request.Id &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken);

        return rm?.ToDto();
    }
}
