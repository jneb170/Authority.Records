using Infrastructure.IntegrationTests.TestInfrastructure;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Citations.Commands.CreateCitation;
using Modules.Records.Application.Citations.Commands.RestoreCitation;
using Modules.Records.Application.Citations.Commands.SoftDeleteCitation;
using Modules.Records.Application.Citations.Queries.GetCitationByRecordNumber;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Citations;

/// <summary>
/// Regression coverage for the citation read-model projection on soft-delete / restore, exercised
/// through the real MediatR dispatch pipeline (so projection handlers actually run — unlike the
/// fake-dispatcher SoftDeleteTests). Before the fix, soft-deleting a citation left its
/// CitationReadModel row behind, so the read path (lists, detail, the Texas PDF) kept returning a
/// "deleted" citation.
/// </summary>
public sealed class CitationSoftDeleteProjectionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;
    private readonly Guid _jurisdictionId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public CitationSoftDeleteProjectionTests()
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
        services.AddApplication();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task SoftDelete_RemovesCitationFromReadModel()
    {
        var (recordNumber, id) = await CreateCitationAsync();
        Assert.NotNull(await Query(recordNumber)); // visible after create

        await Send(new SoftDeleteCitationCommand(id));

        Assert.Null(await Query(recordNumber)); // read model row removed → no longer fetchable
    }

    [Fact]
    public async Task Restore_RebuildsCitationReadModel()
    {
        var (recordNumber, id) = await CreateCitationAsync();
        await Send(new SoftDeleteCitationCommand(id));
        Assert.Null(await Query(recordNumber));

        await Send(new RestoreCitationCommand(id));

        var restored = await Query(recordNumber);
        Assert.NotNull(restored);
        Assert.Equal(id, restored!.Id);
        Assert.Equal(recordNumber, restored.RecordNumber);
        Assert.Equal("Traffic stop", restored.Description);
    }

    private async Task<(long recordNumber, Guid id)> CreateCitationAsync()
    {
        var recordNumber = await Send(new CreateCitationCommand("Traffic stop", DateTime.UtcNow.AddMinutes(-15), []));
        var dto = await Query(recordNumber);
        return (recordNumber, dto!.Id);
    }

    private Task<CitationDto?> Query(long recordNumber) => Send(new GetCitationByRecordNumberQuery(recordNumber));

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
