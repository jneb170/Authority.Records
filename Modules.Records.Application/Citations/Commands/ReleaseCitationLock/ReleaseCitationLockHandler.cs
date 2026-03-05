using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Commands.ReleaseCitationLock;

public sealed class ReleaseCitationLockHandler : IRequestHandler<ReleaseCitationLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public ReleaseCitationLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(ReleaseCitationLockCommand request, CancellationToken cancellationToken)
    {
        var citation = await _dbContext.Citations
            .FirstOrDefaultAsync(c =>
                c.Id == request.CitationId &&
                c.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        citation.ReleaseLock(_tenantProvider.GetUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
