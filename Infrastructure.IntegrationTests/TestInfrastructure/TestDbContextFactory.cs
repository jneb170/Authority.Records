using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Persistence;
using Infrastructure.IntegrationTests.Common;
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
        services.AddSingleton<ITenantProvider>(new TestTenantProvider(tenantId));
        // Register fake domain event dispatcher that doesn't actually dispatch events
        services.AddSingleton<IDomainEventDispatcher, TestDomainEventDispatcher>();

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



