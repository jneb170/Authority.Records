using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Narratives.Commands.RestoreNarrative;

public sealed class RestoreNarrativeHandler : IRequestHandler<RestoreNarrativeCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public RestoreNarrativeHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(RestoreNarrativeCommand request, CancellationToken cancellationToken)
    {
        // Soft-deleted records are excluded by the global query filter, so bypass it to find one.
        var narrative = await _dbContext.Narratives
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(n =>
                n.Id == request.NarrativeId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Narrative record not found.");

        narrative.Restore(_tenantProvider.GetUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
