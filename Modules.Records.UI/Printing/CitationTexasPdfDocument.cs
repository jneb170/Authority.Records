using Modules.Records.Domain.Common.Violations;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Modules.Records.UI.Printing;

/// <summary>
/// Renders the Texas Uniform Traffic Ticket and Complaint (TX17-4R / UTC) form as a fixed-layout PDF
/// from a fully-resolved <see cref="CitationTexasPrintModel"/>. This replaces the former
/// browser-rendered HTML print page, so the output no longer depends on the officer's browser margin
/// or scale settings. The document is pure layout — all data resolution and formatting happens in
/// <see cref="CitationTexasPrintModelBuilder"/>.
/// </summary>
public sealed class CitationTexasPdfDocument : IDocument
{
    private const float LabelSize = 5.4f;
    private const float FillSize = 6.6f;
    private const float BodySize = 5.9f;
    private const float CheckSize = 5.6f;
    private const float LineThickness = 0.7f;

    // The form content is always rendered into a fixed-width block so the two page sizes are spaced
    // identically. 4.00in page − 2 × 0.18in margin = 3.64in usable; the Letter variant pins the same
    // 3.64in block on the left and leaves the rest of the page blank for the future back-of-ticket print.
    private const float FormWidthInches = 3.64f;
    private const float MarginInches = 0.18f;

    private readonly CitationTexasPrintModel _model;
    private readonly CitationPrintSize _size;

    public CitationTexasPdfDocument(CitationTexasPrintModel model, CitationPrintSize size = CitationPrintSize.Letter)
    {
        _model = model;
        _size = size;
    }

    public DocumentMetadata GetMetadata() => new() { Title = _model.DocumentTitle };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            // Use Lato (bundled and registered by QuestPDF on every platform) — NOT a system font
            // like Arial. Linux App Service has no Arial, so it silently fell back to a wider font
            // whose metrics overflowed this dense layout and threw a layout exception for every
            // citation. Lato renders identically on Windows and Linux, so local output matches prod.
            page.Margin(MarginInches, Unit.Inch);
            page.DefaultTextStyle(t => t.FontSize(FillSize).FontColor(Colors.Black).FontFamily("Lato"));

