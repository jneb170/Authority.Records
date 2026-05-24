using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Narratives.Queries.GetNarrativeById;

public sealed class GetNarrativeByIdHandler : IRequestHandler<GetNarrativeByIdQuery, NarrativeDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetNarrativeByIdHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<NarrativeDto?> Handle(GetNarrativeByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.NarrativeReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(n =>
                n.Id == request.Id &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken);

        return rm?.ToDto();
    }
}
