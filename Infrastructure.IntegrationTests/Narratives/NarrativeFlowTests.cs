using Infrastructure.IntegrationTests.TestInfrastructure;
using MediatR;
using Microsoft.Data.Sqlite;
using Modules.Records.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application;
using Modules.Records.Application.Narratives.Commands.AcquireNarrativeLock;
using Modules.Records.Application.Narratives.Commands.CreateNarrative;
using Modules.Records.Application.Narratives.Commands.ReleaseNarrativeLock;
using Modules.Records.Application.Narratives.Commands.RestoreNarrative;
using Modules.Records.Application.Narratives.Commands.SoftDeleteNarrative;
using Modules.Records.Application.Narratives.Commands.UpdateNarrativeContent;
using Modules.Records.Application.Narratives.Queries.GetNarrativeById;
using Modules.Records.Application.Narratives.Queries.GetNarrativesByOwner;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Narratives;

/// <summary>
/// End-to-end vertical for the Narrative feature against in-memory SQLite: command → domain
/// event → projection → read-model query. Exercises create+link, edit, lock, and soft-delete/restore.
/// </summary>
public sealed class NarrativeFlowTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;
    private readonly Guid _jurisdictionId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _incidentId = Guid.NewGuid();

    public NarrativeFlowTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext, SqliteTestAppDbContext>(o => o.UseSqlite(_connection));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITenantProvider>(_ => new FixedTenant(_jurisdictionId, _agencyId, _userId));
        services.AddScoped<IModificationContext>(_ => new UserModificationContext(_userId));
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.AddApplication(); // real handlers, behaviors, validators, projections

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    private async Task<T> Send<T>(IRequest<T> request)
    {
        using var scope = _provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    private async Task Send(IRequest request)
    {
        using var scope = _provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    [Fact]
    public async Task Create_Projects_AndIsQueryableByOwner()
    {
        await Send(new CreateNarrativeCommand(
            NarrativeOwnerTypes.Incident, _incidentId, "Initial Report", "On arrival, officers observed..."));

        var list = await Send(new GetNarrativesByOwnerQuery(NarrativeOwnerTypes.Incident, _incidentId));

        var dto = Assert.Single(list);
        Assert.Equal("Initial Report", dto.Title);
        Assert.Equal("On arrival, officers observed...", dto.Content);
        Assert.True(dto.RecordNumber >= 30000);

        var byId = await Send(new GetNarrativeByIdQuery(dto.Id));
        Assert.NotNull(byId);
        Assert.Equal(dto.Id, byId!.Id);
    }

    [Fact]
    public async Task UpdateContent_IsReflectedInReadModel()
    {
        var id = await CreateAndGetId();

        await Send(new UpdateNarrativeContentCommand(id, "Follow-up", "Supplemental details."));

        var dto = await Send(new GetNarrativeByIdQuery(id));
        Assert.Equal("Follow-up", dto!.Title);
        Assert.Equal("Supplemental details.", dto.Content);
    }

    [Fact]
    public async Task AcquireAndReleaseLock_FlowsToReadModel()
    {
        var id = await CreateAndGetId();

        await Send(new AcquireNarrativeLockCommand(id));
        var locked = await Send(new GetNarrativeByIdQuery(id));
        Assert.True(locked!.IsLocked);
        Assert.Equal(_userId, locked.LockedByUserId);

        await Send(new ReleaseNarrativeLockCommand(id));
        var released = await Send(new GetNarrativeByIdQuery(id));
        Assert.False(released!.IsLocked);
        Assert.Null(released.LockedByUserId);
    }

    [Fact]
    public async Task SoftDelete_RemovesFromOwnerList_AndRestoreBringsItBack()
    {
        var id = await CreateAndGetId();

        await Send(new SoftDeleteNarrativeCommand(id));
        var afterDelete = await Send(new GetNarrativesByOwnerQuery(NarrativeOwnerTypes.Incident, _incidentId));
        Assert.Empty(afterDelete);

        await Send(new RestoreNarrativeCommand(id));
        var afterRestore = await Send(new GetNarrativesByOwnerQuery(NarrativeOwnerTypes.Incident, _incidentId));
        Assert.Single(afterRestore);
    }

    private async Task<Guid> CreateAndGetId()
    {
        await Send(new CreateNarrativeCommand(
            NarrativeOwnerTypes.Incident, _incidentId, "Initial Report", "Body."));
        var list = await Send(new GetNarrativesByOwnerQuery(NarrativeOwnerTypes.Incident, _incidentId));
        return list.Single().Id;
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }

    private sealed class FixedTenant(Guid jurisdiction, Guid agency, Guid user) : ITenantProvider
    {
        public Guid GetAgencyId() => agency;
        public Guid GetJurisdictionId() => jurisdiction;
        public Guid GetUserId() => user;
        public void SetJurisdictionId(Guid jurisdictionId) { }
    }
}
