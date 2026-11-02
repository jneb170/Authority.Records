using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Common;
using Modules.Records.UI.Demo;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Tests.Demo;

/// <summary>
/// Per-visitor demo accounts (issues #90, #91, #92): each "Explore the live demo" click gets its own user,
/// so create quotas, record locks and the audit trail stop being shared between strangers.
/// </summary>
public sealed class DemoVisitorAccountsTests : IDisposable
{
    private readonly MutableTimeProvider _time = new(new DateTimeOffset(2026, 11, 2, 12, 0, 0, TimeSpan.Zero));
    private readonly DemoVisitorOptions _options = new() { MaxNewAccountsPerHour = 3, RetentionDays = 14 };
    private readonly IdentityTestHost _host;
    private readonly Guid _jurisdictionId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();
    private readonly Guid _secondAgencyId = Guid.NewGuid();

    public DemoVisitorAccountsTests()
    {
        _host = new IdentityTestHost(configure: services =>
        {
            services.AddSingleton<TimeProvider>(_time);
            services.AddSingleton(Options.Create(_options));
            services.AddScoped<DemoVisitorAccounts>();
        });
    }

    [Fact]
    public async Task CreateVisitor_CopiesTenantRolesAndAgencies_AndMarksAccountAsDemo()
    {
        var template = await CreateTemplateAsync();

        using var scope = _host.Provider.CreateScope();
        var visitor = await Accounts(scope).CreateVisitorAsync(template);

        Assert.NotNull(visitor);
        Assert.NotEqual(template.Id, visitor!.Id);
        Assert.Matches(@"^Demo Visitor \d{4}$", visitor.FullName);
        Assert.Equal(_jurisdictionId, visitor.JurisdictionId);
        Assert.Equal(_agencyId, visitor.AgencyId);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Equal(["Supervisor"], await users.GetRolesAsync(visitor));
        Assert.False(await users.HasPasswordAsync(visitor));

        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var agencies = await db.UserAgencies.Where(ua => ua.UserId == visitor.Id).Select(ua => ua.AgencyId).ToListAsync();
        Assert.Equal(new[] { _agencyId, _secondAgencyId }.OrderBy(x => x), agencies.OrderBy(x => x));

        // The principal the sign-in builds is recognised as a demo user, and carries the display name.
        var principal = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>().CreateAsync(visitor);
        Assert.True(DemoUserMatching.IsDemoPrincipal(principal, "demo@example.test"));
        Assert.Equal(visitor.FullName, principal.FindFirst(RecordsUserClaimsPrincipalFactory.FullNameClaimType)?.Value);
    }

    [Fact]
    public async Task EachVisitor_GetsADistinctUser()
    {
        var template = await CreateTemplateAsync();

        using var scope = _host.Provider.CreateScope();
        var a = await Accounts(scope).CreateVisitorAsync(template);
        var b = await Accounts(scope).CreateVisitorAsync(template);

        Assert.NotNull(a);
        Assert.NotNull(b);
        Assert.NotEqual(a!.Id, b!.Id);
        Assert.NotEqual(a.Email, b.Email);
    }

    [Fact]
    public async Task CreateVisitor_NeverCopiesTheSuperRole()
    {
        var template = await CreateTemplateAsync();
        using (var scope = _host.Provider.CreateScope())
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roles.CreateAsync(new IdentityRole("Super"));
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await users.AddToRoleAsync((await users.FindByIdAsync(template.Id))!, "Super");
        }

        using var s = _host.Provider.CreateScope();
        var visitor = await Accounts(s).CreateVisitorAsync(template);
        var visitorRoles = await s.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().GetRolesAsync(visitor!);

        Assert.DoesNotContain("Super", visitorRoles);
        Assert.Contains("Supervisor", visitorRoles);
    }

    [Fact]
    public async Task CreateVisitor_ReturnsNull_WhenHourlyCapIsReached_AndResumesAfterAnHour()
    {
        var template = await CreateTemplateAsync();

        using var scope = _host.Provider.CreateScope();
        for (var i = 0; i < _options.MaxNewAccountsPerHour; i++)
            Assert.NotNull(await Accounts(scope).CreateVisitorAsync(template));

        Assert.Null(await Accounts(scope).CreateVisitorAsync(template));

        _time.Advance(TimeSpan.FromMinutes(61));
        Assert.NotNull(await Accounts(scope).CreateVisitorAsync(template));
    }

    [Fact]
    public async Task DeleteExpired_RemovesOnlyVisitorsPastRetention()
    {
        var template = await CreateTemplateAsync();
        ApplicationUser old, recent;

        using (var scope = _host.Provider.CreateScope())
            old = (await Accounts(scope).CreateVisitorAsync(template))!;

        _time.Advance(TimeSpan.FromDays(10));
        using (var scope = _host.Provider.CreateScope())
            recent = (await Accounts(scope).CreateVisitorAsync(template))!;

        _time.Advance(TimeSpan.FromDays(5)); // old is 15 days old, recent is 5
        using (var scope = _host.Provider.CreateScope())
        {
            Assert.Equal(1, await Accounts(scope).DeleteExpiredAsync());

            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.Null(await users.FindByIdAsync(old.Id));
            Assert.NotNull(await users.FindByIdAsync(recent.Id));
            Assert.NotNull(await users.FindByIdAsync(template.Id)); // the shared demo account is never deleted

            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            Assert.False(await db.UserAgencies.AnyAsync(ua => ua.UserId == old.Id));
        }
    }

    private static DemoVisitorAccounts Accounts(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<DemoVisitorAccounts>();

    private async Task<ApplicationUser> CreateTemplateAsync()
    {
        var template = await _host.CreateUserAsync("demo@example.test", "Supervisor", _jurisdictionId, _agencyId);
        using var scope = _host.Provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        db.UserAgencies.Add(ApplicationUserAgency.Create(template.Id, _agencyId));
        db.UserAgencies.Add(ApplicationUserAgency.Create(template.Id, _secondAgencyId));
        await db.SaveChangesAsync();
        return template;
    }

    public void Dispose() => _host.Dispose();

    private sealed class MutableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now += by;
    }
}
