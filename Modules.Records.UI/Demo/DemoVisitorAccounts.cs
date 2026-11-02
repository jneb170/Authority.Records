using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Common;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Demo;

/// <summary>
/// Settings for per-visitor demo accounts (<c>Demo:Visitors</c>; env vars <c>Demo__Visitors__*</c>).
/// </summary>
public sealed class DemoVisitorOptions
{
    public const string SectionName = "Demo:Visitors";

    /// <summary>
    /// When true (default), each "Explore the live demo" click signs in a fresh account copied from the demo
    /// account (same agency and roles). When false, every visitor shares the demo account, as before.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Max visitor accounts created per hour across the whole site. Beyond it, visitors share the demo
    /// account until the hour rolls over, so a bot can't fill the user table. 0 disables the cap.
    /// </summary>
    public int MaxNewAccountsPerHour { get; set; } = 120;

    /// <summary>
    /// Visitor accounts are deleted this many days after they were created (sessions last 12 hours, see
    /// <see cref="DemoUserDefaults.SessionLifetime"/>). Records they created stay; their "by" name then
    /// shows as unknown.
    /// </summary>
    public int RetentionDays { get; set; } = 14;
}

/// <summary>
/// Creates and cleans up per-visitor demo accounts (issues #90, #91, #92). Every visitor used to sign in as the
/// same user, so they shared one create quota, one password, and one lock owner (record locking could not be
/// demonstrated, and two strangers could edit the same record at once).
/// </summary>
/// <remarks>
/// A visitor account is a copy of the demo account's tenant (jurisdiction, agency, agency memberships) and
/// roles, with no password, marked with the <see cref="DemoUserMatching.DemoVisitorClaimType"/> user claim. The
/// claim value is the UTC creation time (round-trip "O" format, so values sort as strings), used for the hourly
/// cap and for retention.
/// </remarks>
public sealed class DemoVisitorAccounts(
    UserManager<ApplicationUser> userManager,
    AuthDbContext authDb,
    IOptions<DemoVisitorOptions> options,
    TimeProvider timeProvider,
    ILogger<DemoVisitorAccounts> logger)
{
    private readonly DemoVisitorOptions _options = options.Value;

    public bool Enabled => _options.Enabled;

    /// <summary>
    /// Creates a visitor account based on <paramref name="template"/>, or returns null when the hourly cap is
    /// reached or creation fails (callers then fall back to the shared demo account).
    /// </summary>
    public async Task<ApplicationUser?> CreateVisitorAsync(ApplicationUser template, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (_options.MaxNewAccountsPerHour > 0)
        {
            var cutoff = FormatTimestamp(now.AddHours(-1));
            var createdLastHour = await authDb.UserClaims
                .CountAsync(c => c.ClaimType == DemoUserMatching.DemoVisitorClaimType
                                 && string.Compare(c.ClaimValue, cutoff) >= 0, cancellationToken);

            if (createdLastHour >= _options.MaxNewAccountsPerHour)
            {
                logger.LogWarning(
                    "Demo visitor account cap reached ({Count} in the last hour); signing the visitor in to the shared demo account.",
                    createdLastHour);
                return null;
            }
        }

        var number = RandomNumberGenerator.GetInt32(1000, 10000);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToLowerInvariant();
        var email = $"visitor-{number}-{token}@{DemoUserDefaults.VisitorEmailDomain}";

        var visitor = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = DemoUserDefaults.VisitorFirstName,
            LastName = $"{DemoUserDefaults.VisitorLastNamePrefix} {number}",
            IsActive = true,
            JurisdictionId = template.JurisdictionId,
            AgencyId = template.AgencyId,
        };

        // No password: visitors only ever sign in through the demo button.
        var created = await userManager.CreateAsync(visitor);
        if (!created.Succeeded)
        {
            logger.LogWarning("Could not create a demo visitor account: {Errors}",
                string.Join("; ", created.Errors.Select(e => e.Description)));
            return null;
        }

        try
        {
            var claim = await userManager.AddClaimAsync(visitor, new Claim(DemoUserMatching.DemoVisitorClaimType, FormatTimestamp(now)));
            if (!claim.Succeeded)
                throw new InvalidOperationException(string.Join("; ", claim.Errors.Select(e => e.Description)));

            var roles = await userManager.GetRolesAsync(template);
            if (roles.Count == 0)
                roles = [DemoUserDefaults.Role];
            // Never hand a visitor more than the demo account has, and never platform-wide rights.
            var grant = roles.Where(r => !string.Equals(r, "Super", StringComparison.OrdinalIgnoreCase)).ToList();
            if (grant.Count > 0)
            {
                var added = await userManager.AddToRolesAsync(visitor, grant);
                if (!added.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", added.Errors.Select(e => e.Description)));
            }

            var agencyIds = await authDb.UserAgencies
                .Where(ua => ua.UserId == template.Id)
                .Select(ua => ua.AgencyId)
                .ToListAsync(cancellationToken);
            if (!agencyIds.Contains(template.AgencyId))
                agencyIds.Add(template.AgencyId);

            authDb.UserAgencies.AddRange(agencyIds.Distinct().Select(id => ApplicationUserAgency.Create(visitor.Id, id)));
            await authDb.SaveChangesAsync(cancellationToken);

            return visitor;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not finish setting up demo visitor account {Email}; removing it.", email);
            await DeleteAsync(visitor, cancellationToken);
            return null;
        }
    }

    /// <summary>Deletes visitor accounts older than <see cref="DemoVisitorOptions.RetentionDays"/>. Returns how many.</summary>
    public async Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        if (_options.RetentionDays <= 0)
            return 0;

        var cutoff = FormatTimestamp(timeProvider.GetUtcNow().UtcDateTime.AddDays(-_options.RetentionDays));
        var expiredIds = await authDb.UserClaims
            .Where(c => c.ClaimType == DemoUserMatching.DemoVisitorClaimType && string.Compare(c.ClaimValue, cutoff) < 0)
            .Select(c => c.UserId)
            .Distinct()
            .Take(500)
            .ToListAsync(cancellationToken);

        var deleted = 0;
        foreach (var id in expiredIds)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is not null && await DeleteAsync(user, cancellationToken))
                deleted++;
        }

        return deleted;
    }

    private async Task<bool> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var memberships = await authDb.UserAgencies.Where(ua => ua.UserId == user.Id).ToListAsync(cancellationToken);
        authDb.UserAgencies.RemoveRange(memberships);
        await authDb.SaveChangesAsync(cancellationToken);
        return (await userManager.DeleteAsync(user)).Succeeded;
    }

    internal static string FormatTimestamp(DateTime utc)
        => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);
}

/// <summary>Hourly clean-up of expired demo visitor accounts.</summary>
public sealed class DemoVisitorCleanupService(IServiceScopeFactory scopeFactory, ILogger<DemoVisitorCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let startup (migrations, seeding) finish first.
        try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var accounts = scope.ServiceProvider.GetRequiredService<DemoVisitorAccounts>();
                var deleted = await accounts.DeleteExpiredAsync(stoppingToken);
                if (deleted > 0)
                    logger.LogInformation("Deleted {Count} expired demo visitor accounts.", deleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Demo visitor clean-up failed; will retry in {Interval}.", Interval);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
