using Shared.Infrastructure.Identity;

namespace Modules.Records.UI.Services;

public interface IJurisdictionManagementService
{
    Task<List<Jurisdiction>> GetAllAsync();
    Task<Jurisdiction?> GetByIdAsync(Guid id);
    Task<Jurisdiction> CreateAsync(string name, string state, string code);
    Task<bool> UpdateAsync(Guid id, string name, string state, string code);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}
