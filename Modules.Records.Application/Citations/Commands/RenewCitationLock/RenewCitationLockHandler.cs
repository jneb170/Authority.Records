using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Commands.RenewCitationLock;

public sealed class RenewCitationLockHandler : IRequestHandler<RenewCitationLockCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public RenewCitationLockHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(RenewCitationLockCommand request, CancellationToken cancellationToken)
    {
        var citation = await _dbContext.Citations
            .FirstOrDefaultAsync(c =>
                c.Id == request.CitationId &&
                c.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        citation.RenewLock(_modificationContext);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
