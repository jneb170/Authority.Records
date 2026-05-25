using Infrastructure.IntegrationTests.TestInfrastructure;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Arrests.Commands.CreateArrest;
using Modules.Records.Application.Arrests.Commands.RestoreArrest;
using Modules.Records.Application.Arrests.Commands.SoftDeleteArrest;
using Modules.Records.Application.Arrests.Queries.GetArrestByRecordNumber;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Locations.Commands.CreateLocation;
using Modules.Records.Application.Locations.Commands.RestoreLocation;
using Modules.Records.Application.Locations.Commands.SoftDeleteLocation;
using Modules.Records.Application.Locations.Queries.GetLocationByRecordNumber;
using Modules.Records.Application.Names.Commands.CreateName;
using Modules.Records.Application.Names.Commands.RestoreName;
using Modules.Records.Application.Names.Commands.SoftDeleteName;
using Modules.Records.Application.Names.Queries.GetNameByRecordNumber;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Factories;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Projections;

/// <summary>
/// Real-MediatR-dispatch regression coverage for the soft-delete / restore projection gap that bit
/// Citation (PR #73). Arrest had NO SoftDeleted/Restored projection handler at all; Name and Location
/// removed their read-model row on delete but never rebuilt it on restore. These read models carry no
/// IsDeleted flag and the read queries don't filter one, so the projection must remove-on-delete and
/// rebuild-on-restore. Exercised through the real pipeline so projection handlers actually run —
/// unlike the fake-dispatcher SoftDeleteTests, which never touch projections.
/// </summary>
public abstract class ProjectionHarness : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ServiceProvider Provider;
    protected readonly Guid JurisdictionId = Guid.NewGuid();
    protected readonly Guid AgencyId = Guid.NewGuid();
    protected readonly Guid UserId = Guid.NewGuid();

    protected ProjectionHarness()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext, SqliteTestAppDbContext>(o => o.UseSqlite(_connection));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITenantProvider>(_ => new FixedTenant(JurisdictionId, AgencyId, UserId));
        services.AddScoped<IModificationContext>(_ => new UserModificationContext(UserId));
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.AddScoped<ArrestFactory>(); // CreateArrestHandler needs it; AddApplication() doesn't register domain factories
        services.AddApplication();

        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    protected async Task<T> Send<T>(IRequest<T> request)
    {
        using var scope = Provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    protected async Task Send(IRequest request)
    {
        using var scope = Provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    public void Dispose()
    {
        Provider.Dispose();
        _connection.Dispose();
    }

    protected sealed class FixedTenant(Guid jurisdiction, Guid agency, Guid user) : ITenantProvider
    {
        public Guid GetAgencyId() => agency;
        public Guid GetJurisdictionId() => jurisdiction;
        public Guid GetUserId() => user;
        public void SetJurisdictionId(Guid jurisdictionId) { }
    }
}

public sealed class ArrestSoftDeleteProjectionTests : ProjectionHarness
{
    [Fact]
    public async Task SoftDelete_RemovesArrestFromReadModel()
    {
        var (recordNumber, id) = await CreateArrestAsync();
        Assert.NotNull(await Query(recordNumber)); // visible after create

        await Send(new SoftDeleteArrestCommand(id));

        Assert.Null(await Query(recordNumber)); // read-model row removed → no longer fetchable
    }

    [Fact]
    public async Task Restore_RebuildsArrestReadModel()
    {
        var (recordNumber, id) = await CreateArrestAsync();
        await Send(new SoftDeleteArrestCommand(id));
        Assert.Null(await Query(recordNumber));

        await Send(new RestoreArrestCommand(id));

        var restored = await Query(recordNumber);
        Assert.NotNull(restored);
        Assert.Equal(id, restored!.Id);
        Assert.Equal(recordNumber, restored.RecordNumber);
    }

    private async Task<(long recordNumber, Guid id)> CreateArrestAsync()
    {
        var nameRecord = await Send(new CreateNameCommand(
            "Person", "Smith", "John", null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, false, null));
        var name = await Send(new GetNameByRecordNumberQuery(nameRecord));

        var recordNumber = await Send(new CreateArrestCommand(name!.Id, DateTime.UtcNow.AddMinutes(-30), []));
        var dto = await Query(recordNumber);
        return (recordNumber, dto!.Id);
    }

    private Task<ArrestDto?> Query(long recordNumber) => Send(new GetArrestByRecordNumberQuery(recordNumber));
}

public sealed class NameSoftDeleteProjectionTests : ProjectionHarness
{
    [Fact]
    public async Task SoftDelete_RemovesNameFromReadModel()
    {
        var (recordNumber, id) = await CreateNameAsync();
        Assert.NotNull(await Query(recordNumber));

        await Send(new SoftDeleteNameCommand(id));

        Assert.Null(await Query(recordNumber));
    }

    [Fact]
    public async Task Restore_RebuildsNameReadModel()
    {
        var (recordNumber, id) = await CreateNameAsync();
        await Send(new SoftDeleteNameCommand(id));
        Assert.Null(await Query(recordNumber));

        await Send(new RestoreNameCommand(id));

        var restored = await Query(recordNumber);
        Assert.NotNull(restored);
        Assert.Equal(id, restored!.Id);
        Assert.Equal(recordNumber, restored.RecordNumber);
        Assert.Equal("Smith", restored.LastOrBusinessName);
    }

    private async Task<(long recordNumber, Guid id)> CreateNameAsync()
    {
        var recordNumber = await Send(new CreateNameCommand(
            "Person", "Smith", "John", null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, false, null));
        var dto = await Query(recordNumber);
        return (recordNumber, dto!.Id);
    }

    private Task<NameDto?> Query(long recordNumber) => Send(new GetNameByRecordNumberQuery(recordNumber));
}

public sealed class LocationSoftDeleteProjectionTests : ProjectionHarness
{
    [Fact]
    public async Task SoftDelete_RemovesLocationFromReadModel()
    {
        var (recordNumber, id) = await CreateLocationAsync();
        Assert.NotNull(await Query(recordNumber));

        await Send(new SoftDeleteLocationCommand(id));

        Assert.Null(await Query(recordNumber));
    }

    [Fact]
    public async Task Restore_RebuildsLocationReadModel()
    {
        var (recordNumber, id) = await CreateLocationAsync();
        await Send(new SoftDeleteLocationCommand(id));
        Assert.Null(await Query(recordNumber));

        await Send(new RestoreLocationCommand(id));

        var restored = await Query(recordNumber);
        Assert.NotNull(restored);
        Assert.Equal(id, restored!.Id);
        Assert.Equal(recordNumber, restored.RecordNumber);
        Assert.Equal("123 Main St", restored.StreetAddress);
    }

    private async Task<(long recordNumber, Guid id)> CreateLocationAsync()
    {
        var recordNumber = await Send(new CreateLocationCommand("123 Main St", "Springfield"));
        var dto = await Query(recordNumber);
        return (recordNumber, dto!.Id);
    }

    private Task<LocationDto?> Query(long recordNumber) => Send(new GetLocationByRecordNumberQuery(recordNumber));
}
