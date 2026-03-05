using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Citations.Commands.RestoreCitation;

public sealed class RestoreCitationHandler : IRequestHandler<RestoreCitationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public RestoreCitationHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(RestoreCitationCommand request, CancellationToken cancellationToken)
    {
        var citation = await _dbContext.Citations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c =>
                c.Id == request.CitationId &&
                c.JurisdictionId == _tenantProvider.GetJurisdictionId(),
                cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        citation.Restore(_tenantProvider.GetUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
