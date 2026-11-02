using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Infrastructure.IntegrationTests.Common;
using Infrastructure.IntegrationTests.TestInfrastructure;

namespace Infrastructure.IntegrationTests.Audit;

/// <summary>
/// Issue #121: a name's SSN must never be written to the outbox or the audit log payload,
/// on create or on update.
/// </summary>
public sealed class SsnPayloadTests : IDisposable
{
    private static readonly Guid JurisdictionId = Guid.NewGuid();
    private static readonly Guid AgencyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteTestAppDbContext> _options;

    public SsnPayloadTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<SqliteTestAppDbContext>().UseSqlite(_connection).Options;
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Ssn_is_not_stored_in_outbox_or_audit_payloads()
    {
        const string originalSsn = "212-55-1234";
        const string newSsn = "212-55-9876";

        var name = new Name(
            JurisdictionId, AgencyId, "Person", "Carter", "Marcus", null,
            null, null, null, null, null, null, null, null, null, null, null, null, null,
            originalSsn, false, null);

        await using (var context = CreateContext())
        {
            context.Names.Add(name);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var tracked = await context.Names.SingleAsync(n => n.Id == name.Id);
            tracked.UpdateDetails(
                "Person", "Carter", "Marcus", null,
                null, null, null, null, null, null, null, null, null, null, null, null, null,
                newSsn, false, null,
                new UserModificationContext(UserId));
            await context.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var auditPayloads = await read.AuditLogReadModels.Where(x => x.AggregateId == name.Id).Select(x => x.Payload).ToListAsync();
        var outboxContents = await read.OutboxMessages.Where(x => x.AggregateId == name.Id).Select(x => x.Content).ToListAsync();

        Assert.NotEmpty(auditPayloads);
        Assert.NotEmpty(outboxContents);
        foreach (var stored in auditPayloads.Concat(outboxContents))
        {
            Assert.DoesNotContain(originalSsn, stored);
            Assert.DoesNotContain(newSsn, stored);
            Assert.DoesNotContain("9876", stored);
        }

        Assert.Contains(auditPayloads, p => p.Contains("\"SocialSecurityNumberChanged\":true"));
        Assert.Equal(newSsn, (await read.Names.SingleAsync(n => n.Id == name.Id)).SocialSecurityNumber);
    }

    private SqliteTestAppDbContext CreateContext() =>
        new(_options, new TestTenantProvider(JurisdictionId, AgencyId), new TestDomainEventDispatcher());

    public void Dispose() => _connection.Dispose();
}
