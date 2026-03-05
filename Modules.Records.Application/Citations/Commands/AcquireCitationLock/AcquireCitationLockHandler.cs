using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Commands.AcquireCitationLock;

public sealed class AcquireCitationLockHandler : IRequestHandler<AcquireCitationLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public AcquireCitationLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(AcquireCitationLockCommand request, CancellationToken cancellationToken)
    {
        var citation = await _dbContext.Citations
            .FirstOrDefaultAsync(c =>
                c.Id == request.CitationId &&
                c.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        citation.AcquireLock(_tenantProvider.GetUserId(), TimeSpan.FromMinutes(10));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
