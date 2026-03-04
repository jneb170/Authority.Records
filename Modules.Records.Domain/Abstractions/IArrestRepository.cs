using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Abstractions;

public interface IArrestRepository
{
    Task<IReadOnlyList<Arrest>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken);
}