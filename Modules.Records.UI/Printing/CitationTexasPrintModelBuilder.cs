using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Violations;
using Modules.Records.UI.Services;

namespace Modules.Records.UI.Printing;

/// <summary>
/// Builds the <see cref="CitationTexasPrintModel"/> for the TX17-4R form. Resolves picklist IDs to
/// labels and location IDs to display strings, then applies the same display formatting the printed
/// form has always used. This logic was lifted verbatim from <c>CitationTexasPrint.razor</c> when the
/// form moved from browser-rendered HTML to a server-generated PDF — behavior is intentionally
/// unchanged, including the known audit items (e.g. B-8 state abbreviation).
/// <para>
/// Issue date/time and court appearance date/time are stored as the agency's local wall-clock time,
/// exactly as entered on the citation screens (see <c>Citation.IssueDate</c>), so they are printed as
/// stored. They used to be converted from UTC to the agency zone, which printed a citation issued at
/// 2:30 PM as 9:30 AM and a date-only issue date as the previous day.
/// </para>
/// </summary>
public sealed class CitationTexasPrintModelBuilder : ICitationTexasPrintModelBuilder
{
    private readonly ICitationService _citationService;
    private readonly IPicklistService _picklistService;
    private readonly ILocationService _locationService;

    public CitationTexasPrintModelBuilder(
        ICitationService citationService,
        IPicklistService picklistService,
        ILocationService locationService)
    {
        _citationService = citationService;
        _picklistService = picklistService;
        _locationService = locationService;
    }

