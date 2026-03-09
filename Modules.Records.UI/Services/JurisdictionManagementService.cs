using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;
using AgencyEntity = Shared.Infrastructure.Identity.Agency;
using JurisdictionEntity = Shared.Infrastructure.Identity.Jurisdiction;

namespace Modules.Records.UI.Services;

public sealed class JurisdictionManagementService(
    AuthDbContext authDb,
    UserManager<ApplicationUser> userManager) : IJurisdictionManagementService
{
    public Task<List<JurisdictionEntity>> GetAllAsync()
        => authDb.Jurisdictions.OrderBy(j => j.Name).ToListAsync();

    public Task<JurisdictionEntity?> GetByIdAsync(Guid id)
        => authDb.Jurisdictions.FirstOrDefaultAsync(j => j.Id == id);

    public async Task<JurisdictionEntity> CreateAsync(string name, string state, string code)
    {
        var jurisdiction = JurisdictionEntity.Create(name, state, code);
        authDb.Jurisdictions.Add(jurisdiction);
        await authDb.SaveChangesAsync();
        return jurisdiction;
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string state, string code)
    {
        var jurisdiction = await authDb.Jurisdictions.FindAsync(id);
        if (jurisdiction is null) return false;
        jurisdiction.Update(name, state, code);
        await authDb.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
        var hasAgencies = await authDb.Agencies.AnyAsync(a => a.JurisdictionId == id);
        if (hasAgencies)
            return (false, "Cannot delete a jurisdiction that has agencies assigned.");

        var hasUsers = await userManager.Users.AnyAsync(u => u.JurisdictionId == id);
        if (hasUsers)
            return (false, "Cannot delete a jurisdiction that has users assigned.");

        var jurisdiction = await authDb.Jurisdictions.FindAsync(id);
        if (jurisdiction is null) return (false, "Jurisdiction not found.");

        authDb.Jurisdictions.Remove(jurisdiction);
        await authDb.SaveChangesAsync();
        return (true, null);
    }
}
