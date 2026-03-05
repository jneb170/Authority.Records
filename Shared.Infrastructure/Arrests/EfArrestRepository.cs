using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Arrests;

public sealed class EfArrestRepository : IArrestRepository
{
    private readonly AppDbContext _dbContext;

    public EfArrestRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Arrest>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Arrests
            .Where(a => a.IncidentId == incidentId)
            .ToListAsync(cancellationToken);
    }
}
