using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;

namespace Shared.Infrastructure.Identity;

public sealed class UserLookupService : IUserLookupService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserLookupService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetDisplayNameAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty) return null;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        var fullName = user.FullName;
        return string.IsNullOrWhiteSpace(fullName) ? (user.Email ?? user.UserName) : fullName;
    }

    public async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.Where(id => id != Guid.Empty).Distinct().Select(id => id.ToString()).ToList();
        if (ids.Count == 0) return [];

        var users = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return users.ToDictionary(
            u => Guid.Parse(u.Id),
            u =>
            {
                var fullName = u.FullName;
                return string.IsNullOrWhiteSpace(fullName) ? (u.Email ?? u.UserName ?? u.Id) : fullName;
            });
    }
}
