using Modules.Records.Domain.Common.Violations;

namespace Modules.Records.UI.Printing;

/// <summary>
/// A fully-resolved, pre-formatted view of a citation for the Texas UTC (TX17-4R) printed form.
/// Every value is the exact string or boolean the form renders — picklist IDs are already mapped to
/// labels, locations to display strings, dates/times formatted, and checkbox flags reduced to the
/// visible set. The PDF document (<see cref="CitationTexasPdfDocument"/>) is pure layout over this
/// model; all resolution and formatting lives in <see cref="ICitationTexasPrintModelBuilder"/>.
/// </summary>
public sealed record CitationTexasPrintModel
{
    public required long RecordNumber { get; init; }
    public required string DocumentTitle { get; init; }

    // Header / meta
    public string CaseNo { get; init; } = string.Empty;
    public string DocketNo { get; init; } = string.Empty;
    public string PageNo { get; init; } = string.Empty;
    public string CourtLabel { get; init; } = string.Empty;
    public string AppearanceOrCitationLocation { get; init; } = string.Empty;

    // Issue date / time row
    public string IssueDate { get; init; } = string.Empty;
    public string IssueDayOfMonth { get; init; } = string.Empty;
    public string IssueMonthYear { get; init; } = string.Empty;
    public string IssueTime { get; init; } = string.Empty;
    public string IssueAmPm { get; init; } = string.Empty;

    // Defendant name
    public string LastName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string Initial { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;

    // Identity
    public string AddressStreet { get; init; } = string.Empty;
    public string CityState { get; init; } = string.Empty;
    public string Age { get; init; } = string.Empty;
    public string BirthDate { get; init; } = string.Empty;
    public string Race { get; init; } = string.Empty;
    public string Sex { get; init; } = string.Empty;
    public string Height { get; init; } = string.Empty;
    public string Weight { get; init; } = string.Empty;
    public string SocialSecurityNumber { get; init; } = string.Empty;

    // Driver's license
    public string DriversLicenseNumber { get; init; } = string.Empty;
    public string DriversLicenseState { get; init; } = string.Empty;

    // Vehicle
    public bool VehicleIsCommercial { get; init; }
    public bool VehicleCarriesHazmat { get; init; }
    public string PlateNumber { get; init; } = string.Empty;
    public string PlateYear { get; init; } = string.Empty;
    public string PlateState { get; init; } = string.Empty;
    public string ModelYear { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Style { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    // Offense location text
    public string OccurredAt { get; init; } = string.Empty;

    // Speeding
    public bool SpeedRange5To10 { get; init; }
    public bool SpeedRange11To15 { get; init; }
    public bool SpeedRangeOver15 { get; init; }
    public string SpeedMph { get; init; } = string.Empty;
    public string ZoneMph { get; init; } = string.Empty;

    // Other violations
    public string OtherViolations { get; init; } = string.Empty;
    public bool SourceStateStatute { get; init; }
    public bool SourceLocalOrdinance { get; init; }
    public string ViolationSection { get; init; } = string.Empty;

    // Parking — all four boxes share the same source today (see audit B-2/B-3).
    public bool IsParking { get; init; }

    // Area
    public bool AreaBusiness { get; init; }
    public bool AreaSchool { get; init; }
    public bool AreaResidential { get; init; }
    public bool AreaRural { get; init; }

    // Highway type
    public bool Highway2Lane { get; init; }
    public bool Highway3Lane { get; init; }
    public bool Highway4Lane { get; init; }
    public bool Highway4LaneDivided { get; init; }

    // Side columns
    public string AcceptedBondNotes { get; init; } = string.Empty;
    public string ReceiptNumber { get; init; } = string.Empty;

    // Signatures / court appearance
    public string AffidavitSignedDate { get; init; } = string.Empty;
    public string ComplainantSignature { get; init; } = string.Empty;
    public string OfficerNameAndTitle { get; init; } = string.Empty;
    public string UnitNumber { get; init; } = string.Empty;
    public string CourtAppearanceDay { get; init; } = string.Empty;
    public string CourtAppearanceTime { get; init; } = string.Empty;
    public string CourtAddress { get; init; } = string.Empty;
    public string DefendantSignature { get; init; } = string.Empty;

    /// <summary>The structured violation flags that should print (manual flags, plus any active
    /// charge-derived flags once that feature lands). The PDF marks a checkbox iff its key is here.</summary>
    public IReadOnlySet<ViolationFlagKey> VisibleFlags { get; init; } = new HashSet<ViolationFlagKey>();

    public bool Flag(ViolationFlagKey key) => VisibleFlags.Contains(key);
}
