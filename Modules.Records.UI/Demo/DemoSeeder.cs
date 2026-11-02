using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Demo;

/// <summary>
/// Idempotent startup seeder for the public demo account.
/// Creates the demo jurisdiction, agency, the demo admin, the passwordless
/// public demo user (Supervisor by default), and a curated sample dataset
/// (<see cref="DemoDataset"/>) so the "Try the demo" experience has realistic
/// records to explore. Safe to run on every boot: every step checks for
/// existence before writing.
/// </summary>
public static class DemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var sp = scope.ServiceProvider;

            var authDb = sp.GetRequiredService<AuthDbContext>();
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = sp.GetRequiredService<IConfiguration>();

            await EnsureRoleAsync(roleMgr, "Admin");
            await EnsureRoleAsync(roleMgr, DemoUserDefaults.Role);

            // One-time cleanup: the demo user used to carry a read-only "Demo"
            // role enforced by a write guard. That concept is gone. Strip any
            // lingering "Demo" assignments and drop the role so it no longer
            // appears as assignable. Idempotent: no-ops once the role is gone.
            await RemoveLegacyDemoRoleAsync(roleMgr, userMgr);

            var jurisdiction = await EnsureJurisdictionAsync(authDb);
            var agency = await EnsureAgencyAsync(authDb, jurisdiction.Id);

            // The public demo account gets the Supervisor role when it is created,
            // or when it has no role at all (older builds seeded it role-less, which
            // left visitors on an empty screen). Any role an admin has since chosen
            // in /admin/users is left alone.
            var demoUser = await EnsureUserAsync(
                userMgr,
                email: DemoUserDefaults.Email,
                password: DemoUserDefaults.Password,
                firstName: DemoUserDefaults.FirstName,
                lastName: DemoUserDefaults.LastName,
                role: DemoUserDefaults.Role,
                roleOnlyIfNone: true,
                jurisdictionId: jurisdiction.Id,
                agencyId: agency.Id);
            await EnsureUserAgencyAsync(authDb, demoUser.Id, agency.Id);

            var configuredAdminPassword = configuration[DemoUserDefaults.AdminPasswordConfigKey];
            var adminPassword = string.IsNullOrWhiteSpace(configuredAdminPassword)
                ? GenerateUnguessablePassword()
                : configuredAdminPassword;

            var adminUser = await EnsureUserAsync(
                userMgr,
                email: DemoUserDefaults.AdminEmail,
                password: adminPassword,
                firstName: DemoUserDefaults.AdminFirstName,
                lastName: DemoUserDefaults.AdminLastName,
                role: "Admin",
                roleOnlyIfNone: false,
                jurisdictionId: jurisdiction.Id,
                agencyId: agency.Id);
            await EnsureUserAgencyAsync(authDb, adminUser.Id, agency.Id);
            await SyncAdminPasswordAsync(userMgr, adminUser, configuredAdminPassword, logger);

            // Sample data is best-effort and isolated from identity setup —
            // failures here must not block sign-in.
            try
            {
                await DemoDataset.SeedAsync(sp, adminUser, jurisdiction.Id, agency.Id, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Demo sample record seeding failed; demo user can still sign in.");
            }

            logger.LogInformation(
                "Demo account ready: user={Email}, admin={AdminEmail}, agency={Agency}, jurisdiction={Jurisdiction}.",
                DemoUserDefaults.Email, DemoUserDefaults.AdminEmail, agency.Name, jurisdiction.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo seeding failed; the Try-the-demo button will not work until this is resolved.");
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleMgr, string roleName)
    {
        if (!await roleMgr.RoleExistsAsync(roleName))
            await roleMgr.CreateAsync(new IdentityRole(roleName));
    }

    /// <summary>
    /// Removes the obsolete read-only "Demo" role: unassigns it from every user
    /// (the public demo account in particular) and deletes the role itself.
    /// Safe to call repeatedly — once the role is gone this is a no-op.
    /// </summary>
    private static async Task RemoveLegacyDemoRoleAsync(
        RoleManager<IdentityRole> roleMgr, UserManager<ApplicationUser> userMgr)
    {
        const string legacyDemoRole = "Demo";

        var role = await roleMgr.FindByNameAsync(legacyDemoRole);
        if (role is null) return;

        foreach (var user in await userMgr.GetUsersInRoleAsync(legacyDemoRole))
            await userMgr.RemoveFromRoleAsync(user, legacyDemoRole);

        await roleMgr.DeleteAsync(role);
    }

    private static async Task<Jurisdiction> EnsureJurisdictionAsync(AuthDbContext authDb)
    {
        var existing = await authDb.Jurisdictions
            .FirstOrDefaultAsync(j => j.Code == DemoUserDefaults.JurisdictionCode);
        if (existing is not null) return existing;

        var jurisdiction = Jurisdiction.Create(
            DemoUserDefaults.JurisdictionName,
            DemoUserDefaults.JurisdictionState,
            DemoUserDefaults.JurisdictionCode);
        authDb.Jurisdictions.Add(jurisdiction);
        await authDb.SaveChangesAsync();
        return jurisdiction;
    }

    private static async Task<Agency> EnsureAgencyAsync(AuthDbContext authDb, Guid jurisdictionId)
    {
        var existing = await authDb.Agencies
            .FirstOrDefaultAsync(a => a.JurisdictionId == jurisdictionId && a.Code == DemoUserDefaults.AgencyCode);
        if (existing is not null) return existing;

        var agency = Agency.Create(jurisdictionId, DemoUserDefaults.AgencyName, DemoUserDefaults.AgencyCode);
        authDb.Agencies.Add(agency);
        await authDb.SaveChangesAsync();
        return agency;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userMgr,
        string email,
        string password,
        string firstName,
        string lastName,
        string? role,
        bool roleOnlyIfNone,
        Guid jurisdictionId,
        Guid agencyId)
    {
        var existing = await userMgr.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Repair tenant pointers in case agency/jurisdiction was re-seeded.
            // Password is intentionally not reset — admins may have changed it.
            if (existing.JurisdictionId != jurisdictionId || existing.AgencyId != agencyId)
            {
                existing.JurisdictionId = jurisdictionId;
                existing.AgencyId = agencyId;
                await userMgr.UpdateAsync(existing);
            }
            // Roles are only ensured, never removed — an admin may have changed
            // them in /admin/users. With roleOnlyIfNone the role is a default for
            // an account that has no role at all, not a requirement.
            if (role is not null && !await userMgr.IsInRoleAsync(existing, role))
            {
                var hasAnyRole = (await userMgr.GetRolesAsync(existing)).Count > 0;
                if (!roleOnlyIfNone || !hasAnyRole)
                    await userMgr.AddToRoleAsync(existing, role);
            }
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
        };

        var create = await userMgr.CreateAsync(user, password);
        if (!create.Succeeded)
            throw new InvalidOperationException(
                $"Could not create user '{email}': " + string.Join("; ", create.Errors.Select(e => e.Description)));

        if (role is not null)
            await userMgr.AddToRoleAsync(user, role);
        return (await userMgr.FindByEmailAsync(email))!;
    }

    private static async Task EnsureUserAgencyAsync(AuthDbContext authDb, string userId, Guid agencyId)
    {
        var exists = await authDb.UserAgencies.AnyAsync(ua => ua.UserId == userId && ua.AgencyId == agencyId);
        if (exists) return;

        authDb.UserAgencies.Add(ApplicationUserAgency.Create(userId, agencyId));
        await authDb.SaveChangesAsync();
    }

    /// <summary>
    /// Keeps the demo admin's password out of source control. When
    /// <c>Demo:AdminPassword</c> is configured, the account's password follows
    /// it (checked on every start). When it is not, any account still using the
    /// old, publicly known default gets a random password instead, so the
    /// account can only be used after an operator sets the config value.
    /// </summary>
    private static async Task SyncAdminPasswordAsync(
        UserManager<ApplicationUser> userMgr,
        ApplicationUser admin,
        string? configuredPassword,
        ILogger logger)
    {
        string? target = null;
        if (!string.IsNullOrWhiteSpace(configuredPassword))
        {
            if (!await userMgr.CheckPasswordAsync(admin, configuredPassword))
                target = configuredPassword;
        }
        else if (await userMgr.CheckPasswordAsync(admin, DemoUserDefaults.LegacyAdminPassword))
        {
            target = GenerateUnguessablePassword();
        }

        if (target is null) return;

        // Validate first so a bad configured password can't strip the existing one.
        foreach (var validator in userMgr.PasswordValidators)
        {
            var check = await validator.ValidateAsync(userMgr, admin, target);
            if (!check.Succeeded)
            {
                logger.LogWarning(
                    "Demo:AdminPassword does not meet the password rules ({Errors}); the demo admin password was not changed.",
                    string.Join("; ", check.Errors.Select(e => e.Description)));
                return;
            }
        }

        await userMgr.RemovePasswordAsync(admin);
        var result = await userMgr.AddPasswordAsync(admin, target);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Could not set the demo admin password: " + string.Join("; ", result.Errors.Select(e => e.Description)));

        logger.LogInformation(string.IsNullOrWhiteSpace(configuredPassword)
            ? "Replaced the demo admin's legacy default password with a random one. Set Demo__AdminPassword to sign in as {Email}."
            : "Demo admin password for {Email} updated from Demo:AdminPassword.", admin.Email);
    }

    /// <summary>Random password that satisfies the Identity password rules.</summary>
    private static string GenerateUnguessablePassword()
        => Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)) + "aA1!";
}
