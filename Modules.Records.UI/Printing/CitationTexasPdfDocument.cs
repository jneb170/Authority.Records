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
///
/// The layout is a faithful (human-eye, not pixel-exact) reproduction of the State master form: the
/// defendant block packs onto the same lines with the same stacked sub-labels, and the offense box is
/// flanked by the same rotated vertical margin fields (ACCIDENT CASE / Leading Causes of Accidents and
/// Conditions that Increased Seriousness of Violation on the left; Arrest-Delivered to, Accepted
/// Bond-Amt. or Type, Receipt No., NAME, Occupation, Social Security Number on the right).
/// </summary>
public sealed class CitationTexasPdfDocument : IDocument
{
    // The State form face is Arial/Helvetica. We cannot use the system "Arial" because Linux App Service
    // has no Arial and silently falls back to a wider font whose metrics overflow this dense layout and
    // throw (see project_citation_pdf_linux_font). So we bundle Arimo — metric-identical to Arial,
    // freely licensed (OFL) — embedded in this assembly and registered in the static constructor below,
    // so it matches the State face AND renders identically on Windows and Linux.
    private const string FontFamily = "Arimo";

    static CitationTexasPdfDocument()
    {
        // Register the embedded Arimo faces with QuestPDF before any render. Runs in both the web host
        // and the test host (the type is touched before GeneratePdf), so neither relies on a system font.
        var assembly = typeof(CitationTexasPdfDocument).Assembly;
        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (!resource.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                !resource.Contains("Arimo", StringComparison.OrdinalIgnoreCase))
                continue;

            using var stream = assembly.GetManifestResourceStream(resource);
            if (stream is not null)
                QuestPDF.Drawing.FontManager.RegisterFont(stream);
        }
    }

    private const float LabelSize = 5.4f;
    private const float FillSize = 6.6f;
    private const float BodySize = 5.9f;
    private const float CheckSize = 5.6f;
    private const float SubLabelSize = 4.4f;
    private const float VLabelSize = 5f;
    private const float LineThickness = 0.7f;

    // The form content is always rendered into a fixed-width block so the two page sizes are spaced
    // identically. 4.00in page − 2 × 0.18in margin = 3.64in usable; this is essentially the State form's
    // true width, so it reproduces 1:1 on a 4in thermal roll. The Letter variant pins the same 3.64in
    // block on the left and leaves the rest of the page blank for the future back-of-ticket print.
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
            page.Margin(MarginInches, Unit.Inch);
            page.DefaultTextStyle(t => t.FontSize(FillSize).FontColor(Colors.Black).FontFamily(FontFamily));

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
            ComposeOffenseBox(root);
            ComposeParking(root);
            ComposeConditionsBox(root);
            ComposeAreaAndHighway(root);
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

        col.Item().Row(row =>
        {
            row.RelativeItem().AlignLeft().Text("IN THE NAME AND BY THE AUTHORITY OF THE STATE OF TEXAS").FontSize(BodySize);
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
        // NAME: a single underline carrying LAST / FIRST / INITIAL, with the State form's sub-labels
        // printed BENEATH the line at the matching positions (not as inline field labels).
        col.Item().Column(name =>
        {
            name.Item().Row(row =>
            {
                row.AutoItem().PaddingRight(2).AlignBottom().Text("NAME").FontSize(LabelSize);
                row.RelativeItem().BorderBottom(LineThickness).MinHeight(9).PaddingHorizontal(1).PaddingBottom(1)
                    .AlignBottom().Row(fill =>
                    {
                        fill.RelativeItem(3).Element(c => Fill(c, _model.LastName));
                        fill.RelativeItem(3).Element(c => Fill(c, _model.FirstName));
                        fill.RelativeItem(1).Element(c => Fill(c, _model.Initial));
                    });
            });

            // Sub-labels beneath the line. The left third carries both LAST and (PLEASE PRINT).
            name.Item().PaddingLeft(20).Row(sub =>
            {
                sub.RelativeItem(3).Row(left =>
                {
                    left.RelativeItem().AlignLeft().Text("LAST").FontSize(SubLabelSize);
                    left.RelativeItem().AlignCenter().Text("(PLEASE PRINT)").FontSize(SubLabelSize);
                });
                sub.RelativeItem(3).AlignLeft().Text("FIRST").FontSize(SubLabelSize);
                sub.RelativeItem(1).AlignLeft().Text("INITIAL").FontSize(SubLabelSize);
            });
        });

        col.Item().Element(c => Field(c, "STREET", _model.AddressStreet));
        col.Item().Element(c => Field(c, "CITY - STATE", _model.CityState));

        // AGE / BIRTH DATE / RACE / SEX / HT. / WT. on ONE line, matching the master. BIRTH DATE uses a
        // two-line stacked label (BIRTH over DATE) sitting left of its fill.
        col.Item().Row(row =>
        {
            row.Spacing(4);
            row.RelativeItem(1.0f).Element(c => Field(c, "AGE", _model.Age));
            row.RelativeItem(1.7f).Element(c => StackedLabelField(c, "BIRTH", "DATE", _model.BirthDate));
            row.RelativeItem(1.2f).Element(c => Field(c, "RACE", _model.Race));
            row.RelativeItem(1.2f).Element(c => Field(c, "SEX", _model.Sex));
            row.RelativeItem(0.9f).Element(c => Field(c, "HT.", _model.Height));
            row.RelativeItem(0.9f).Element(c => Field(c, "WT.", _model.Weight));
        });

        // DRIV. / LIC. No. stacked label; STATE and KIND are sub-labels under the same continuing line,
        // and DID UNLAWFULLY OPERATE (PARK) trails at the right of that line.
        col.Item().Row(row =>
        {
            row.Spacing(3);
            row.AutoItem().AlignBottom().Column(lbl =>
            {
                lbl.Item().Text("DRIV.").FontSize(LabelSize);
                lbl.Item().Text("LIC. No.").FontSize(LabelSize);
            });
            row.RelativeItem(1.6f).Element(c => SubLabelledFill(c, _model.DriversLicenseNumber, "STATE", _model.DriversLicenseState, "KIND", string.Empty));
            row.AutoItem().AlignBottom().PaddingLeft(2).Text("DID UNLAWFULLY OPERATE (PARK)").FontSize(LabelSize);
        });

        // COMMERCIAL VEHICLE and HAZARDOUS MATERIAL: label first, then the checkbox (master order).
        col.Item().Row(row =>
        {
            row.Spacing(10);
            row.AutoItem().Element(c => CheckAfter(c, _model.VehicleIsCommercial, "COMMERCIAL VEHICLE"));
            row.AutoItem().Element(c => CheckAfter(c, _model.VehicleCarriesHazmat, "HAZARDOUS MATERIAL"));
        });
    }

    private void ComposeVehicle(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.Spacing(4);
            row.RelativeItem(2.0f).Element(c => Field(c, "VEH. LIC. No.", _model.PlateNumber));
            row.RelativeItem(0.8f).Element(c => Field(c, "YR.", _model.PlateYear));
            row.RelativeItem(1.0f).Element(c => Field(c, "STATE", _model.PlateState));
        });

        col.Item().Row(row =>
        {
            row.Spacing(4);
            row.RelativeItem(0.8f).Element(c => Field(c, "YR.", _model.ModelYear));
            row.RelativeItem(1.3f).Element(c => Field(c, "MAKE", _model.Make));
            row.RelativeItem(1.1f).Element(c => Field(c, "STYLE", _model.Style));
            row.RelativeItem(1.0f).Element(c => Field(c, "COLOR", _model.Color));
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

    // ---- Offense box (flanked by vertical margin fields) ------------------------------------

    private void ComposeOffenseBox(ColumnDescriptor col)
    {
        // The main offense box is flanked the way the master form is: ACCIDENT CASE / Leading Causes of
        // Accidents reads up the left edge; the right edge carries the Arrest-Delivered to / Accepted
        // Bond / Receipt No. / NAME / Occupation / Social Security Number vertical fields.
        col.Item().Row(row =>
        {
            row.ConstantItem(20).Element(c => LeftAccidentStrip(c));
            row.RelativeItem().Border(0.7f).Padding(3).Column(ComposeOffensePanel);
            row.ConstantItem(58).Element(c => RightMarginStrip(c));
        });
    }

    private void LeftAccidentStrip(IContainer container)
    {
        container.Row(strip =>
        {
            strip.ConstantItem(10).BorderLeft(LineThickness).AlignBottom().PaddingBottom(1)
                .RotateLeft().Row(r =>
                {
                    r.AutoItem().Text("ACCIDENT CASE").FontSize(VLabelSize).Bold();
                    r.AutoItem().PaddingLeft(2).Element(c => Box(c, false));
                });
            strip.ConstantItem(9).AlignBottom().RotateLeft().Text("Leading Causes of Accidents").FontSize(VLabelSize - 0.5f);
        });
    }

    private void RightMarginStrip(IContainer container)
    {
        // Reads (left→right in the right margin): the upper Arrest/Bond/Receipt inner column, then the
        // tall NAME / Occupation / Social Security Number lines down the far edge.
        container.Row(strip =>
        {
            VField(strip.ConstantItem(9), "Arrest-Delivered to", string.Empty, withBox: true);
            VField(strip.ConstantItem(9), "Accepted Bond-Amt. or Type", _model.AcceptedBondNotes);
            VField(strip.ConstantItem(9), "Receipt No.", _model.ReceiptNumber);
            VField(strip.ConstantItem(10), "NAME", _model.FullName);
            VField(strip.ConstantItem(10), "Occupation", string.Empty);
            VField(strip.ConstantItem(11), "Social Security Number", _model.SocialSecurityNumber);
        });
    }

    /// <summary>A rotated (bottom-to-top) margin field: a vertical line with its label at the base, and
    /// the value written up the line above the label.</summary>
    private static void VField(IContainer container, string label, string value, bool withBox = false)
    {
        container.BorderLeft(LineThickness).PaddingHorizontal(1).AlignBottom().RotateLeft().Row(r =>
        {
            if (withBox)
                r.AutoItem().PaddingRight(2).AlignMiddle().Element(c => Box(c, false));
            r.AutoItem().Text(label).FontSize(VLabelSize);
            if (!string.IsNullOrEmpty(value))
                r.AutoItem().PaddingLeft(3).Text(value).FontSize(VLabelSize);
        });
    }

    private void ComposeOffensePanel(ColumnDescriptor panel)
    {
        panel.Spacing(2);

        panel.Item().Row(row =>
        {
            row.AutoItem().AlignBottom().Text("SPEEDING").FontSize(6.2f).Bold();
            row.RelativeItem().PaddingLeft(4).AlignBottom().Text("(over limit)").FontSize(5.2f);
        });

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
    }

    private void ComposeParking(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.Spacing(8);
            row.AutoItem().AlignBottom().Text("PARKING").FontSize(5.8f).Bold();
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Overtime"));
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Area"));
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Double Parking"));
            row.RelativeItem().Element(c => Check(c, _model.IsParking, "Expired Meter"));
        });
    }

    private void ComposeConditionsBox(ColumnDescriptor col)
    {
        // The lower box is flanked on the left by the rotated "Conditions that Increased Seriousness of
        // Violation" label.
        col.Item().Row(row =>
        {
            row.ConstantItem(11).BorderLeft(LineThickness).AlignBottom().PaddingBottom(1)
                .RotateLeft().Text("Conditions that Increased Seriousness of Violation").FontSize(VLabelSize - 0.5f);

            row.RelativeItem().BorderTop(0.6f).PaddingTop(3).Row(inner =>
            {
                inner.Spacing(4);

                inner.RelativeItem().Border(0.6f).Padding(2).Column(c =>
                {
                    c.Item().Text("CONTRIBUTORS TO LAST VIOLATION").FontSize(5.6f).Bold();
                    Check(c.Item(), _model.Flag(ViolationFlagKey.Rain), "Rain");
                    Check(c.Item(), _model.Flag(ViolationFlagKey.Snow), "Snow");
                    Check(c.Item(), _model.Flag(ViolationFlagKey.Ice), "Ice");
                    Check(c.Item(), _model.Flag(ViolationFlagKey.Night), "Night");
                    Check(c.Item(), _model.Flag(ViolationFlagKey.Fog), "Fog");
                });

                inner.RelativeItem().Border(0.6f).Padding(2).Column(c =>
                {
                    c.Item().Text("CAUSED PERSON TO DODGE").FontSize(5.6f).Bold();
                    Check(c.Item(), _model.Flag(ViolationFlagKey.DodgePedestrian), "Pedestrian");
                    Check(c.Item(), _model.Flag(ViolationFlagKey.DodgeDriver), "Driver");
                    Check(c.Item(), _model.Flag(ViolationFlagKey.JustMissedAccident), "Just missed accident");
                });

                inner.RelativeItem(1.15f).Border(0.6f).Padding(2).Column(c =>
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
        });
    }

    private void ComposeAreaAndHighway(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.Spacing(8);
            row.AutoItem().AlignBottom().Text("AREA:").FontSize(5.8f);
            row.RelativeItem().Element(c => Check(c, _model.AreaBusiness, "Business"));
            row.RelativeItem().Element(c => Check(c, _model.AreaSchool, "School"));
            row.RelativeItem().Element(c => Check(c, _model.AreaResidential, "Residential"));
            row.RelativeItem().Element(c => Check(c, _model.AreaRural, "Rural"));
        });

        col.Item().Row(row =>
        {
            row.Spacing(8);
            row.AutoItem().AlignBottom().Text("HIGHWAY TYPE:").FontSize(5.8f);
            row.RelativeItem().Element(c => Check(c, _model.Highway2Lane, "2 lane undivided"));
            row.RelativeItem().Element(c => Check(c, _model.Highway3Lane, "3 lane undivided"));
            row.RelativeItem().Element(c => Check(c, _model.Highway4Lane, "4 lane undivided"));
            row.RelativeItem().Element(c => Check(c, _model.Highway4LaneDivided, "4 lane divided"));
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
                .AlignBottom().Element(c => Fill(c, value));
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
                .AlignBottom().Element(c => Fill(c, value));
        });
    }

    /// <summary>A field with a two-line stacked label (e.g. BIRTH / DATE) sitting left of the fill.</summary>
    private static void StackedLabelField(IContainer container, string labelLine1, string labelLine2, string value)
    {
        container.Row(row =>
        {
            row.AutoItem().PaddingRight(2).AlignBottom().Column(lbl =>
            {
                lbl.Item().Text(labelLine1).FontSize(LabelSize);
                lbl.Item().Text(labelLine2).FontSize(LabelSize);
            });
            row.RelativeItem().BorderBottom(LineThickness).MinHeight(9).PaddingBottom(1).PaddingHorizontal(1)
                .AlignBottom().Element(c => Fill(c, value));
        });
    }

    /// <summary>One underlined fill carrying a primary value, with two trailing sub-labelled fills
    /// (e.g. the driver's-license line: number, then STATE and KIND).</summary>
    private static void SubLabelledFill(IContainer container, string value, string sub1, string sub1Value, string sub2, string sub2Value)
    {
        container.Column(col =>
        {
            col.Item().BorderBottom(LineThickness).MinHeight(9).PaddingBottom(1).PaddingHorizontal(1).AlignBottom().Row(fill =>
            {
                fill.RelativeItem(2.4f).Element(c => Fill(c, value));
                fill.RelativeItem(1f).Element(c => Fill(c, sub1Value));
                fill.RelativeItem(1f).Element(c => Fill(c, sub2Value));
            });
            col.Item().Row(sub =>
            {
                sub.RelativeItem(2.4f).Text(string.Empty);
                sub.RelativeItem(1f).AlignCenter().Text(sub1).FontSize(SubLabelSize);
                sub.RelativeItem(1f).AlignCenter().Text(sub2).FontSize(SubLabelSize);
            });
        });
    }

    /// <summary>A single-line, single-size data fill that truncates rather than wrapping or expanding —
    /// a long value must never grow a row and shift its neighbours.</summary>
    private static void Fill(IContainer container, string? value)
    {
        container.Text(t =>
        {
            t.ClampLines(1);
            t.Span(value ?? string.Empty).FontSize(FillSize);
        });
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

    /// <summary>A caption followed by its checkbox (master order for COMMERCIAL VEHICLE / HAZARDOUS MATERIAL).</summary>
    private static void CheckAfter(IContainer container, bool on, string caption)
    {
        container.Row(row =>
        {
            row.AutoItem().AlignBottom().PaddingRight(2).Text(caption).FontSize(CheckSize);
            row.AutoItem().AlignBottom().Element(c => Box(c, on));
        });
    }

    private static void Box(IContainer container, bool on)
    {
        var box = container.Width(7).Height(7).Border(0.6f).AlignCenter().AlignMiddle();

        // Only emit a text line when the box is ticked. An empty Text still reserves a full line
        // box, and at this size the rendering font's line height can exceed the 7pt box height —
        // which throws a layout exception under fonts with taller metrics than Arial. LineHeight(1)
        // pins the line box to the glyph size so the "X" fits.
        if (on)
            box.Text("X").FontSize(5.5f).LineHeight(1f).Bold();
    }
}
