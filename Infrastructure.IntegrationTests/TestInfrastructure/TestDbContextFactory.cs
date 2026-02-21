using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.TestInfrastructure;

/// <summary>
/// Factory for creating test database contexts using SQLite in-memory database.
/// This class manages the lifecycle of a SQLite connection and service provider for integration testing.
/// </summary>
public sealed class SqliteTestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTestDbContextFactory"/> class.
    /// Creates an in-memory SQLite database and configures the service provider with fake implementations.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to use for testing multi-tenant scenarios.</param>
    public SqliteTestDbContextFactory(Guid tenantId)
    {
        // Create an in-memory SQLite connection that persists only while the connection is open
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        // Register fake tenant provider for testing
        services.AddSingleton<ITenantProvider>(new FakeTenantProvider(tenantId));
        // Register fake domain event dispatcher that doesn't actually dispatch events
        services.AddSingleton<IDomainEventDispatcher, FakeDomainEventDispatcher>();

        // Configure DbContext to use the in-memory SQLite connection
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_connection));

        _provider = services.BuildServiceProvider();

        // Ensure the database schema is created
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a new instance of <see cref="AppDbContext"/> with a new service scope.
    /// </summary>
    /// <returns>A new database context instance.</returns>
    public AppDbContext CreateContext()
    {
        return _provider.CreateScope()
            .ServiceProvider
            .GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// Disposes the SQLite connection and service provider.
    /// </summary>
    public void Dispose()
    {
        _connection.Dispose();
        _provider.Dispose();
    }
}

/// <summary>
/// Fake implementation of <see cref="ITenantProvider"/> for testing purposes.
/// </summary>
internal class FakeTenantProvider : ITenantProvider
{
    private readonly Guid _tenantId;

    public FakeTenantProvider(Guid tenantId)
    {
        _tenantId = tenantId;
    }

    /// <summary>
    /// Not implemented for test scenarios.
    /// </summary>
    /// <exception cref="NotImplementedException">This method is not used in current tests.</exception>
    public Guid GetAgencyId()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the tenant's jurisdiction identifier.
    /// </summary>
    /// <returns>The jurisdiction identifier set during construction.</returns>
    public Guid GetJurisdictionId() => _tenantId;

    /// <summary>
    /// Not implemented for test scenarios.
    /// </summary>
    /// <exception cref="NotImplementedException">This method is not used in current tests.</exception>
    public Guid GetUserId()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Fake implementation of <see cref="IDomainEventDispatcher"/> that doesn't dispatch events.
/// Used in tests where domain event handling is not the focus.
/// </summary>
internal class FakeDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IDomainEvent domainEvent, 
        CancellationToken cancellationToken = default) 
        => Task.CompletedTask;

    /// <summary>
    /// No-op implementation that completes immediately without dispatching events.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch (ignored).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task DispatchAsync(
        IEnumerable<Modules.Records.Domain.DomainEvents.IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