            if (_size == CitationPrintSize.FourInch)
            {
                // Patrol-car thermal printer: a single 4in-wide page with unbounded height (no page
                // breaks), so the whole ticket prints as one continuous strip.
                page.ContinuousSize(4f, Unit.Inch);
                page.Content().Element(ComposeForm);
            }
            else
            {
                // Letter: the SAME fixed-width form block anchored left; the empty RelativeItem is the
                // blank right half reserved for printing the back of the ticket on the same page later.
                // AutoItem (not RelativeItem) so the form keeps its pinned width and cannot stretch.
                page.Size(PageSizes.Letter);
                page.Content().Row(row =>
                {
                    row.AutoItem().Element(ComposeForm);
                    row.RelativeItem();
                });
            }
        });
    }

    /// <summary>
    /// Builds the form into a fixed-width block. Rendered identically for both page sizes — the only
    /// difference between 4-inch and Letter output is the page wrapper around this block.
    /// </summary>
    private void ComposeForm(IContainer container)
    {
        container.Width(FormWidthInches, Unit.Inch).Border(0.8f).Padding(4).Column(root =>
        {
            root.Spacing(2);

            ComposeHeader(root);
            ComposeAffidavitIntro(root);
            ComposeTimeRow(root);
            ComposeDefendant(root);
            ComposeVehicle(root);
            ComposeOffenseLocation(root);
            ComposeMidLayout(root);
            ComposeFooter(root);
        });
    }

    // ---- Header -----------------------------------------------------------------------------

    private void ComposeHeader(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().AlignLeft().Text("Authority Records").FontSize(5.5f);
            row.RelativeItem().AlignCenter().Text("G.A. THOMPSON, P.O. BOX 720254, DALLAS, TEXAS 75372").FontSize(5.5f);
            row.RelativeItem().AlignRight().Text("TX17-4R (NCR)").FontSize(5.5f);
        });

        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);

        col.Item().AlignCenter().Text("UNIFORM TRAFFIC TICKET AND COMPLAINT")
            .FontSize(9.5f).Bold().LetterSpacing(0.01f);

        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(2.2f).Element(c => Field(c, "CASE NO.", _model.CaseNo));
            row.RelativeItem(1.5f).Element(c => Field(c, "DOCKET NO.", _model.DocketNo));
            row.RelativeItem(0.9f).Element(c => Field(c, "PAGE NO.", _model.PageNo));
        });

        col.Item().AlignCenter().Text(text =>
        {
            text.Span("IN THE NAME AND BY THE AUTHORITY OF THE STATE OF TEXAS").FontSize(BodySize);
        });

        col.Item().AlignRight().Text("COMPLAINT - AFFIDAVIT").FontSize(8.8f).Bold();
    }

    private void ComposeAffidavitIntro(ColumnDescriptor col)
    {
        col.Item().Text(text =>
        {
            text.DefaultTextStyle(t => t.FontSize(BodySize));
            text.Span("IN THE ");
            text.Span(_model.CourtLabel).Underline();
            text.Span(" COURT OF ");
            text.Span(_model.AppearanceOrCitationLocation).Underline();
        });

        col.Item().Text("THE UNDERSIGNED, BEING DULY SWORN, UPON HIS OATH DEPOSES AND SAYS:")
            .FontSize(BodySize);
    }

    private void ComposeTimeRow(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(1.4f).Element(c => Field(c, "ON", _model.IssueDate));
            row.RelativeItem(0.7f).Element(c => Field(c, "THE", _model.IssueDayOfMonth));
            row.RelativeItem(1.1f).Element(c => Field(c, "DAY OF", _model.IssueMonthYear));
            row.RelativeItem(0.9f).Element(c => Field(c, "AT", _model.IssueTime));
            row.AutoItem().AlignBottom().PaddingLeft(2).Text(_model.IssueAmPm).FontSize(6);
        });
    }

    private void ComposeDefendant(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(1.25f).Element(c => Field(c, "NAME (LAST)", _model.LastName));
            row.RelativeItem(1.2f).Element(c => Field(c, "(FIRST)", _model.FirstName));
            row.RelativeItem(0.45f).Element(c => Field(c, "(INIT)", _model.Initial));
        });

        col.Item().Element(c => Field(c, "STREET", _model.AddressStreet));

        // Identity split across rows — seven fields on one line cannot fit the ~262pt width. City/State
        // gets its own line (it is the longest), so AGE/BIRTH DATE/RACE keep enough width to print a
        // full date without truncating.
        col.Item().Element(c => Field(c, "CITY - STATE", _model.CityState));

        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(0.7f).Element(c => Field(c, "AGE", _model.Age));
            row.RelativeItem(1.4f).Element(c => Field(c, "BIRTH DATE", _model.BirthDate));
            row.RelativeItem(1f).Element(c => Field(c, "RACE", _model.Race));
        });

        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(1f).Element(c => Field(c, "SEX", _model.Sex));
            row.RelativeItem(0.8f).Element(c => Field(c, "HT", _model.Height));
            row.RelativeItem(0.8f).Element(c => Field(c, "WT", _model.Weight));
        });

        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(2f).Element(c => Field(c, "DRIV. LIC. NO.", _model.DriversLicenseNumber));
            row.RelativeItem(0.8f).Element(c => Field(c, "STATE", _model.DriversLicenseState));
            row.RelativeItem(0.8f).Element(c => Field(c, "KIND", string.Empty));
        });

        col.Item().AlignRight().Text("DID UNLAWFULLY OPERATE (PARK)").FontSize(5.4f);
    }

    private void ComposeVehicle(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Element(c => Check(c, _model.VehicleIsCommercial, "COMMERCIAL VEHICLE"));
            row.RelativeItem().Element(c => Check(c, _model.VehicleCarriesHazmat, "HAZARDOUS MATERIAL"));
        });

        // Vehicle details split across two rows for the narrow width.
        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(1.6f).Element(c => Field(c, "VEH. LIC. NO.", _model.PlateNumber));
            row.RelativeItem(0.7f).Element(c => Field(c, "YR.", _model.PlateYear));
            row.RelativeItem(0.9f).Element(c => Field(c, "STATE", _model.PlateState));
        });

        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(0.7f).Element(c => Field(c, "YR.", _model.ModelYear));
            row.RelativeItem(1.2f).Element(c => Field(c, "MAKE", _model.Make));
            row.RelativeItem(1f).Element(c => Field(c, "STYLE", _model.Style));
            row.RelativeItem(0.9f).Element(c => Field(c, "COLOR", _model.Color));
        });
    }

    private void ComposeOffenseLocation(ColumnDescriptor col)
    {
        col.Item().Text(text =>
        {
            text.DefaultTextStyle(t => t.FontSize(BodySize));
            text.Span("UPON A PUBLIC STREET OR HIGHWAY, NAMELY ");
            text.Span(_model.OccurredAt).Underline();
            text.Span(" LOCATED IN THE CITY, VILLAGE, TOWNSHIP, COUNTY AND STATE AFORESAID AND DID THEN AND THERE COMMIT THE FOLLOWING OFFENSE.");
        });
    }

    // ---- Offense panel + side strip ---------------------------------------------------------

    private void ComposeMidLayout(ColumnDescriptor col)
    {
        // At ~3.64in usable width the former side-by-side layout (a ~1.33in side strip beside the
        // dense checkbox grid) starves the offense panel, so the side strip is stacked BELOW the
        // panel as a full-width section. This keeps the printed form's top-to-bottom order.
        col.Item().Border(0.7f).Padding(3).Column(ComposeOffensePanel);
        col.Item().Element(ComposeSideStrip);
    }

    private void ComposeOffensePanel(ColumnDescriptor panel)
    {
        panel.Spacing(2);

        panel.Item().Row(row =>
        {
            row.AutoItem().AlignBottom().Text("SPEEDING").FontSize(6.2f).Bold();
            row.RelativeItem().PaddingLeft(4).AlignBottom().Text("(over limit)").FontSize(5.2f);
        });

        // Speeding split into two rows — the three over-limit boxes plus both fields will not fit one row.
        panel.Item().Row(row =>
        {
            row.Spacing(4);
            row.RelativeItem().Element(c => Check(c, _model.SpeedRange5To10, "5-10 m.p.h."));
            row.RelativeItem().Element(c => Check(c, _model.SpeedRange11To15, "11-15 m.p.h."));
            row.RelativeItem().Element(c => Check(c, _model.SpeedRangeOver15, "over 15 m.p.h."));
        });

        panel.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem().Element(c => Field(c, "MPH", _model.SpeedMph));
            row.RelativeItem(1.6f).Element(c => Field(c, "in ____ MPH zone", _model.ZoneMph));
        });

        // Four-column checkbox grid (mirrors the printed form's panels 1:1).
        panel.Item().Row(row =>
        {
            row.Spacing(4);

            row.RelativeItem().Column(c =>
            {
                Check(c.Item(), _model.Flag(ViolationFlagKey.UnreasonableForConditions), "Unreasonable for conditions");
                Check(c.Item(), _model.Flag(ViolationFlagKey.UnableToStop), "Unable to stop in assured clear distance ahead");
                Check(c.Item(), _model.Flag(ViolationFlagKey.ImproperLeftTurn), "Improper LEFT TURN");
                Check(c.Item(), _model.Flag(ViolationFlagKey.ImproperRightTurn), "Improper RIGHT TURN");
                Check(c.Item(), _model.Flag(ViolationFlagKey.ImproperPassingAndLaneUsage), "Improper PASSING AND LANE USAGE");
            });

            row.RelativeItem().Column(c =>
            {
                Check(c.Item(), _model.Flag(ViolationFlagKey.NoSignal), "No Signal");
                Check(c.Item(), _model.Flag(ViolationFlagKey.SignalDeviceDisobeyed), "Signal device disobeyed");
                Check(c.Item(), _model.Flag(ViolationFlagKey.WrongPlace), "Wrong place");
                Check(c.Item(), _model.Flag(ViolationFlagKey.AtIntersection), "At intersection");
                Check(c.Item(), _model.Flag(ViolationFlagKey.Lane), "Lane");
                Check(c.Item(), _model.Flag(ViolationFlagKey.Straddling), "Straddling");
            });

            row.RelativeItem().Column(c =>
            {
                Check(c.Item(), _model.Flag(ViolationFlagKey.CutCorner), "Cut corner");
                Check(c.Item(), _model.Flag(ViolationFlagKey.IntoWrongLane), "Into wrong lane");
                Check(c.Item(), _model.Flag(ViolationFlagKey.MiddleOfIntersection), "Middle of intersection");
                Check(c.Item(), _model.Flag(ViolationFlagKey.WalkSpeed), "Walk speed");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CutIn), "Cut in");
                Check(c.Item(), _model.Flag(ViolationFlagKey.OnRight), "On right");
                Check(c.Item(), _model.Flag(ViolationFlagKey.WrongLane), "Wrong lane");
            });

            row.RelativeItem().Column(c =>
            {
                Check(c.Item(), _model.Flag(ViolationFlagKey.FromWrongLane), "From wrong lane");
                Check(c.Item(), _model.Flag(ViolationFlagKey.WrongSideOfPavement), "Wrong side of pavement");
                Check(c.Item(), _model.Flag(ViolationFlagKey.Faster), "Faster");
                Check(c.Item(), _model.Flag(ViolationFlagKey.OnHill), "On hill");
                Check(c.Item(), _model.Flag(ViolationFlagKey.OnCurve), "On curve");
            });
        });

        // Other violations
        panel.Item().PaddingTop(2).BorderTop(0.6f).PaddingTop(3).Column(c =>
        {
            c.Item().Element(x => Field(x, "OTHER VIOLATIONS (describe)", _model.OtherViolations));
            c.Item().PaddingTop(2).Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem().Element(x => Check(x, _model.SourceStateStatute, "State Statute"));
                row.RelativeItem().Element(x => Check(x, _model.SourceLocalOrdinance, "Local Ordinance"));
                row.RelativeItem().Element(x => Field(x, "Sec.", _model.ViolationSection));
            });
        });

        // Parking
        panel.Item().Row(row =>
        {
            row.Spacing(8);
            row.AutoItem().AlignBottom().Text("PARKING").FontSize(5.8f).Bold();
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Overtime"));
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Area"));
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Double Parking"));
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Expired Meter"));
        });

        // Bottom three mini-panels
        panel.Item().BorderTop(0.6f).PaddingTop(3).Row(row =>
        {
            row.Spacing(4);

            row.RelativeItem().Border(0.6f).Padding(2).Column(c =>
            {
                c.Item().Text("CONTRIBUTORS TO LAST VIOLATION").FontSize(5.6f).Bold();
                Check(c.Item(), _model.Flag(ViolationFlagKey.Rain), "Rain");
                Check(c.Item(), _model.Flag(ViolationFlagKey.Snow), "Snow");
                Check(c.Item(), _model.Flag(ViolationFlagKey.Ice), "Ice");
                Check(c.Item(), _model.Flag(ViolationFlagKey.Night), "Night");
                Check(c.Item(), _model.Flag(ViolationFlagKey.Fog), "Fog");
            });

            row.RelativeItem().Border(0.6f).Padding(2).Column(c =>
            {
                c.Item().Text("CAUSED PERSON TO DODGE").FontSize(5.6f).Bold();
                Check(c.Item(), _model.Flag(ViolationFlagKey.DodgePedestrian), "Pedestrian");
                Check(c.Item(), _model.Flag(ViolationFlagKey.DodgeDriver), "Driver");
                Check(c.Item(), _model.Flag(ViolationFlagKey.JustMissedAccident), "Just missed accident");
            });

            row.RelativeItem(1.15f).Border(0.6f).Padding(2).Column(c =>
            {
                c.Item().Text("TYPE OF COLLISION").FontSize(5.6f).Bold();
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionPropertyDamage), "PD");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionPersonalInjury), "PI");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionFatal), "Fatal");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionVehicle), "Vehicle");
                Check(c.Item(), _model.Flag(ViolationFlagKey.HitFixedObject), "Hit fixed object");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionRightAngle), "Right angle");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionHeadOn), "Head on");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionSideswipe), "Sideswipe");
                Check(c.Item(), _model.Flag(ViolationFlagKey.CollisionRearEnd), "Rear end");
            });
        });

        // Area
        panel.Item().Row(row =>
        {
            row.Spacing(8);
            row.AutoItem().AlignBottom().Text("AREA:").FontSize(5.8f);
            row.RelativeItem().Element(c => Check(c, _model.AreaBusiness, "Business"));
            row.RelativeItem().Element(c => Check(c, _model.AreaSchool, "School"));
            row.RelativeItem().Element(c => Check(c, _model.AreaResidential, "Residential"));
            row.RelativeItem().Element(c => Check(c, _model.AreaRural, "Rural"));
        });

        // Highway type
        panel.Item().Row(row =>
        {
            row.Spacing(8);
            row.AutoItem().AlignBottom().Text("HIGHWAY TYPE:").FontSize(5.8f);
            row.RelativeItem().Element(c => Check(c, _model.Highway2Lane, "2 lane undivided"));
            row.RelativeItem().Element(c => Check(c, _model.Highway3Lane, "3 lane undivided"));
            row.RelativeItem().Element(c => Check(c, _model.Highway4Lane, "4 lane undivided"));
            row.RelativeItem().Element(c => Check(c, _model.Highway4LaneDivided, "4 lane divided"));
        });
    }

    private void ComposeSideStrip(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(2);
            SideField(col.Item(), "Accepted Bond Amt. or Type", _model.AcceptedBondNotes);
            SideField(col.Item(), "Receipt No.", _model.ReceiptNumber);
            SideField(col.Item(), "Name", _model.FullName);
            SideField(col.Item(), "Occupation", string.Empty);
            SideField(col.Item(), "Social Security Number", _model.SocialSecurityNumber);
        });
    }

    private static void SideField(IContainer container, string label, string value)
    {
        container.Border(0.6f).Padding(2).Column(col =>
        {
            col.Item().Text(label).FontSize(5f);
            col.Item().MinHeight(13).PaddingTop(1).Text(t =>
            {
                // One clamped line so an oversized value (e.g. a long name) can't grow the box and
                // shove the sections below it; shrink the font first to fit more before truncating.
                t.ClampLines(1);
                t.DefaultTextStyle(s => s.FontSize(FitFontSize(value, 5.8f, 24)));
                t.Span(value ?? string.Empty);
            });
        });
    }

    // ---- Footer / signatures ----------------------------------------------------------------

    private void ComposeFooter(ColumnDescriptor col)
    {
        col.Item().Text("THE UNDERSIGNED FURTHER STATES THAT HE HAS JUST AND REASONABLE GROUNDS TO BELIEVE, AND DOES BELIEVE, THAT THE PERSON NAMED ABOVE COMMITTED THE OFFENSE HEREIN SET FORTH.")
            .FontSize(BodySize);

        col.Item().Text("CONTRARY TO LAW AND AGAINST THE PEACE AND DIGNITY OF THE STATE. SWORN TO AND SUBSCRIBED BEFORE ME")
            .FontSize(BodySize);

        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(0.8f).Element(c => Field(c, "THIS", _model.AffidavitSignedDate));
            // Long caption — stack it below the line so it can't crowd out the fill on a narrow row.
            row.RelativeItem(1.7f).Element(c => StackedField(c, "(Signature and identification of officer or other complainant)", _model.ComplainantSignature));
        });

        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(1.6f).Element(c => StackedField(c, "(Name and title)", _model.OfficerNameAndTitle));
            row.RelativeItem(0.7f).Element(c => StackedField(c, "(Unit No.)", _model.UnitNumber));
        });

        col.Item().Element(c => Field(c, "COURT APPEARANCE", _model.CourtLabel));

        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(1f).Element(c => Field(c, "DAY OF", _model.CourtAppearanceDay));
            row.RelativeItem(1f).Element(c => Field(c, "AT", _model.CourtAppearanceTime));
        });

        col.Item().Element(c => Field(c, "ADDRESS OF COURT", _model.CourtAddress));

        col.Item().Text("I PROMISE TO APPEAR IN SAID COURT OR BUREAU AT SAID TIME AND PLACE.").FontSize(BodySize);

        col.Item().Element(c => Field(c, "SIGNATURE", _model.DefendantSignature));

        col.Item().Text("NOTICE, UNDERSTANDING THAT FAILURE TO APPEAR CONSTITUTES A SEPARATE OFFENSE.")
            .FontSize(5.6f);
    }

    // ---- Reusable primitives ----------------------------------------------------------------

    /// <summary>
    /// A field whose caption sits BELOW the underlined fill (signature-line style). Used where the
    /// caption is too long to sit inline on a narrow row without stealing the fill's width.
    /// </summary>
    private static void StackedField(IContainer container, string caption, string value)
    {
        container.Column(col =>
        {
            col.Item().BorderBottom(LineThickness).MinHeight(11).PaddingHorizontal(1).PaddingBottom(1)
                .AlignBottom().Text(t =>
                {
                    t.ClampLines(1);
                    t.DefaultTextStyle(s => s.FontSize(FitFontSize(value, FillSize, 24)));
                    t.Span(value ?? string.Empty);
                });
            col.Item().Text(caption).FontSize(4.8f).FontColor(Colors.Grey.Darken2);
        });
    }

    /// <summary>A labeled field: small caption followed by an underlined fill carrying the value.</summary>
    private static void Field(IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            if (!string.IsNullOrEmpty(label))
                row.AutoItem().PaddingRight(2).AlignBottom().Text(label).FontSize(LabelSize);

            row.RelativeItem().BorderBottom(LineThickness).MinHeight(9).PaddingBottom(1).PaddingHorizontal(1)
                .AlignBottom().Text(t =>
                {
                    // A data fill must never grow the row and shift neighbouring fields. Clamp to one
                    // line (truncate with an ellipsis) and shrink the font for long values so more fits
                    // before truncation — never let it wrap or expand the layout.
                    t.ClampLines(1);
                    t.DefaultTextStyle(s => s.FontSize(FitFontSize(value, FillSize, 22)));
                    t.Span(value ?? string.Empty);
                });
        });
    }

    /// <summary>
    /// Shrink-to-fit font size for a data value: keeps <paramref name="baseSize"/> up to
    /// <paramref name="comfortable"/> characters, then steps down so longer values stay on one line
    /// before the caller's line clamp truncates them. Heuristic (QuestPDF has no native auto-fit).
    /// </summary>
    private static float FitFontSize(string? value, float baseSize, int comfortable)
    {
        var len = value?.Length ?? 0;
        if (len <= comfortable) return baseSize;
        if (len <= comfortable * 1.5) return baseSize - 0.8f;
        if (len <= comfortable * 2.1) return baseSize - 1.4f;
        return baseSize - 1.9f;
    }

    /// <summary>A checkbox followed by its caption.</summary>
    private static void Check(IContainer container, bool on, string caption)
    {
        container.Row(row =>
        {
            row.ConstantItem(8).AlignBottom().Element(c => Box(c, on));
            // LineHeight < 1 compresses captions that wrap to 2-3 lines in the narrow grid columns,
            // keeping the form short enough that the Letter front fits on a single page.
            row.RelativeItem().PaddingLeft(2).AlignBottom().Text(caption).FontSize(CheckSize).LineHeight(0.95f);
        });
    }

    private static void Box(IContainer container, bool on)
    {
        var box = container.Width(7).Height(7).Border(0.6f).AlignCenter().AlignMiddle();

        // Only emit a text line when the box is ticked. An empty Text still reserves a full line
        // box, and at this size the rendering font's line height can exceed the 7pt box height —
        // which throws a layout exception under fonts with taller metrics than Arial (e.g. the Lato
        // fallback used on Linux). LineHeight(1) pins the line box to the glyph size so the "X" fits.
        if (on)
            box.Text("X").FontSize(5.5f).LineHeight(1f).Bold();
    }
}
