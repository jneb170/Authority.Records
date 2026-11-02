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
            // Agency wall-clock time as entered on the citation screen (stored Kind=Unspecified).
            IssueDate: new DateTime(2026, 5, 24, 14, 30, 0),
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
    public async Task BuildAsync_PrintsIssueDateTime_AsEntered()
    {
        // The citation screens store the issue date/time as the agency's wall-clock time. The form
        // must print what the officer entered (2:30 PM), not shift it as if it were UTC (9:30 AM).
        var (builder, recordNumber) = Build(NewCitation());
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("2:30", model!.IssueTime);
        Assert.Equal("PM", model.IssueAmPm);
        Assert.Equal("05/24/2026", model.IssueDate);
        Assert.Equal("24", model.IssueDayOfMonth);
        Assert.Equal("May 2026", model.IssueMonthYear);
    }

    [Fact]
    public async Task BuildAsync_DateOnlyIssueDate_IsNotRolledBackADay()
    {
        // The create form takes a date only, stored as midnight. Treating that as UTC printed the
        // previous evening (05/23, 7:00 PM) in Central.
        var citation = NewCitation() with { IssueDate = new DateTime(2026, 5, 24) };
        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("05/24/2026", model!.IssueDate);
        Assert.Equal("24", model.IssueDayOfMonth);
        Assert.Equal("May 2026", model.IssueMonthYear);
    }

    [Fact]
    public async Task BuildAsync_PrintsCourtAppearance_AsEntered()
    {
        var offense = new CitationOffenseDetailsDto(CourtAppearanceDateTime: new DateTime(2026, 6, 15, 9, 0, 0));
        var (builder, recordNumber) = Build(NewCitation(offense: offense));
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("06/15/2026", model!.CourtAppearanceDay);
        Assert.Equal("9:00 AM", model.CourtAppearanceTime);
    }

    [Fact]
    public async Task BuildAsync_BirthDate_IsNotShiftedByTimeZone()
    {
        // A date of birth is a calendar date, not an instant. Even stored at midnight UTC it must
        // print as that date in any zone -- never rolled back a day like an instant would be.
        var name = new NameSnapshotDto(
            SourceNameId: null, SourceNameRecordNumber: null, NameType: NameTypes.Person,
            LastOrBusinessName: "Perez", FirstName: "Mary", MiddleName: "L",
            DateOfBirth: new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var (builder, recordNumber) = Build(NewCitation(name: name));
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("01/01/1990", model!.BirthDate);
    }

    [Fact]
    public async Task BuildAsync_RendersOfficerNameWithTitle_AndBadgeOnIdentificationLine()
    {
        // A-4: name + title on the "(Name and title)" line (was title only).
        // A-3: the captured badge prints on the identification line (was never printed).
        var profile = new CitationOfficerProfileDto(
            SourceNameId: null, SourceNameRecordNumber: null,
            OfficerName: "J. Smith", Title: "Sergeant", BadgeOrIdentifier: "1234", UnitNumber: "12");
        var citation = NewCitation() with { OfficerProfile = profile };

        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("J. Smith, Sergeant", model!.OfficerNameAndTitle);
        Assert.Equal("J. Smith  #1234", model.ComplainantSignature); // name (no signature text) + badge
        Assert.Equal("12", model.UnitNumber);
    }

    [Fact]
    public async Task BuildAsync_OfficerIdentification_FallsBackCleanly()
    {
        // No badge -> identification line is just the name; no title -> name/title line is just the name.
        var profile = new CitationOfficerProfileDto(
            SourceNameId: null, SourceNameRecordNumber: null,
            OfficerName: "A. Jones", Title: null, BadgeOrIdentifier: null, UnitNumber: null);
        var citation = NewCitation() with { OfficerProfile = profile };

        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("A. Jones", model!.OfficerNameAndTitle);
        Assert.Equal("A. Jones", model.ComplainantSignature);
    }

    [Fact]
    public async Task BuildAsync_ComputesAge_AtIssueDate_NotPrintDate()
    {
        // B-7: born 2000-01-01, issued 2020-01-01 -> age 20 at issuance. Computing at today's date
        // (2026+) would give 26, so this proves age is anchored to the issue date.
        var name = new NameSnapshotDto(
            SourceNameId: null, SourceNameRecordNumber: null, NameType: NameTypes.Person,
            LastOrBusinessName: "Perez", FirstName: "Mary",
            DateOfBirth: new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var citation = NewCitation(name: name) with { IssueDate = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc) };

        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.NotNull(model);
        Assert.Equal("20", model!.Age);
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
        // The location's formatted address, without the country (issue #98: was "5445, Legacy Dr, Plano").
        Assert.Equal("5445 Legacy Dr, Plano, TX 75024", model!.OccurredAt);
    }

    // --- Issue #98 -----------------------------------------------------------------------------------

    private static LocationDto NewLocation(Guid id, string streetNumber, string street, string city, string? address)
        => new(
            Id: id, RecordNumber: 7, JurisdictionId: Guid.NewGuid(),
            StreetNumber: streetNumber, PreDirectionId: null, StreetAddress: street,
            StreetTypeId: null, PostDirectionId: null, City: city, StateId: null,
            CountryId: null, Zip: null, AptSuite: null, Coordinates: null,
            CommonPlaceName: null, Comments: null, Address: address,
            IsLocked: false, LockedByUserId: null, CreatedBy: Guid.NewGuid(), ModifiedBy: null,
            CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow);

    [Fact]
    public async Task CourtLine_NeverFallsBackToTheOffenseLocation()
    {
        var location = NewLocation(CitationLocationId, "77", "Center", "Riverbend", "77 Center St, Riverbend, TX 78640");
        var citation = NewCitation(offense: new CitationOffenseDetailsDto(), locationId: CitationLocationId);

        var (builder, recordNumber) = Build(citation, locations: new() { [CitationLocationId] = location });
        var model = await builder.BuildAsync(recordNumber);

        Assert.Equal(string.Empty, model!.AffidavitCourtName);
        Assert.Equal(string.Empty, model.AffidavitCourtOf);
        Assert.Equal("77 Center St, Riverbend, TX 78640", model.OccurredAt);
    }

    [Fact]
    public async Task CourtLine_UsesTheCourtLabel_AndTheAppearanceLocationCity()
    {
        var courtId = Guid.NewGuid();
        var appearanceId = Guid.NewGuid();
        var appearance = NewLocation(appearanceId, "100", "Main", "Riverbend", "100 Main St, Riverbend, TX 78640");
        var citation = NewCitation(offense: new CitationOffenseDetailsDto(CourtAppearanceLocationId: appearanceId)) with { CourtId = courtId };

        var (builder, recordNumber) = Build(
            citation,
            labels: new() { [courtId] = "Municipal Court" },
            locations: new() { [appearanceId] = appearance });
        var model = await builder.BuildAsync(recordNumber);

        Assert.Equal("Municipal", model!.AffidavitCourtName); // not "Municipal Court COURT OF"
        Assert.Equal("Riverbend", model.AffidavitCourtOf);
        Assert.Equal("Municipal Court", model.CourtLabel);   // the COURT APPEARANCE field keeps the full label
    }

    [Theory]
    [InlineData("Municipal Court of Riverbend", "Municipal", "Riverbend")]
    [InlineData("Municipal Court", "Municipal", null)]
    [InlineData("Justice", "Justice", null)]
    [InlineData("", "", null)]
    public void SplitCourtLabel_SeparatesCourtTypeAndPlace(string label, string name, string? of)
    {
        var (actualName, actualOf) = CitationTexasPrintModelBuilder.SplitCourtLabel(label);

        Assert.Equal(name, actualName);
        Assert.Equal(of, actualOf);
    }

    [Fact]
    public void FormatLocation_FallsBackToParts_WithSpacesNotCommas()
    {
        var location = NewLocation(Guid.NewGuid(), "77", "Center St", "Riverbend", address: null);

        Assert.Equal("77 Center St, Riverbend", CitationTexasPrintModelBuilder.FormatLocation(location));
    }

    [Fact]
    public async Task OtherViolations_FallsBackToTheCitationDescription_WhenTheOffensePanelIsEmpty()
    {
        var citation = NewCitation(offense: new CitationOffenseDetailsDto()) with { Description = "Open container of alcohol in a vehicle" };

        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.Equal("Open container of alcohol in a vehicle", model!.OtherViolations);
    }

    [Fact]
    public async Task OtherViolations_DoesNotRepeatTheDescription_WhenBoxesAreTicked()
    {
        var flags = new List<CitationViolationFlagDto> { new(ViolationFlagKey.ImproperLeftTurn, ViolationFlagSource.Manual, null) };
        var citation = NewCitation(offense: new CitationOffenseDetailsDto(), flags: flags) with { Description = "Turned left from the right lane" };

        var (builder, recordNumber) = Build(citation);
        var model = await builder.BuildAsync(recordNumber);

        Assert.Equal(string.Empty, model!.OtherViolations);
    }
}
