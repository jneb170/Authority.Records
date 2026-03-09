using Shared.Infrastructure.Identity;

namespace Modules.Records.UI.Services;

public interface IAgencyManagementService
{
    Task<List<Agency>> GetByJurisdictionAsync(Guid jurisdictionId);
    Task<Agency?> GetByIdAsync(Guid id);
    Task<Agency> CreateAsync(Guid jurisdictionId, string name, string code);
    Task<bool> UpdateAsync(Guid id, string name, string code);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}
