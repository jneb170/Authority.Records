using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.ReadModels;

public sealed class ReadModelAuditTimestampTests
{
    [Fact]
    public void IncidentReadModel_ApplyModifiedAudit_Uses_CreatedAt_When_ModifiedAt_Is_Null()
    {
        var createdAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var model = IncidentReadModel.Create(
            Guid.NewGuid(),
            10018,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new IncidentDetails { IncidentNum = "INC-10018", LocalNum = "", Description = "Test", CFSNum = "" },
            RecordStatus.Open,
            createdAt,
            Guid.NewGuid());

        model.ApplyLocationChanged(Guid.NewGuid());
        model.ApplyOccurredOnChanged(createdAt.AddDays(-1));
        model.IncrementArrestCount();
        model.ApplyModifiedAudit(null, null, createdAt);

        Assert.Equal(createdAt, model.UpdatedAtUtc);
    }

    [Fact]
    public void ArrestReadModel_ApplyModifiedAudit_Uses_CreatedAt_When_ModifiedAt_Is_Null()
    {
        var createdAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var model = ArrestReadModel.Create(
            Guid.NewGuid(),
            20018,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            createdAt,
            Guid.NewGuid(),
            "AR-20018");

        model.ApplyDetailsChanged(Guid.NewGuid(), createdAt, null, "AR-20018", null);
        model.ApplyLocationChanged(Guid.NewGuid());
        model.ApplyPrimaryMugshot("/mugshots/test.jpg");
        model.ApplyModifiedAudit(null, null, createdAt);

        Assert.Equal(createdAt, model.UpdatedAtUtc);
    }

    [Fact]
    public void CitationReadModel_ApplyModifiedAudit_Uses_CreatedAt_When_ModifiedAt_Is_Null()
    {
        var createdAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var model = CitationReadModel.Create(
            Guid.NewGuid(),
            30018,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test citation",
            createdAt,
            createdAt,
            Guid.NewGuid(),
            "CT-30018");

        model.ApplyDetailsChanged("Updated citation", createdAt, null, "CT-30018", Guid.NewGuid());
        model.ApplyLocationChanged(Guid.NewGuid());
        model.ApplyIssued();
        model.ApplyModifiedAudit(null, null, createdAt);

        Assert.Equal(createdAt, model.UpdatedAtUtc);
    }

    [Fact]
    public void NameReadModel_ApplyModifiedAudit_Uses_CreatedAt_When_ModifiedAt_Is_Null()
    {
        var createdAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var model = NameReadModel.Create(
            Guid.NewGuid(),
            40018,
            Guid.NewGuid(),
            Guid.NewGuid(),
            NameTypes.Person,
            "Example",
            "Taylor",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            createdAt,
            Guid.NewGuid());

        model.ApplyLocationChanged(Guid.NewGuid(), Guid.NewGuid());
        model.ApplyPrimaryMugshot("/mugshots/name.jpg");
        model.ApplyModifiedAudit(null, null, createdAt);

        Assert.Equal(createdAt, model.UpdatedAtUtc);
    }

    [Fact]
    public void LocationReadModel_ApplyModifiedAudit_Uses_CreatedAt_When_ModifiedAt_Is_Null()
    {
        var createdAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var model = LocationReadModel.Create(
            id:             Guid.NewGuid(),
            recordNumber:   50018,
            jurisdictionId: Guid.NewGuid(),
            streetNumber:   "123",
            preDirectionId: null,
            streetAddress:  "Main St",
            streetTypeId:   null,
            postDirectionId: null,
            city:           "Springfield",
            stateId:        null,
            countryId:      null,
            zip:            null,
            aptSuite:       null,
            coordinates:    null,
            commonPlaceName: null,
            comments:       null,
            address:        "123 Main St, Springfield",
            createdAtUtc:   createdAt,
            createdBy:      Guid.NewGuid());

        model.ApplyDetailsChanged("123", null, "Main St", null, null, "Springfield", null, null, null, null, null, null, null, "123 Main St, Springfield");
        model.ApplyModifiedAudit(null, null, createdAt);

        Assert.Equal(createdAt, model.UpdatedAtUtc);
    }
}
