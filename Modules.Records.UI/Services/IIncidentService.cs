using Modules.Records.Application.DTOs;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.UI.Services;

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentDto>> GetByJurisdictionAsync();
    Task<IReadOnlyList<IncidentDto>> SearchAsync(string? term);
    Task<IncidentDto?> GetByIdAsync(Guid id);
    Task<IncidentDto?> GetByRecordNumberAsync(long recordNumber);
    Task<long> CreateAsync(IncidentDetails details);
    Task OpenAsync(Guid id);
    Task CloseAsync(Guid id);
    Task ArchiveAsync(Guid id);
    Task UpdateDetailsAsync(Guid id, IncidentDetails details, Guid? locationId = null, DateTime? occurredOn = null);
    Task SavePageAsync(
        Guid id,
        IncidentDetails details,
        Guid? locationId = null,
        DateTime? occurredOn = null,
        IReadOnlyCollection<Guid>? chargeIdsToAdd = null,
        IReadOnlyCollection<Guid>? chargeIdsToRemove = null);
    Task AcquireLockAsync(Guid id);
    Task RenewLockAsync(Guid id);
    Task ReleaseLockAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task RestoreAsync(Guid id);
}
