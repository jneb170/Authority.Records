using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Arrests.Queries.GetArrestByRecordNumber;

public sealed class GetArrestByRecordNumberHandler : IRequestHandler<GetArrestByRecordNumberQuery, ArrestDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetArrestByRecordNumberHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<ArrestDto?> Handle(GetArrestByRecordNumberQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.ArrestReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .FirstOrDefaultAsync(a => a.RecordNumber == request.RecordNumber, cancellationToken);

        if (rm is null)
            return null;

        return (await ArrestDtoMapper.ToDtosAsync([rm], _dbContext, cancellationToken)).Single();
    }
}
