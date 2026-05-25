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

    private readonly CitationTexasPrintModel _model;

    public CitationTexasPdfDocument(CitationTexasPrintModel model) => _model = model;

    public DocumentMetadata GetMetadata() => new() { Title = _model.DocumentTitle };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(0.18f, Unit.Inch);
            page.DefaultTextStyle(t => t.FontSize(FillSize).FontColor(Colors.Black).FontFamily(Fonts.Arial));

            page.Content().Border(0.8f).Padding(4).Column(root =>
            {
                root.Spacing(3);

                ComposeHeader(root);
                ComposeAffidavitIntro(root);
                ComposeTimeRow(root);
                ComposeDefendant(root);
                ComposeVehicle(root);
                ComposeOffenseLocation(root);
                ComposeMidLayout(root);
                ComposeFooter(root);
            });
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

        col.Item().PaddingLeft(150).Text("COMPLAINT - AFFIDAVIT").FontSize(8.8f).Bold();
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

        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(2f).Element(c => Field(c, "CITY - STATE", _model.CityState));
            row.RelativeItem(0.55f).Element(c => Field(c, "AGE", _model.Age));
            row.RelativeItem(0.95f).Element(c => Field(c, "BIRTH DATE", _model.BirthDate));
            row.RelativeItem(0.7f).Element(c => Field(c, "RACE", _model.Race));
            row.RelativeItem(0.55f).Element(c => Field(c, "SEX", _model.Sex));
            row.RelativeItem(0.5f).Element(c => Field(c, "HT", _model.Height));
            row.RelativeItem(0.5f).Element(c => Field(c, "WT", _model.Weight));
        });

        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(2f).Element(c => Field(c, "DRIV. LIC. NO.", _model.DriversLicenseNumber));
            row.RelativeItem(0.65f).Element(c => Field(c, "STATE", _model.DriversLicenseState));
            row.RelativeItem(0.65f).Element(c => Field(c, "KIND", string.Empty));
            row.RelativeItem(1.5f).AlignBottom().Text("DID UNLAWFULLY OPERATE (PARK)").FontSize(5.4f);
        });
    }

    private void ComposeVehicle(ColumnDescriptor col)
    {
        col.Item().Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Element(c => Check(c, _model.VehicleIsCommercial, "COMMERCIAL VEHICLE"));
            row.RelativeItem().Element(c => Check(c, _model.VehicleCarriesHazmat, "HAZARDOUS MATERIAL"));
        });

        col.Item().Row(row =>
        {
            row.Spacing(5);
            row.RelativeItem(1.2f).Element(c => Field(c, "VEH. LIC. NO.", _model.PlateNumber));
            row.RelativeItem(0.45f).Element(c => Field(c, "YR.", _model.PlateYear));
            row.RelativeItem(0.65f).Element(c => Field(c, "STATE", _model.PlateState));
            row.RelativeItem(0.45f).Element(c => Field(c, "YR.", _model.ModelYear));
            row.RelativeItem(1f).Element(c => Field(c, "MAKE", _model.Make));
            row.RelativeItem(0.9f).Element(c => Field(c, "STYLE", _model.Style));
            row.RelativeItem(0.7f).Element(c => Field(c, "COLOR", _model.Color));
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
        col.Item().Row(row =>
        {
            row.Spacing(3);
            row.RelativeItem().Border(0.7f).Padding(3).Column(ComposeOffensePanel);
            row.ConstantItem(96).Element(ComposeSideStrip);
        });
    }

    private void ComposeOffensePanel(ColumnDescriptor panel)
    {
        panel.Spacing(3);

        panel.Item().Text("SPEEDING").FontSize(6.2f).Bold();

        panel.Item().Row(row =>
        {
            row.Spacing(5);
            row.AutoItem().AlignBottom().Text("(over limit)").FontSize(5.2f);
            row.RelativeItem().Element(c => Check(c, _model.SpeedRange5To10, "5-10 m.p.h."));
            row.RelativeItem().Element(c => Check(c, _model.SpeedRange11To15, "11-15 m.p.h."));
            row.RelativeItem().Element(c => Check(c, _model.SpeedRangeOver15, "over 15 m.p.h."));
            row.RelativeItem().Element(c => Field(c, "MPH", _model.SpeedMph));
            row.RelativeItem().Element(c => Field(c, "in ... MPH zone", _model.ZoneMph));
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
            col.Item().MinHeight(13).PaddingTop(1).Text(value).FontSize(5.8f);
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
            row.RelativeItem(1f).Element(c => Field(c, "THIS", _model.AffidavitSignedDate));
            row.RelativeItem(1.45f).Element(c => Field(c, "(Signature and identification of officer or other complainant)", _model.ComplainantSignature));
        });

        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(1f).Element(c => Field(c, "(Name and title)", _model.OfficerNameAndTitle));
            row.RelativeItem(0.6f).Element(c => Field(c, "(Unit No.)", _model.UnitNumber));
        });

        col.Item().Row(row =>
        {
            row.Spacing(6);
            row.RelativeItem(1.5f).Element(c => Field(c, "COURT APPEARANCE:", _model.CourtLabel));
            row.RelativeItem(0.7f).Element(c => Field(c, "DAY OF", _model.CourtAppearanceDay));
            row.RelativeItem(0.8f).Element(c => Field(c, "AT", _model.CourtAppearanceTime));
        });

        col.Item().Element(c => Field(c, "ADDRESS OF COURT", _model.CourtAddress));

        col.Item().Text("I PROMISE TO APPEAR IN SAID COURT OR BUREAU AT SAID TIME AND PLACE.").FontSize(BodySize);

        col.Item().Element(c => Field(c, "SIGNATURE", _model.DefendantSignature));

        col.Item().Text("NOTICE, UNDERSTANDING THAT FAILURE TO APPEAR CONSTITUTES A SEPARATE OFFENSE.")
            .FontSize(5.6f);
    }

    // ---- Reusable primitives ----------------------------------------------------------------

    /// <summary>A labeled field: small caption followed by an underlined fill carrying the value.</summary>
    private static void Field(IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            if (!string.IsNullOrEmpty(label))
                row.AutoItem().PaddingRight(2).AlignBottom().Text(label).FontSize(LabelSize);

            row.RelativeItem().BorderBottom(LineThickness).MinHeight(9).PaddingBottom(1).PaddingHorizontal(1)
                .AlignBottom().Text(value).FontSize(FillSize);
        });
    }

    /// <summary>A checkbox followed by its caption.</summary>
    private static void Check(IContainer container, bool on, string caption)
    {
        container.Row(row =>
        {
            row.ConstantItem(8).AlignBottom().Element(c => Box(c, on));
            row.RelativeItem().PaddingLeft(2).AlignBottom().Text(caption).FontSize(CheckSize);
        });
    }

    private static void Box(IContainer container, bool on)
    {
        container.Width(7).Height(7).Border(0.6f).AlignCenter().AlignMiddle()
            .Text(on ? "X" : string.Empty).FontSize(6).Bold();
    }
}
