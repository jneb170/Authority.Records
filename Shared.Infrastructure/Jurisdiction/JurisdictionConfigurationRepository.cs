using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Shared.Infrastructure.Persistence;


namespace Shared.Infrastructure.Jurisdiction;

public sealed class JurisdictionConfigurationRepository
    : IJurisdictionConfigurationRepository
{
    private readonly AppDbContext _context;

    public JurisdictionConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JurisdictionConfiguration?> GetByJurisdictionIdAsync(
        Guid jurisdictionId,
        CancellationToken cancellationToken)
    {
        var entity = await _context.JurisdictionConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.JurisdictionId == jurisdictionId, cancellationToken);

        if (entity == null)
            return null;

        return new JurisdictionConfiguration(
            entity.JurisdictionId,
            entity.MustCloseAllArrests,
            entity.MustCloseAllCitations);
    }
}