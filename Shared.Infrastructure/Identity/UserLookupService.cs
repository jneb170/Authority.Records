using Microsoft.AspNetCore.Identity;
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
        if (userId == Guid.Empty)
            return null;

        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email ?? user?.UserName;
    }
}
