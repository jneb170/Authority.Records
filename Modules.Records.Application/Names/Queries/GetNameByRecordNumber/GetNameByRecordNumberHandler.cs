using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Names.Queries.GetNameByRecordNumber;

public sealed class GetNameByRecordNumberHandler : IRequestHandler<GetNameByRecordNumberQuery, NameDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetNameByRecordNumberHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<NameDto?> Handle(
        GetNameByRecordNumberQuery request,
        CancellationToken cancellationToken)
    {
        var rm = await _dbContext.NameReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(n =>
                n.RecordNumber == request.RecordNumber &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken);

        return rm?.ToDto();
    }
}
