using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application.Common;
using Modules.Records.UI.Services;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Tests.Demo;

/// <summary>
/// Issue #91: any visitor could change the shared demo password, which rotated the security stamp and
/// signed every other visitor out. The service must refuse it whatever the UI shows.
/// </summary>
public sealed class DemoPasswordChangeTests : IDisposable
{
    private readonly IdentityTestHost _host = new(new() { [DemoUserMatching.LoginEmailConfigKey] = "demo@example.test" });

    [Fact]
    public async Task SharedDemoAccount_CannotChangePassword()
    {
        var demo = await _host.CreateUserAsync("demo@example.test");
        var stampBefore = demo.SecurityStamp;

        var result = await ChangePasswordAsync(demo.Id);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "DemoAccount");
        Assert.Equal(stampBefore, await GetStampAsync(demo.Id));
        Assert.True(await CheckPasswordAsync(demo.Id, IdentityTestHost.Password));
    }

    [Fact]
    public async Task DemoVisitorAccount_CannotChangePassword()
    {
        var visitor = await _host.CreateUserAsync("visitor-1@visitors.example.test");
        using (var scope = _host.Provider.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await users.AddClaimAsync((await users.FindByIdAsync(visitor.Id))!, new Claim(DemoUserMatching.DemoVisitorClaimType, "true"));
        }

        var result = await ChangePasswordAsync(visitor.Id);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "DemoAccount");
    }

    [Fact]
    public async Task RegularAccount_CanStillChangePassword()
    {
        var user = await _host.CreateUserAsync("officer@example.test");

        var result = await ChangePasswordAsync(user.Id);

        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
        Assert.True(await CheckPasswordAsync(user.Id, "N3w-Passw0rd!"));
    }

    private async Task<IdentityResult> ChangePasswordAsync(string userId)
    {
        using var scope = _host.Provider.CreateScope();
        var service = new UserManagementService(
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<AuthDbContext>(),
            scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>(),
            _host.Configuration);
        return await service.ChangePasswordAsync(userId, IdentityTestHost.Password, "N3w-Passw0rd!");
    }

    private async Task<string?> GetStampAsync(string userId)
    {
        using var scope = _host.Provider.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return (await users.FindByIdAsync(userId))!.SecurityStamp;
    }

    private async Task<bool> CheckPasswordAsync(string userId, string password)
    {
        using var scope = _host.Provider.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await users.CheckPasswordAsync((await users.FindByIdAsync(userId))!, password);
    }

    public void Dispose() => _host.Dispose();
}
