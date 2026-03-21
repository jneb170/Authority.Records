using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Queries.GetArrestById;

public sealed class GetArrestByIdHandler : IRequestHandler<GetArrestByIdQuery, ArrestDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetArrestByIdHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<ArrestDto?> Handle(GetArrestByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .FirstOrDefaultAsync(a => a.Id == request.ArrestId, cancellationToken);

        if (rm is null)
            return null;

        return (await ArrestDtoMapper.ToDtosAsync([rm], _dbContext, cancellationToken)).Single();
    }
}