    public async Task<CitationTexasPrintModel?> BuildAsync(long recordNumber, CancellationToken cancellationToken = default)
    {
        var citation = await _citationService.GetByRecordNumberAsync(recordNumber);
        if (citation is null)
            return null;

        var picklistIds = new[]
        {
            citation.CourtId,
            citation.AtTimeOfName?.SexId,
            citation.AtTimeOfName?.RaceId,
            citation.AtTimeOfName?.DriversLicenseStateId,
            citation.Vehicle?.PlateStateId,
            citation.OffenseDetails?.ViolationSourceTypeId,
            citation.OffenseDetails?.ViolationGroupId,
            citation.OffenseDetails?.SpeedBandId
        }
        .Where(id => id.HasValue)
        .Select(id => id!.Value)
        .Distinct()
        .ToArray();

        var labels = picklistIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await _picklistService.GetItemsByIdsAsync(picklistIds);

        var courtLabel = citation.CourtId.HasValue ? labels.GetValueOrDefault(citation.CourtId.Value) : null;
        var sexLabel = citation.AtTimeOfName?.SexId is Guid sexId ? labels.GetValueOrDefault(sexId) : null;
        var raceLabel = citation.AtTimeOfName?.RaceId is Guid raceId ? labels.GetValueOrDefault(raceId) : null;
        var dlStateLabel = citation.AtTimeOfName?.DriversLicenseStateId is Guid dlStateId ? labels.GetValueOrDefault(dlStateId) : null;
        var vehiclePlateStateLabel = citation.Vehicle?.PlateStateId is Guid vehicleStateId ? labels.GetValueOrDefault(vehicleStateId) : null;
        var violationSourceLabel = citation.OffenseDetails?.ViolationSourceTypeId is Guid sourceId ? labels.GetValueOrDefault(sourceId) : null;
        var violationGroupLabel = citation.OffenseDetails?.ViolationGroupId is Guid groupId ? labels.GetValueOrDefault(groupId) : null;
        var speedBandLabel = citation.OffenseDetails?.SpeedBandId is Guid speedBandId ? labels.GetValueOrDefault(speedBandId) : null;

        var citationLocation = await ResolveLocationAsync(citation.LocationId);
        var appearanceLocation = await ResolveLocationAsync(citation.OffenseDetails?.CourtAppearanceLocationId);
        var citationLocationDisplay = citationLocation is null ? null : FormatLocation(citationLocation);
        var appearanceLocationDisplay = appearanceLocation is null ? null : FormatLocation(appearanceLocation);
        var (affidavitCourtName, courtOfFromLabel) = SplitCourtLabel(courtLabel);

        var name = citation.AtTimeOfName;
        var offense = citation.OffenseDetails;
        var vehicle = citation.Vehicle;

        return new CitationTexasPrintModel
        {
            RecordNumber = recordNumber,
            DocumentTitle = string.IsNullOrWhiteSpace(citation.CitationNum)
                ? $"Citation {recordNumber}"
                : $"Citation {citation.CitationNum}",

            CaseNo = Display(citation.CitationNum),
            DocketNo = Display(citation.TexasDetails?.DocketNumber),
            PageNo = Display(citation.TexasDetails?.PageNumber),
            CourtLabel = Display(courtLabel),
            // The court line used to fall back to the offense location ("COURT OF 77, Center, Riverbend").
            AffidavitCourtName = affidavitCourtName,
            AffidavitCourtOf = Display(courtOfFromLabel, appearanceLocation?.City),

            IssueDate = DateDisplay(citation.IssueDate),
            IssueDayOfMonth = DayOfMonthDisplay(citation.IssueDate),
            IssueMonthYear = MonthYearDisplay(citation.IssueDate),
            IssueTime = TimeDisplay(citation.IssueDate),
            IssueAmPm = AmPmDisplay(citation.IssueDate),

            LastName = Display(name?.LastOrBusinessName),
            FirstName = Display(name?.FirstName),
            Initial = string.IsNullOrWhiteSpace(name?.MiddleName) ? string.Empty : name!.MiddleName![0].ToString(),
            FullName = DisplayFullName(citation),

            AddressStreet = CompactAddress(name?.PrimaryAddress?.Address, includeStreet: true),
            CityState = CompactAddress(name?.PrimaryAddress?.Address, includeStreet: false),
            Age = AgeDisplay(name?.DateOfBirth, citation.IssueDate),
            BirthDate = DateOnlyDisplay(name?.DateOfBirth),
            Race = CompactRaceDisplay(raceLabel),
            Sex = Display(sexLabel),
            Height = name?.HeightInches?.ToString() ?? string.Empty,
            Weight = name?.WeightLbs?.ToString() ?? string.Empty,
            SocialSecurityNumber = Display(name?.SocialSecurityNumber),

            DriversLicenseNumber = Display(name?.DriversLicenseNumber),
            DriversLicenseState = CompactState(dlStateLabel),

            VehicleIsCommercial = vehicle?.IsCommercial == true,
            VehicleCarriesHazmat = vehicle?.CarriesHazardousMaterial == true,
            PlateNumber = Display(vehicle?.PlateNumber),
            PlateYear = NumberDisplay(vehicle?.PlateYear),
            PlateState = CompactState(vehiclePlateStateLabel),
            ModelYear = NumberDisplay(vehicle?.ModelYear),
            Make = Display(vehicle?.Make),
            Style = Display(vehicle?.Style),
            Color = Display(vehicle?.Color),

            OccurredAt = Display(offense?.OccurredAtText, citationLocationDisplay),

            SpeedRange5To10 = IsSpeedRange(speedBandLabel, "5-10"),
            SpeedRange11To15 = IsSpeedRange(speedBandLabel, "11-15"),
            SpeedRangeOver15 = IsSpeedRange(speedBandLabel, "over_15"),
            SpeedMph = NumberDisplay(offense?.SpeedMph),
            ZoneMph = NumberDisplay(offense?.ZoneMph),

            OtherViolations = OtherViolationsDisplay(citation, offense, speedBandLabel),
            SourceStateStatute = IsSource(violationSourceLabel, "state"),
            SourceLocalOrdinance = IsSource(violationSourceLabel, "local"),
            ViolationSection = Display(offense?.ViolationSection),

            IsParking = HasLabel(violationGroupLabel, "Parking"),

            AreaBusiness = HasLabel(violationGroupLabel, "Business"),
            AreaSchool = HasLabel(violationGroupLabel, "School"),
            AreaResidential = HasLabel(violationGroupLabel, "Residential"),
            AreaRural = HasLabel(violationGroupLabel, "Rural"),

            Highway2Lane = HasLabel(speedBandLabel, "2 lane"),
            Highway3Lane = HasLabel(speedBandLabel, "3 lane"),
            Highway4Lane = HasLabel(speedBandLabel, "4 lane"),
            Highway4LaneDivided = HasLabel(speedBandLabel, "divided"),

            AcceptedBondNotes = Display(offense?.AcceptedBondNotes),
            ReceiptNumber = Display(offense?.ReceiptNumber),

            AffidavitSignedDate = DateOnlyDisplay(offense?.AffidavitSignedDate),
            ComplainantSignature = OfficerIdentification(citation.OfficerProfile, offense?.ComplainantSignatureText),
            OfficerNameAndTitle = OfficerNameAndTitle(citation.OfficerProfile),
            UnitNumber = Display(citation.OfficerProfile?.UnitNumber),
            CourtAppearanceDay = CourtAppearanceDay(offense?.CourtAppearanceDateTime),
            CourtAppearanceTime = CourtAppearanceTime(offense?.CourtAppearanceDateTime),
            CourtAddress = Display(appearanceLocationDisplay),
            DefendantSignature = Display(offense?.DefendantSignatureText),

            VisibleFlags = ComputeVisibleFlagKeys(citation.ViolationFlags),
        };
    }

