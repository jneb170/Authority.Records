using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Records.Application;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Mugshots;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Modules.Records.UI.Endpoints;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Mugshots;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.UI.Tests.Mugshots;

/// <summary>
/// End-to-end tests of the authorized mugshot endpoint over a test server. Everything below
/// the endpoint is the production code: the MediatR pipeline, the query handler,
/// <see cref="AppDbContext"/> with its tenant query filters (in-memory SQLite), the
/// claims-based <see cref="HttpTenantProvider"/>, and <see cref="LocalMugshotStorageService"/>
/// on a temporary directory. Only sign-in is replaced, by a header-driven test scheme.
/// </summary>
public sealed class MugshotEndpointTests : IAsyncLifetime
{
    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private readonly Guid _jurisdictionA = Guid.NewGuid();
    private readonly Guid _jurisdictionB = Guid.NewGuid();
    private readonly Guid _agencyA = Guid.NewGuid();

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly string _contentRoot = Directory.CreateTempSubdirectory("mugshot-endpoint-tests-").FullName;
    private WebApplication _app = default!;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        _connection.Open();
        Directory.CreateDirectory(Path.Combine(_contentRoot, "wwwroot"));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ContentRootPath = _contentRoot,
            WebRootPath = Path.Combine(_contentRoot, "wwwroot"),
        });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        var services = builder.Services;
        services.AddHttpContextAccessor();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));
        services.AddTransient<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITenantProvider, HttpTenantProvider>();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();
        services.AddApplication();
        services.Configure<MugshotStorageOptions>(o => o.StoragePath = "mugshot-store");
        services.AddScoped<IMugshotStorageService, LocalMugshotStorageService>();
        services.AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        services.AddAuthorization();

        _app = builder.Build();
        _app.BlockLegacyUploadPaths();
        _app.UseStaticFiles();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapMugshotEndpoints();

        await _app.StartAsync();
        _client = _app.GetTestClient();

        await using var db = CreateSeedContext(_jurisdictionA);
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task NotSignedIn_IsRejected()
    {
        var mugshotId = await SeedMugshotAsync(_jurisdictionA);

        var response = await _client.GetAsync(MugshotUrls.For(mugshotId));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(PngBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task UserFromOtherTenant_Gets404()
    {
        var mugshotId = await SeedMugshotAsync(_jurisdictionA);

        var response = await GetAsUserAsync(MugshotUrls.For(mugshotId), _jurisdictionB);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserFromOwningTenant_GetsImage()
    {
        var mugshotId = await SeedMugshotAsync(_jurisdictionA);

        var response = await GetAsUserAsync(MugshotUrls.For(mugshotId), _jurisdictionA);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(PngBytes, await response.Content.ReadAsByteArrayAsync());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Contains("nosniff", response.Headers.GetValues("X-Content-Type-Options"));
    }

    [Fact]
    public async Task UnknownMugshot_Gets404()
    {
        var response = await GetAsUserAsync(MugshotUrls.For(Guid.NewGuid()), _jurisdictionA);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SoftDeletedMugshot_Gets404()
    {
        var mugshotId = await SeedMugshotAsync(_jurisdictionA);
        await using (var db = CreateSeedContext(_jurisdictionA))
        {
            var mugshot = await db.Mugshots.SingleAsync(m => m.Id == mugshotId);
            mugshot.SoftDelete(Guid.NewGuid());
            await db.SaveChangesAsync();
        }

        var response = await GetAsUserAsync(MugshotUrls.For(mugshotId), _jurisdictionA);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MugshotUploadedBeforeTheChange_IsServedFromLegacyLocation()
    {
        // As written by the old storage service: key prefixed uploads/mugshots/, file in wwwroot.
        var relative = $"{_jurisdictionA:N}/2026/03/{Guid.NewGuid():N}.png";
        var legacyFile = Path.Combine(_contentRoot, "wwwroot", "uploads", "mugshots", relative);
        Directory.CreateDirectory(Path.GetDirectoryName(legacyFile)!);
        await File.WriteAllBytesAsync(legacyFile, PngBytes);
        var mugshotId = await SeedMugshotRowAsync(_jurisdictionA, "uploads/mugshots/" + relative);

        var viaEndpoint = await GetAsUserAsync(MugshotUrls.For(mugshotId), _jurisdictionA);
        var viaOldStaticPath = await GetAsUserAsync("/uploads/mugshots/" + relative, _jurisdictionA);

        Assert.Equal(HttpStatusCode.OK, viaEndpoint.StatusCode);
        Assert.Equal(PngBytes, await viaEndpoint.Content.ReadAsByteArrayAsync());
        Assert.Equal(HttpStatusCode.NotFound, viaOldStaticPath.StatusCode);
    }

    [Fact]
    public async Task NewUploads_AreNotStoredUnderWebRoot()
    {
        await SeedMugshotAsync(_jurisdictionA);

        Assert.Empty(Directory.EnumerateFiles(Path.Combine(_contentRoot, "wwwroot"), "*", SearchOption.AllDirectories));
        Assert.Single(Directory.EnumerateFiles(Path.Combine(_contentRoot, "mugshot-store"), "*", SearchOption.AllDirectories));
    }

    private async Task<Guid> SeedMugshotAsync(Guid jurisdictionId)
    {
        var mugshotId = Guid.NewGuid();
        using var scope = _app.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IMugshotStorageService>();
        var saved = await storage.SaveAsync(jurisdictionId, mugshotId, PngBytes, "photo.png", "image/png", CancellationToken.None);

        return await SeedMugshotRowAsync(jurisdictionId, saved.StoragePath, mugshotId);
    }

    private async Task<Guid> SeedMugshotRowAsync(Guid jurisdictionId, string storagePath, Guid? id = null)
    {
        var mugshotId = id ?? Guid.NewGuid();
        await using var db = CreateSeedContext(jurisdictionId);
        db.Mugshots.Add(new Mugshot(
            mugshotId, jurisdictionId, _agencyA, "photo.png", "image/png", PngBytes.Length,
            storagePath, MugshotUrls.For(mugshotId), DateTime.UtcNow));
        await db.SaveChangesAsync();
        return mugshotId;
    }

    private AppDbContext CreateSeedContext(Guid jurisdictionId) => new(
        new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options,
        new FixedTenantProvider(jurisdictionId, _agencyA),
        new NoOpDomainEventDispatcher());

    private Task<HttpResponseMessage> GetAsUserAsync(string url, Guid jurisdictionId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add(TestAuthHandler.JurisdictionHeader, jurisdictionId.ToString());
        request.Headers.Add(TestAuthHandler.AgencyHeader, _agencyA.ToString());
        return _client.SendAsync(request);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        _connection.Dispose();
        Directory.Delete(_contentRoot, recursive: true);
    }

    /// <summary>Signs the request in when the test headers are present; otherwise anonymous.</summary>
    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";
        public const string JurisdictionHeader = "X-Test-Jurisdiction";
        public const string AgencyHeader = "X-Test-Agency";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(JurisdictionHeader, out var jurisdiction))
                return Task.FromResult(AuthenticateResult.NoResult());

            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim("jurisdiction", jurisdiction.ToString()),
                new Claim("agency", Request.Headers[AgencyHeader].ToString()),
            ], SchemeName);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private sealed class FixedTenantProvider(Guid jurisdictionId, Guid agencyId) : ITenantProvider
    {
        private Guid _jurisdictionId = jurisdictionId;
        public Guid GetJurisdictionId() => _jurisdictionId;
        public Guid GetAgencyId() => agencyId;
        public Guid GetUserId() => Guid.Empty;
        public void SetJurisdictionId(Guid jurisdictionId) => _jurisdictionId = jurisdictionId;
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
