using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Abstractions;

public interface ICitationRepository
{
    Task<IReadOnlyList<Citation>> GetByIncidentIdAsync(
        Guid incidentId,
        CancellationToken cancellationToken);
}