    private async Task<LocationDto?> ResolveLocationAsync(Guid? locationId)
        => locationId.HasValue ? await _locationService.GetByIdAsync(locationId.Value) : null;

    /// <summary>
    /// Splits a court picklist label for the "IN THE ___ COURT OF ___" line: "Municipal Court of Riverbend"
    /// gives ("Municipal", "Riverbend"); "Municipal Court" gives ("Municipal", null); "Justice" stays as is.
    /// </summary>
    internal static (string Name, string? Of) SplitCourtLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return (string.Empty, null);

        var text = label.Trim();
        string? of = null;
        var ofIndex = text.IndexOf(" Court of ", StringComparison.OrdinalIgnoreCase);
        if (ofIndex >= 0)
        {
            of = text[(ofIndex + " Court of ".Length)..].Trim();
            text = text[..ofIndex];
        }
        else if (text.EndsWith(" Court", StringComparison.OrdinalIgnoreCase))
        {
            text = text[..^" Court".Length];
        }

        return (text.Trim(), string.IsNullOrWhiteSpace(of) ? null : of);
    }

    /// <summary>
    /// OTHER VIOLATIONS: the officer's narrative, else the primary violation description. When nothing on the
    /// offense panel is filled in at all (no boxes, no speed), fall back to the citation description so the
    /// ticket still states an offense (issue #98).
    /// </summary>
    private static string OtherViolationsDisplay(CitationDto citation, CitationOffenseDetailsDto? offense, string? speedBandLabel)
    {
        var text = Display(offense?.NarrativeOtherViolations, offense?.PrimaryViolationDescription);
        if (!string.IsNullOrWhiteSpace(text))
            return text;

        var panelIsEmpty = (citation.ViolationFlags?.Count ?? 0) == 0
                           && offense?.SpeedMph is null
                           && string.IsNullOrWhiteSpace(speedBandLabel);
        return panelIsEmpty ? Display(citation.Description) : string.Empty;
    }

    // --- Flag visibility ---------------------------------------------------------------------

    // Manual flags always print. Charge-derived flags print only while their originating charge link
    // is still active — that visibility rule is applied here at build time (per-jurisdiction-tunable
    // later) without changing storage. Charge derivation isn't implemented yet, so today the citation
    // carries only manual flags; the charge branch is wired for when it lands.
    private static IReadOnlySet<ViolationFlagKey> ComputeVisibleFlagKeys(IReadOnlyList<CitationViolationFlagDto>? flags)
    {
        var visible = new HashSet<ViolationFlagKey>();
        if (flags is null)
            return visible;

        foreach (var flag in flags)
        {
            if (flag.Source == ViolationFlagSource.Manual)
                visible.Add(flag.Key);
            // else: charge-derived — show only if flag.SourceChargeLinkId is an active charge link.
            // No active-link set is loaded yet because charge derivation is not implemented.
        }

        return visible;
    }

    // --- Display formatting (ported verbatim from CitationTexasPrint.razor) -------------------

    private static string DisplayFullName(CitationDto citation)
    {
        var snapshot = citation.AtTimeOfName;
        if (snapshot is null)
            return Display(citation.DefendantName);

        if (snapshot.NameType == NameTypes.Business || string.IsNullOrWhiteSpace(snapshot.FirstName))
            return Display(snapshot.LastOrBusinessName);

        return $"{snapshot.LastOrBusinessName}, {snapshot.FirstName}";
    }

    // A-4: the "(Name and title)" line previously printed only the title. Render the officer's name
    // with the title, mirroring the form label. Falls back gracefully when either is missing.
    private static string OfficerNameAndTitle(CitationOfficerProfileDto? profile)
    {
        var name = profile?.OfficerName?.Trim();
        var title = profile?.Title?.Trim();

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(title))
            return $"{name}, {title}";

        return Display(name, title);
    }

    // A-3: the badge/ID is captured but was never printed. Render it on the officer-identification
    // line ("(Signature and identification of officer or other complainant)"), appended to the
    // signature text or, absent that, the officer name.
    private static string OfficerIdentification(CitationOfficerProfileDto? profile, string? signatureText)
    {
        var signature = Display(signatureText, profile?.OfficerName);
        var badge = profile?.BadgeOrIdentifier?.Trim();

        if (string.IsNullOrWhiteSpace(badge))
            return signature;

        return string.IsNullOrWhiteSpace(signature) ? $"#{badge}" : $"{signature}  #{badge}";
    }

    private static string CompactRaceDisplay(string? raceLabel)
    {
        var race = raceLabel ?? string.Empty;
        if (race.Contains("Hispanic", StringComparison.OrdinalIgnoreCase))
            return "Hispanic";

        if (race.Length <= 12)
            return race;

        return race[..12];
    }

    private static readonly Dictionary<string, string> StateAbbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Alabama"] = "AL",
        ["Alaska"] = "AK",
        ["Arizona"] = "AZ",
        ["Arkansas"] = "AR",
        ["California"] = "CA",
        ["Colorado"] = "CO",
        ["Connecticut"] = "CT",
        ["Delaware"] = "DE",
        ["District of Columbia"] = "DC",
        ["Florida"] = "FL",
        ["Georgia"] = "GA",
        ["Hawaii"] = "HI",
        ["Idaho"] = "ID",
        ["Illinois"] = "IL",
        ["Indiana"] = "IN",
        ["Iowa"] = "IA",
        ["Kansas"] = "KS",
        ["Kentucky"] = "KY",
        ["Louisiana"] = "LA",
        ["Maine"] = "ME",
        ["Maryland"] = "MD",
        ["Massachusetts"] = "MA",
        ["Michigan"] = "MI",
        ["Minnesota"] = "MN",
        ["Mississippi"] = "MS",
        ["Missouri"] = "MO",
        ["Montana"] = "MT",
        ["Nebraska"] = "NE",
        ["Nevada"] = "NV",
        ["New Hampshire"] = "NH",
        ["New Jersey"] = "NJ",
        ["New Mexico"] = "NM",
        ["New York"] = "NY",
        ["North Carolina"] = "NC",
        ["North Dakota"] = "ND",
        ["Ohio"] = "OH",
        ["Oklahoma"] = "OK",
        ["Oregon"] = "OR",
        ["Pennsylvania"] = "PA",
        ["Rhode Island"] = "RI",
        ["South Carolina"] = "SC",
        ["South Dakota"] = "SD",
        ["Tennessee"] = "TN",
        ["Texas"] = "TX",
        ["Utah"] = "UT",
        ["Vermont"] = "VT",
        ["Virginia"] = "VA",
        ["Washington"] = "WA",
        ["West Virginia"] = "WV",
        ["Wisconsin"] = "WI",
        ["Wyoming"] = "WY",
        ["American Samoa"] = "AS",
        ["Guam"] = "GU",
        ["Northern Mariana Islands"] = "MP",
        ["Puerto Rico"] = "PR",
        ["Virgin Islands"] = "VI"
    };

    private static readonly HashSet<string> StateAbbreviationStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "of", "the", "and"
    };

    private static string CompactState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
            return string.Empty;

        state = state.Trim();

        if (state.Length == 2)
            return state.ToUpperInvariant();

        if (StateAbbreviations.TryGetValue(state, out var mapped))
            return mapped;

        var parts = state.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var significant = parts.Where(p => !StateAbbreviationStopWords.Contains(p)).ToArray();
        if (significant.Length == 0)
            significant = parts;

        string code;
        if (significant.Length == 1)
        {
            var word = significant[0];
            code = word.Length <= 2 ? word.ToUpperInvariant() : word[..2].ToUpperInvariant();
        }
        else
        {
            code = string.Concat(significant.Select(p => char.ToUpperInvariant(p[0])));
            if (code.Length > 2)
                code = code[..2];
        }

        return code;
    }

    private static string CompactAddress(string? address, bool includeStreet)
    {
        if (string.IsNullOrWhiteSpace(address))
            return string.Empty;

        var parts = address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return address;

        if (includeStreet)
            return parts[0];

        if (parts.Length == 1)
            return parts[0];

        return string.Join(", ", parts.Skip(1));
    }

    // B-7: age is the defendant's age at the time of the offense (issue date), not the print date —
    // otherwise a reprint months later shows a different age than the citation was issued with.
    private static string AgeDisplay(DateTime? dob, DateTime issueDate)
    {
        if (!dob.HasValue)
            return string.Empty;

        var asOf = issueDate.Date;
        var age = asOf.Year - dob.Value.Year;
        if (dob.Value.Date > asOf.AddYears(-age))
            age--;

        return age.ToString();
    }

    private static string Display(string? primary, string? fallback = null)
        => string.IsNullOrWhiteSpace(primary) ? (string.IsNullOrWhiteSpace(fallback) ? string.Empty : fallback) : primary;

    private static string NumberDisplay(int? value)
        => value.HasValue ? value.Value.ToString() : string.Empty;

    // Issue and court-appearance values are agency wall-clock times (as entered), printed as stored.
    private static string DateDisplay(DateTime? value)
        => value.HasValue ? value.Value.ToString("MM/dd/yyyy") : string.Empty;

    // Calendar dates (date of birth, affidavit signed date) are not instants — printing them in a
    // different zone could roll a midnight-stored date back a day, so they are rendered as stored.
    private static string DateOnlyDisplay(DateTime? value)
        => value.HasValue ? value.Value.ToString("MM/dd/yyyy") : string.Empty;

    private static string TimeDisplay(DateTime value)
        => value.ToString("h:mm");

    private static string AmPmDisplay(DateTime value)
        => value.ToString("tt");

    private static string DayOfMonthDisplay(DateTime value)
        => value.Day.ToString();

    private static string MonthYearDisplay(DateTime value)
        => value.ToString("MMMM yyyy");

    private static string CourtAppearanceDay(DateTime? value)
        => value.HasValue ? value.Value.ToString("MM/dd/yyyy") : string.Empty;

    private static string CourtAppearanceTime(DateTime? value)
        => value.HasValue ? value.Value.ToString("h:mm tt") : string.Empty;

    private static bool IsSpeedRange(string? speedBandLabel, string token)
    {
        var label = speedBandLabel ?? string.Empty;
        return token switch
        {
            "5-10" => label.Contains("5-10", StringComparison.OrdinalIgnoreCase),
            "11-15" => label.Contains("11-15", StringComparison.OrdinalIgnoreCase),
            "over_15" => label.Contains("Over 15", StringComparison.OrdinalIgnoreCase) || label.Contains("over 15", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static bool IsSource(string? violationSourceLabel, string token)
    {
        var label = violationSourceLabel ?? string.Empty;
        return token switch
        {
            "state" => label.Contains("State", StringComparison.OrdinalIgnoreCase),
            "local" => label.Contains("Local", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static bool HasLabel(string? label, string token)
        => !string.IsNullOrWhiteSpace(label) && label.Contains(token, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Prints a location as a postal address. <see cref="LocationDto.Address"/> is the canonical formatted
    /// address ("88 Lakeview Ct, Riverbend, TX 78640"); the parts are only a fallback. The old version joined the
    /// parts with commas and dropped the street type ("77, Center, Riverbend"), issue #98.
    /// </summary>
    internal static string FormatLocation(LocationDto location)
    {
        if (!string.IsNullOrWhiteSpace(location.Address))
            return TrimCountry(location.Address.Trim());

        var street = string.Join(' ', new[] { location.StreetNumber, location.StreetAddress }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim()));
        if (!string.IsNullOrWhiteSpace(location.AptSuite))
            street = $"{street} {location.AptSuite.Trim()}".Trim();

        return string.Join(", ", new[] { street, location.City }.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static string TrimCountry(string address)
    {
        foreach (var suffix in new[] { ", USA", ", United States", ", US" })
        {
            if (address.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return address[..^suffix.Length].TrimEnd();
        }
        return address;
    }
}
