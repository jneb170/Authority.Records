using Infrastructure.IntegrationTests.TestInfrastructure;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Application;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Citations.Commands.CreateCitation;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;
using Modules.Records.Application.Citations.Queries.GetCitationByRecordNumber;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Violations;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Citations;

/// <summary>
/// End-to-end vertical against in-memory SQLite for the split offense data + structured violation
/// flags: SaveCitationPageCommand → CitationOffenseDetails / CitationTexasDetails / CitationViolationFlag
/// rows → GetCitationByRecordNumberQuery DTO. Also verifies the manual-flag set is reconciled (diffed)
/// across saves rather than appended.
/// </summary>
public sealed class CitationOffenseAndFlagsFlowTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;
    private readonly Guid _jurisdictionId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public CitationOffenseAndFlagsFlowTests()
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
    public async Task Save_RoundTripsOffenseDetails_TexasDetails_AndFlags()
    {
        var citationId = await CreateCitationAsync();

        await Send(BuildSave(citationId, flags: [ViolationFlagKey.NoSignal, ViolationFlagKey.Ice]));

        var dto = await Query(citationId);

        Assert.NotNull(dto!.TexasDetails);
        Assert.Equal("DKT-1", dto.TexasDetails!.DocketNumber);

        Assert.NotNull(dto.OffenseDetails);
        Assert.Equal("Speeding", dto.OffenseDetails!.PrimaryViolationDescription);
        Assert.Equal(72, dto.OffenseDetails.SpeedMph);
        Assert.Equal(55, dto.OffenseDetails.ZoneMph);

        Assert.NotNull(dto.ViolationFlags);
        Assert.Equal(2, dto.ViolationFlags!.Count);
        Assert.Contains(dto.ViolationFlags, f => f.Key == ViolationFlagKey.NoSignal && f.Source == ViolationFlagSource.Manual);
        Assert.Contains(dto.ViolationFlags, f => f.Key == ViolationFlagKey.Ice);
    }

    [Fact]
    public async Task Save_ReconcilesManualFlagSet_AcrossSaves()
    {
        var citationId = await CreateCitationAsync();

        await Send(BuildSave(citationId, flags: [ViolationFlagKey.NoSignal, ViolationFlagKey.Ice]));
        await Send(BuildSave(citationId, flags: [ViolationFlagKey.NoSignal, ViolationFlagKey.Lane]));

        var dto = await Query(citationId);

        Assert.NotNull(dto!.ViolationFlags);
        var keys = dto.ViolationFlags!.Select(f => f.Key).ToHashSet();
        Assert.Equal(2, keys.Count);
        Assert.Contains(ViolationFlagKey.NoSignal, keys);   // retained
        Assert.Contains(ViolationFlagKey.Lane, keys);       // added
        Assert.DoesNotContain(ViolationFlagKey.Ice, keys);  // removed (unticked)
    }

    [Fact]
    public async Task Save_WithEmptyFlagSet_ClearsAllManualFlags()
    {
        var citationId = await CreateCitationAsync();

        await Send(BuildSave(citationId, flags: [ViolationFlagKey.NoSignal]));
        await Send(BuildSave(citationId, flags: []));

        var dto = await Query(citationId);
        Assert.Empty(dto!.ViolationFlags ?? []);
    }

    private SaveCitationPageCommand BuildSave(Guid citationId, IReadOnlyCollection<ViolationFlagKey> flags) => new(
        CitationId: citationId,
        DefendantNameId: null,
        Description: "Traffic stop",
        IssueDate: DateTime.UtcNow.AddMinutes(-10),
        CourtId: null,
        CitationNum: "CT-1",
        TexasDetails: new CitationTexasDetailsInput("DKT-1", "1"),
        OffenseDetails: new CitationOffenseDetailsInput(
            PrimaryViolationDescription: "Speeding",
            SpeedMph: 72,
            ZoneMph: 55,
            ViolationSection: "545.351"),
        ViolationFlags: flags);

    private async Task<Guid> CreateCitationAsync()
    {
        var recordNumber = await Send(new CreateCitationCommand("Traffic stop", DateTime.UtcNow.AddMinutes(-15), []));
        var dto = await Query(recordNumber);
        return dto!.Id;
    }

    private async Task<CitationDto?> Query(Guid citationId)
    {
        // GetCitationByRecordNumber is the read path used by the print page; resolve via record number.
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recordNumber = await db.Citations.Where(c => c.Id == citationId).Select(c => c.RecordNumber).SingleAsync();
        return await Query(recordNumber);
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
