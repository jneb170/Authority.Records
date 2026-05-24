using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Narratives.Commands.SoftDeleteNarrative;

public sealed class SoftDeleteNarrativeHandler : IRequestHandler<SoftDeleteNarrativeCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SoftDeleteNarrativeHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(SoftDeleteNarrativeCommand request, CancellationToken cancellationToken)
    {
        var narrative = await _dbContext.Narratives
            .FirstOrDefaultAsync(n =>
                n.Id == request.NarrativeId &&
                n.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Narrative record not found.");

        narrative.SoftDelete(_tenantProvider.GetUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
