using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Citations;

public sealed class EfCitationRepository : ICitationRepository
{
    private readonly AppDbContext _dbContext;

    public EfCitationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Citation>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var citationIds = await _dbContext.IncidentCitationLinks
            .Where(l => l.IncidentId == incidentId)
            .Select(l => l.CitationId)
            .ToListAsync(cancellationToken);

        return await _dbContext.Citations
            .Where(c => citationIds.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }
}
