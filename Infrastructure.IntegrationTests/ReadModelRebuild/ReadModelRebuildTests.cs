using Infrastructure.IntegrationTests.Common;
using Infrastructure.IntegrationTests.TestInfrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Admin.Commands.RebuildReadModels;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;

namespace Infrastructure.IntegrationTests.ReadModelRebuild;

public sealed class ReadModelRebuildTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteTestAppDbContext> _options;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();

    public ReadModelRebuildTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SqliteTestAppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task RebuildReadModels_Should_Preserve_Citation_IssueDate_And_Location()
    {
        var citationId = Guid.Empty;
        var expectedIssueDate = new DateTime(2026, 03, 13);
        var expectedLocationId = Guid.NewGuid();
        var modificationContext = new UserModificationContext(Guid.NewGuid(), false, false, false);

        await using (var context = CreateContext())
        {
            var citation = new Citation(_tenantId, _agencyId, "Original citation", new DateTime(2026, 01, 01), "CT-100");
            context.Citations.Add(citation);
            await context.SaveChangesAsync();

            citation.UpdateDetails("Updated citation", expectedIssueDate, courtId: null, citationNum: "CT-200", defendantNameId: null, modificationContext);
            citation.SetLocation(expectedLocationId, modificationContext);
            await context.SaveChangesAsync();

            citationId = citation.Id;
        }

        await using (var rebuildContext = CreateContext())
        {
            var handler = new RebuildReadModelsHandler(rebuildContext, new TestTenantProvider(_tenantId));
            var result = await handler.Handle(new RebuildReadModelsCommand(), CancellationToken.None);

            Assert.Equal(1, result.CitationsRebuilt);
        }

        await using (var verificationContext = CreateContext())
        {
            var readModel = await verificationContext.CitationReadModels
                .AsNoTracking()
                .SingleAsync(c => c.Id == citationId);

            Assert.Equal(expectedIssueDate, readModel.IssueDate);
            Assert.Equal(expectedLocationId, readModel.LocationId);
            Assert.Equal("Updated citation", readModel.Description);
            Assert.Equal("CT-200", readModel.CitationNum);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private SqliteTestAppDbContext CreateContext()
    {
        return new SqliteTestAppDbContext(
            _options,
            new TestTenantProvider(_tenantId),
            new TestDomainEventDispatcher());
    }
}
