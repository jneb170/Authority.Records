using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Queries.GetCitationById;

public sealed class GetCitationByIdHandler : IRequestHandler<GetCitationByIdQuery, CitationDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCitationByIdHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<CitationDto?> Handle(GetCitationByIdQuery request, CancellationToken cancellationToken)
    {
        var rm = await _dbContext.CitationReadModels
            .AsNoTracking()
            .WhereAgencyScoped(_tenantProvider.GetAgencyId())
            .FirstOrDefaultAsync(c => c.Id == request.CitationId, cancellationToken);

        if (rm is null)
            return null;

        return (await CitationDtoMapper.ToDtosAsync([rm], _dbContext, cancellationToken)).Single();
    }
}
