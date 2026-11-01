using Microsoft.AspNetCore.Identity;
using Shared.Infrastructure.Identity;

namespace Modules.Records.UI.Setup;

/// <summary>
/// Opt-in, idempotent creation of the first Super user and the standard role set on a fresh
/// production database (development uses the seeding in Program.cs instead).
/// <para>
/// Runs only when both <c>Bootstrap:SuperUserEmail</c> and <c>Bootstrap:SuperUserPassword</c>
/// are configured (e.g. <c>Bootstrap__SuperUserEmail</c> / <c>Bootstrap__SuperUserPassword</c>
/// in the host's app.env). Once any user holds the Super role it does nothing, so it never
/// resets a password or creates a second account; remove the settings after first sign-in.
/// </para>
/// </summary>
public static class SuperUserBootstrapper
{
    public const string EmailConfigKey = "Bootstrap:SuperUserEmail";
    public const string PasswordConfigKey = "Bootstrap:SuperUserPassword";

    /// <summary>Roles the admin UI can assign; it lists whatever roles exist in the database.</summary>
    public static readonly IReadOnlyList<string> StandardRoles =
        ["Super", "Admin", "Supervisor", "Officer", "Dispatcher"];

    /// <summary>Best-effort: failures are logged and never block startup.</summary>
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        try
        {
            await BootstrapAsync(services, configuration, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Super user bootstrap failed.");
        }
    }

    private static async Task BootstrapAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var email = configuration[EmailConfigKey];
        var password = configuration[PasswordConfigKey];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        await using var scope = services.CreateAsyncScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in StandardRoles)
        {
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));
        }

        if ((await userMgr.GetUsersInRoleAsync("Super")).Count > 0)
        {
            logger.LogInformation(
                "A Super user already exists; skipping bootstrap. The Bootstrap__* settings can be removed.");
            return;
        }

        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Super",
                IsActive = true,
            };

            var created = await userMgr.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                logger.LogError(
                    "Super user bootstrap failed: {Errors}",
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }
        }

        var added = await userMgr.AddToRoleAsync(user, "Super");
        if (!added.Succeeded)
        {
            logger.LogError(
                "Super user bootstrap could not assign the Super role: {Errors}",
                string.Join("; ", added.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogWarning(
            "Bootstrapped Super user {Email}. Sign in, then remove the Bootstrap__* settings.", email);
    }
}
