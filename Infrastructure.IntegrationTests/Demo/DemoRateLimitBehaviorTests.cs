using Infrastructure.IntegrationTests.Common;
using Infrastructure.IntegrationTests.TestInfrastructure;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common;
using Modules.Records.Application.Common.Behaviors;
using Modules.Records.Application.Common.Exceptions;
using Modules.Records.Application.Incidents.Commands.CreateIncident;
using Modules.Records.Application.Mugshots.Commands.UploadMugshot;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Demo;

/// <summary>
/// Exercises <see cref="DemoRateLimitBehavior{TRequest,TResponse}"/> against a real in-memory
/// SQLite database: the shared public demo account is capped on how many records it can create
/// per rolling window and on how many bytes a single write may carry, while real users are
/// untouched.
/// </summary>
public sealed class DemoRateLimitBehaviorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;
    private readonly Guid _jurisdictionId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();
    private readonly Guid _demoUserId = Guid.NewGuid();

    public DemoRateLimitBehaviorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext, SqliteTestAppDbContext>(o => o.UseSqlite(_connection));
        services.AddScoped<ITenantProvider>(_ => new TestTenantProvider(_jurisdictionId, _agencyId));
        services.AddScoped<IDomainEventDispatcher, TestDomainEventDispatcher>();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task DemoUser_AtCreateLimit_IsRejected()
    {
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 3, WindowMinutes = 60 };
        await SeedIncidentsAsync(_demoUserId, DateTime.UtcNow, count: 3);

        var nextCalled = false;
        var ex = await Assert.ThrowsAsync<DemoLimitExceededException>(() =>
            Handle(SampleCommand(), isDemo: true, options, () => nextCalled = true));

        Assert.Contains("3 new records", ex.Message);
        Assert.False(nextCalled); // rejected before the handler runs
    }

    [Fact]
    public async Task DemoUser_UnderCreateLimit_PassesThrough()
    {
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 3, WindowMinutes = 60 };
        await SeedIncidentsAsync(_demoUserId, DateTime.UtcNow, count: 2);

        var nextCalled = false;
        await Handle(SampleCommand(), isDemo: true, options, () => nextCalled = true);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task DemoUser_OldRecordsOutsideWindow_DoNotCount()
    {
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 3, WindowMinutes = 60 };
        // 5 records, but created 2 hours ago — outside the 60-minute window.
        await SeedIncidentsAsync(_demoUserId, DateTime.UtcNow.AddHours(-2), count: 5);

        var nextCalled = false;
        await Handle(SampleCommand(), isDemo: true, options, () => nextCalled = true);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task NonDemoUser_IsNeverRateLimited()
    {
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 3, WindowMinutes = 60 };
        await SeedIncidentsAsync(_demoUserId, DateTime.UtcNow, count: 10); // well over the cap

        var nextCalled = false;
        await Handle(SampleCommand(), isDemo: false, options, () => nextCalled = true);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task DemoUser_OversizedWrite_IsRejected()
    {
        // High create cap so only the size limit can trip.
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 1000, MaxBytesPerWrite = 512 };
        var huge = SampleCommand(new string('x', 4096)); // ~4 KB description

        var nextCalled = false;
        var ex = await Assert.ThrowsAsync<DemoLimitExceededException>(() =>
            Handle(huge, isDemo: true, options, () => nextCalled = true));

        Assert.Contains("too large", ex.Message);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task NonDemoUser_OversizedWrite_IsAllowed()
    {
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 1000, MaxBytesPerWrite = 512 };
        var huge = SampleCommand(new string('x', 4096));

        var nextCalled = false;
        await Handle(huge, isDemo: false, options, () => nextCalled = true);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task MugshotUpload_IsExemptFromSizeCap_ForDemoUser()
    {
        // The mugshot command carries image bytes well over the text cap, but enforces its
        // own 5 MB / image-type limit, so it opts out of the per-write size cap.
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 1000, MaxBytesPerWrite = 512 };
        var upload = new UploadMugshotCommand("Name", Guid.NewGuid(), "f.jpg", "image/jpeg",
            Content: new byte[4096]);

        var nextCalled = false;
        await HandleUpload(upload, isDemo: true, options, () => nextCalled = true);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task MugshotUpload_CountsAgainstTheCreateCap()
    {
        // Mugshot uploads create a record too, so they share the demo creation bucket.
        var options = new DemoRateLimitOptions { MaxCreatesPerWindow = 3, WindowMinutes = 60 };
        await SeedIncidentsAsync(_demoUserId, DateTime.UtcNow, count: 3); // bucket already full

        var upload = new UploadMugshotCommand("Name", Guid.NewGuid(), "f.jpg", "image/jpeg",
            Content: new byte[4096]);

        var nextCalled = false;
        await Assert.ThrowsAsync<DemoLimitExceededException>(() =>
            HandleUpload(upload, isDemo: true, options, () => nextCalled = true));

        Assert.False(nextCalled);
    }

    private static CreateIncidentCommand SampleCommand(string description = "test") =>
        new(new IncidentDetails { IncidentNum = "INC-1", LocalNum = "L-1", Description = description });

    private Task<long> Handle(
        CreateIncidentCommand command, bool isDemo, DemoRateLimitOptions options, Action onNext)
        => RunBehavior<CreateIncidentCommand, long>(command, 42L, isDemo, options, onNext);

    private Task<Guid> HandleUpload(
        UploadMugshotCommand command, bool isDemo, DemoRateLimitOptions options, Action onNext)
        => RunBehavior<UploadMugshotCommand, Guid>(command, Guid.NewGuid(), isDemo, options, onNext);

    private async Task<TResponse> RunBehavior<TRequest, TResponse>(
        TRequest request, TResponse nextResult, bool isDemo, DemoRateLimitOptions options, Action onNext)
        where TRequest : notnull
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var behavior = new DemoRateLimitBehavior<TRequest, TResponse>(
            new StubCurrentUser(isDemo),
            new FixedTenantProvider(_jurisdictionId, _demoUserId),
            db,
            Options.Create(options));

        return await behavior.Handle(
            request,
            (_) => { onNext(); return Task.FromResult(nextResult); },
            CancellationToken.None);
    }

    private async Task SeedIncidentsAsync(Guid userId, DateTime createdAt, int count)
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var factory = new IncidentFactory();
        var ids = new List<Guid>();
        for (var i = 0; i < count; i++)
        {
            var incident = factory.Create(new CreateIncidentRequest
            {
                JurisdictionId = _jurisdictionId,
                AgencyId = _agencyId,
                Details = new IncidentDetails { IncidentNum = $"INC-{i}", LocalNum = $"L-{i}" }
            });
            db.Incidents.Add(incident);
            ids.Add(incident.Id);
        }

        await db.SaveChangesAsync();

        // The AuditInterceptor isn't wired in this minimal harness, so stamp the audit
        // columns directly (column-level update bypasses the private setters).
        await db.Incidents.IgnoreQueryFilters()
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.CreatedBy, userId)
                .SetProperty(x => x.CreatedAt, createdAt));
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }

    private sealed class StubCurrentUser(bool isDemo) : ICurrentUserContext
    {
        public bool IsInRole(string roleName) => false;
        public bool IsDemoUser { get; } = isDemo;
    }

    private sealed class FixedTenantProvider(Guid jurisdictionId, Guid userId) : ITenantProvider
    {
        public Guid GetAgencyId() => Guid.Empty;
        public Guid GetJurisdictionId() => jurisdictionId;
        public Guid GetUserId() => userId;
        public void SetJurisdictionId(Guid value) { }
    }
}
