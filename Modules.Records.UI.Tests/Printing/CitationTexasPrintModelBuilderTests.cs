using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Violations;
using Modules.Records.UI.Printing;

namespace Modules.Records.UI.Tests.Printing;

public class CitationTexasPrintModelBuilderTests
{
    private static readonly Guid SexId = Guid.NewGuid();
    private static readonly Guid RaceId = Guid.NewGuid();
    private static readonly Guid DlStateId = Guid.NewGuid();
    private static readonly Guid GroupId = Guid.NewGuid();
    private static readonly Guid BandId = Guid.NewGuid();
    private static readonly Guid CitationLocationId = Guid.NewGuid();

    private static (CitationTexasPrintModelBuilder builder, long recordNumber) Build(
        CitationDto citation,
        Dictionary<Guid, string>? labels = null,
        Dictionary<Guid, LocationDto>? locations = null)
    {
        var builder = new CitationTexasPrintModelBuilder(
            new FakeCitationService(citation),
            new FakePicklistService(labels ?? new()),
            new FakeLocationService(locations));
        return (builder, citation.RecordNumber);
    }

    private static CitationDto NewCitation(
        string citationNum = "CT-2026-000001",
        NameSnapshotDto? name = null,
        CitationOffenseDetailsDto? offense = null,
        IReadOnlyList<CitationViolationFlagDto>? flags = null,
        Guid? locationId = null)
        => new(
            Id: Guid.NewGuid(),
            RecordNumber: 10000,
            JurisdictionId: Guid.NewGuid(),
            AgencyId: Guid.NewGuid(),
            DefendantNameId: null,
            DefendantName: "Perez, Mary",
            DefendantNameRecordNumber: null,
            Description: "test",
            IssueDate: new DateTime(2026, 5, 24, 14, 30, 0, DateTimeKind.Utc),
            IsIssued: true,
            IsLocked: false,
            LockedByUserId: null,
            CreatedBy: Guid.NewGuid(),
            ModifiedBy: null,
            CreatedAtUtc: DateTime.UtcNow,
            UpdatedAtUtc: DateTime.UtcNow,
            CitationNum: citationNum,
            LocationId: locationId,
            AtTimeOfName: name,
            OffenseDetails: offense,
            ViolationFlags: flags);

    [Fact]
    public async Task BuildAsync_ReturnsNull_WhenCitationMissing()
    {
        var builder = new CitationTexasPrintModelBuilder(
            new FakeCitationService(null),
            new FakePicklistService(new()),
            new FakeLocationService());

        Assert.Null(await builder.BuildAsync(999));
    }

    [Fact]
    public async Task BuildAsync_MapsPicklistLabels_ToFormFields()
    {
        var name = new NameSnapshotDto(
            SourceNameId: null, SourceNameRecordNumber: null, NameType: NameTypes.Person,
            LastOrBusinessName: "Perez", FirstName: "Mary", MiddleName: "L",
            SexId: SexId, RaceId: RaceId, DriversLicenseStateId: DlStateId);

        var citation = NewCitation(name: name);
        var labels = new Dictionary<Guid, string>
        {
            [SexId] = "Female",
            [RaceId] = "White",
            [DlStateId] = "Texas",
        };

        var (builder, recordNumber) = Build(citation, labels);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("Female", model!.Sex);
        Assert.Equal("White", model.Race);
        Assert.Equal("TX", model.DriversLicenseState); // CompactState maps full name -> abbreviation
        Assert.Equal("Perez", model.LastName);
        Assert.Equal("Mary", model.FirstName);
        Assert.Equal("L", model.Initial);
        Assert.Equal("Perez, Mary", model.FullName);
    }

    [Fact]
    public async Task BuildAsync_OnlyManualFlagsAreVisible_ChargeDerivedExcluded()
    {
        var flags = new List<CitationViolationFlagDto>
        {
            new(ViolationFlagKey.ImproperLeftTurn, ViolationFlagSource.Manual, null),
            new(ViolationFlagKey.CollisionRearEnd, ViolationFlagSource.Manual, null),
            new(ViolationFlagKey.Snow, ViolationFlagSource.Charge, Guid.NewGuid()),
        };
        var citation = NewCitation(flags: flags);

        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.True(model!.Flag(ViolationFlagKey.ImproperLeftTurn));
        Assert.True(model.Flag(ViolationFlagKey.CollisionRearEnd));
        Assert.False(model.Flag(ViolationFlagKey.Snow)); // charge-derived, not yet visible
    }

    [Fact]
    public async Task BuildAsync_DoesNotInferFlagsFromNarrativeText()
    {
        // The pre-B-1 bug marked checkboxes by substring-matching the narrative: "device" set Ice,
        // "lane change" set Lane, etc. With structured flags, a narrative full of trigger words but
        // zero structured flags must mark nothing.
        var offense = new CitationOffenseDetailsDto(
            PrimaryViolationDescription: "Failure to signal lane change",
            NarrativeOtherViolations: "Vehicle struck a fixed device near the curve at night.");
        var citation = NewCitation(offense: offense, flags: new List<CitationViolationFlagDto>());

        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Empty(model!.VisibleFlags);
        Assert.False(model.Flag(ViolationFlagKey.Ice));    // would have matched "dev[ice]"
        Assert.False(model.Flag(ViolationFlagKey.Lane));   // would have matched "lane"
        Assert.False(model.Flag(ViolationFlagKey.Night));  // would have matched "night"
        Assert.False(model.Flag(ViolationFlagKey.NoSignal));
    }

    [Fact]
    public async Task BuildAsync_DerivesSpeedRange_AreaAndParking_FromPicklistLabels()
    {
        var offense = new CitationOffenseDetailsDto(
            ViolationGroupId: GroupId,
            SpeedBandId: BandId,
            SpeedMph: 47,
            ZoneMph: 30);
        var citation = NewCitation(offense: offense);
        var labels = new Dictionary<Guid, string>
        {
            [GroupId] = "Parking - Area",
            [BandId] = "Over 15 m.p.h. over limit",
        };

        var (builder, recordNumber) = Build(citation, labels);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.True(model!.SpeedRangeOver15);
        Assert.False(model.SpeedRange5To10);
        Assert.Equal("47", model.SpeedMph);
        Assert.Equal("30", model.ZoneMph);
        Assert.True(model.IsParking);
        Assert.True(model.AreaResidential is false); // "Parking - Area" doesn't contain "Residential"
    }

    [Fact]
    public async Task BuildAsync_FallsBackToCitationLocation_ForOccurredAt()
    {
        var location = new LocationDto(
            Id: CitationLocationId, RecordNumber: 5, JurisdictionId: Guid.NewGuid(),
            StreetNumber: "5445", PreDirectionId: null, StreetAddress: "Legacy Dr",
            StreetTypeId: null, PostDirectionId: null, City: "Plano", StateId: null,
            CountryId: null, Zip: "75024", AptSuite: null, Coordinates: null,
            CommonPlaceName: null, Comments: null, Address: "5445 Legacy Dr, Plano, TX 75024, USA",
            IsLocked: false, LockedByUserId: null, CreatedBy: Guid.NewGuid(), ModifiedBy: null,
            CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow);

        // No OccurredAtText on the offense -> falls back to the resolved citation location.
        var citation = NewCitation(offense: new CitationOffenseDetailsDto(), locationId: CitationLocationId);

        var (builder, recordNumber) = Build(
            citation,
            locations: new Dictionary<Guid, LocationDto> { [CitationLocationId] = location });
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("5445, Legacy Dr, Plano", model!.OccurredAt); // FormatLocation joins parts
    }
}
