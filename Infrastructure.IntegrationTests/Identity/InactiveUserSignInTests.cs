using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Identity;

/// <summary>
/// Exercises the production Identity wiring (<see cref="DependencyInjection.AddRecordsIdentity"/>)
/// against an in-memory SQLite <see cref="AuthDbContext"/>: deactivated users
/// (<see cref="ApplicationUser.IsActive"/> = false) can neither sign in nor keep using an
/// existing auth cookie.
/// </summary>
public sealed class InactiveUserSignInTests : IDisposable
{
    private const string Password = "Passw0rd!";

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public InactiveUserSignInTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddDbContext<AuthDbContext>(o => o.UseSqlite(_connection));
        services.AddRecordsIdentity();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task ActiveUser_WithCorrectPassword_SignsIn()
    {
        await CreateUserAsync("active@example.com", isActive: true);

        using var scope = CreateRequestScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        var result = await signInManager.PasswordSignInAsync(
            "active@example.com", Password, isPersistent: false, lockoutOnFailure: false);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task InactiveUser_WithCorrectPassword_IsNotAllowedToSignIn()
    {
        await CreateUserAsync("inactive@example.com", isActive: false);

        using var scope = CreateRequestScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        var result = await signInManager.PasswordSignInAsync(
            "inactive@example.com", Password, isPersistent: false, lockoutOnFailure: false);

        Assert.False(result.Succeeded);
        Assert.True(result.IsNotAllowed);
    }

    [Fact]
    public async Task InactiveUser_CannotSignIn_ViaPasswordlessPath()
    {
        // The demo button uses SignInAsync, which skips pre-sign-in checks; Login.cshtml.cs
        // gates it with CanSignInAsync.
        var user = await CreateUserAsync("demo@example.com", isActive: false);

        using var scope = CreateRequestScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        Assert.False(await signInManager.CanSignInAsync(user));
    }

    [Fact]
    public async Task DeactivatingUser_InvalidatesExistingCookie()
    {
        var user = await CreateUserAsync("deactivated@example.com", isActive: true);
        var principal = await CreatePrincipalAsync(user.Id);

        Assert.True(await CookieIsValidAsync(principal));

        // Same steps as UserManagementService.UpdateAsync when deactivating.
        using (var scope = _provider.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var stored = await userManager.FindByIdAsync(user.Id);
            stored!.IsActive = false;
            Assert.True((await userManager.UpdateSecurityStampAsync(stored)).Succeeded);
        }

        Assert.False(await CookieIsValidAsync(principal));
    }

    [Fact]
    public async Task InactiveUser_WithUnchangedSecurityStamp_CookieIsStillRejected()
    {
        var user = await CreateUserAsync("flagonly@example.com", isActive: true);
        var principal = await CreatePrincipalAsync(user.Id);

        // Flip the flag without rotating the stamp (e.g. edited directly in the database).
        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var stored = await db.Users.SingleAsync(u => u.Id == user.Id);
            stored.IsActive = false;
            await db.SaveChangesAsync();
        }

        Assert.False(await CookieIsValidAsync(principal));
    }

    private async Task<ApplicationUser> CreateUserAsync(string email, bool isActive)
    {
        using var scope = _provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive,
            JurisdictionId = Guid.NewGuid(),
            AgencyId = Guid.NewGuid(),
        };

        var result = await userManager.CreateAsync(user, Password);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
        return user;
    }

    private async Task<ClaimsPrincipal> CreatePrincipalAsync(string userId)
    {
        using var scope = _provider.CreateScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var user = await signInManager.UserManager.FindByIdAsync(userId);
        return await signInManager.CreateUserPrincipalAsync(user!);
    }

    /// <summary>
    /// Runs the cookie's configured OnValidatePrincipal handler, as the cookie middleware
    /// would on a request, with a ticket old enough to be due for validation.
    /// </summary>
    private async Task<bool> CookieIsValidAsync(ClaimsPrincipal principal)
    {
        using var scope = CreateRequestScope();
        var httpContext = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext!;

        var schemes = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await schemes.GetSchemeAsync(IdentityConstants.ApplicationScheme);
        var cookieOptions = scope.ServiceProvider
            .GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);

        var issued = DateTimeOffset.UtcNow - ActiveUserSecurityStampValidator.ValidationInterval - TimeSpan.FromSeconds(1);
        var properties = new AuthenticationProperties { IssuedUtc = issued, ExpiresUtc = issued.AddHours(8) };
        var ticket = new AuthenticationTicket(principal, properties, IdentityConstants.ApplicationScheme);

        var context = new CookieValidatePrincipalContext(httpContext, scheme!, cookieOptions, ticket);
        await cookieOptions.Events.ValidatePrincipal(context);

        return context.Principal is not null;
    }

    private IServiceScope CreateRequestScope()
    {
        var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        return scope;
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
