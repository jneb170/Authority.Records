using Infrastructure.IntegrationTests.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Mugshots;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Mugshots;
using Shared.Infrastructure.Persistence;

namespace Infrastructure.IntegrationTests.Mugshots;

/// <summary>
/// <see cref="LegacyMugshotUrlRepair"/> against in-memory SQLite: URLs written while mugshots
/// were public static files are rewritten to the authorized endpoint everywhere they are
/// stored, regardless of tenant, and the repair is idempotent.
/// </summary>
public sealed class LegacyMugshotUrlRepairTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly Guid _jurisdictionId = Guid.NewGuid();
    private readonly Guid _agencyId = Guid.NewGuid();

    public LegacyMugshotUrlRepairTests()
    {
        _connection.Open();
        using var db = CreateContext(_jurisdictionId);
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Repair_RewritesLegacyUrls_OnAggregateAndAllReadModels()
    {
        var legacyId = Guid.NewGuid();
        var legacyUrl = $"/uploads/mugshots/{_jurisdictionId:N}/2026/03/{Guid.NewGuid():N}.jpg";
        var currentId = Guid.NewGuid();
        var nameId = Guid.NewGuid();
        var arrestId = Guid.NewGuid();
        var otherNameId = Guid.NewGuid();

        await using (var db = CreateContext(_jurisdictionId))
        {
            AddMugshot(db, legacyId, legacyUrl);
            AddMugshot(db, currentId, MugshotUrls.For(currentId));
            db.NameReadModels.Add(CreateName(nameId, primaryMugshotUrl: legacyUrl));
            db.NameReadModels.Add(CreateName(otherNameId, primaryMugshotUrl: MugshotUrls.For(currentId)));
            var arrest = ArrestReadModel.Create(arrestId, 1, _jurisdictionId, _agencyId, nameId, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid());
            arrest.ApplyPrimaryMugshot(legacyUrl);
            db.ArrestReadModels.Add(arrest);
            await db.SaveChangesAsync();
        }

        // Runs at startup outside any tenant: a different jurisdiction must not hide the rows.
        await using (var db = CreateContext(Guid.NewGuid()))
        {
            Assert.Equal(1, await LegacyMugshotUrlRepair.RepairCoreAsync(db, CancellationToken.None));
        }

        await using (var db = CreateContext(_jurisdictionId))
        {
            var expected = MugshotUrls.For(legacyId);
            Assert.Equal(expected, (await db.Mugshots.SingleAsync(m => m.Id == legacyId)).PublicUrl);
            Assert.Equal(expected, (await db.MugshotReadModels.SingleAsync(m => m.Id == legacyId)).PublicUrl);
            Assert.Equal(expected, (await db.NameReadModels.SingleAsync(n => n.Id == nameId)).PrimaryMugshotUrl);
            Assert.Equal(expected, (await db.ArrestReadModels.SingleAsync(a => a.Id == arrestId)).PrimaryMugshotUrl);

            // Already-current rows are untouched.
            Assert.Equal(MugshotUrls.For(currentId), (await db.Mugshots.SingleAsync(m => m.Id == currentId)).PublicUrl);
            Assert.Equal(MugshotUrls.For(currentId), (await db.NameReadModels.SingleAsync(n => n.Id == otherNameId)).PrimaryMugshotUrl);
        }
    }

    [Fact]
    public async Task Repair_IsIdempotent()
    {
        await using (var db = CreateContext(_jurisdictionId))
        {
            AddMugshot(db, Guid.NewGuid(), "/uploads/mugshots/x/2026/03/a.jpg");
            await db.SaveChangesAsync();
        }

        await using (var db = CreateContext(_jurisdictionId))
        {
            Assert.Equal(1, await LegacyMugshotUrlRepair.RepairCoreAsync(db, CancellationToken.None));
            Assert.Equal(0, await LegacyMugshotUrlRepair.RepairCoreAsync(db, CancellationToken.None));
        }
    }

    private void AddMugshot(AppDbContext db, Guid id, string url)
    {
        db.Mugshots.Add(new Mugshot(id, _jurisdictionId, _agencyId, "a.jpg", "image/jpeg", 10,
            "uploads/mugshots/a.jpg", url, DateTime.UtcNow));
        db.MugshotReadModels.Add(MugshotReadModel.Create(id, _jurisdictionId, _agencyId, "a.jpg", "image/jpeg", 10,
            "uploads/mugshots/a.jpg", url, DateTime.UtcNow, Guid.NewGuid(), DateTime.UtcNow));
    }

    private NameReadModel CreateName(Guid id, string primaryMugshotUrl)
    {
        var name = NameReadModel.Create(
            id, recordNumber: 1, _jurisdictionId, _agencyId, nameType: "Person", lastOrBusinessName: "Doe",
            firstName: "Jane", middleName: null, sexId: null, raceId: null, dateOfBirth: null,
            driversLicenseNumber: null, driversLicenseStateId: null, heightInches: null, weightLbs: null,
            hairColorId: null, eyeColorId: null, suffixId: null, placeOfBirth: null, fbiNumber: null,
            localNumber: null, socialSecurityNumber: null, isCitizen: true, deceasedDate: null,
            createdAtUtc: DateTime.UtcNow, createdBy: Guid.NewGuid());
        name.ApplyPrimaryMugshot(primaryMugshotUrl);
        return name;
    }

    private AppDbContext CreateContext(Guid jurisdictionId) => new(
        new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options,
        new TestTenantProvider(jurisdictionId, _agencyId),
        new TestDomainEventDispatcher());

    public void Dispose() => _connection.Dispose();
}
