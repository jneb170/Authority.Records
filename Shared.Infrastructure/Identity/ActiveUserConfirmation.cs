using Microsoft.AspNetCore.Identity;

namespace Shared.Infrastructure.Identity;

/// <summary>
/// Identity's account-confirmation hook, used here to refuse sign-in for users an
/// administrator has deactivated (<see cref="ApplicationUser.IsActive"/> = false).
/// </summary>
/// <remarks>
/// Only consulted when <c>IdentityOptions.SignIn.RequireConfirmedAccount</c> is true.
/// <see cref="SignInManager{TUser}.CanSignInAsync"/> calls it, so it gates password
/// sign-in (the result is <see cref="SignInResult.NotAllowed"/>) and, through
/// <see cref="ActiveUserSecurityStampValidator"/>, revalidation of existing cookies.
/// It replaces the default confirmation, which would otherwise require a confirmed email.
/// </remarks>
public sealed class ActiveUserConfirmation : IUserConfirmation<ApplicationUser>
{
    public Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        => Task.FromResult(user.IsActive);
}
