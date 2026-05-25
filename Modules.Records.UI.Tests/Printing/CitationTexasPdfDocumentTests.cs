using Modules.Records.Domain.Common.Violations;
using Modules.Records.UI.Printing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Modules.Records.UI.Tests.Printing;

public class CitationTexasPdfDocumentTests
{
    static CitationTexasPdfDocumentTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.EnableDebugging = true;
    }

    private static CitationTexasPrintModel SampleModel() => new()
    {
        RecordNumber = 10000,
        DocumentTitle = "Citation CT-2026-000001",
        CaseNo = "CT-2026-000001",
        DocketNo = "DK-99",
        PageNo = "8-8",
        CourtLabel = "Municipal",
        AppearanceOrCitationLocation = "Plano Municipal Court, 1400 Ave K, Plano",
        IssueDate = "05/24/2026",
        IssueDayOfMonth = "24",
        IssueMonthYear = "May 2026",
        IssueTime = "2:30",
        IssueAmPm = "PM",
        LastName = "Perez",
        FirstName = "Mary",
        Initial = "L",
        FullName = "Perez, Mary",
        AddressStreet = "5445 Legacy Dr #100",
        CityState = "Plano, TX 75024",
        Age = "34",
        BirthDate = "03/02/1992",
        Race = "White",
        Sex = "Female",
        Height = "65",
        Weight = "140",
        SocialSecurityNumber = "xxx-xx-1234",
        DriversLicenseNumber = "12345678",
        DriversLicenseState = "TX",
        VehicleIsCommercial = false,
        VehicleCarriesHazmat = false,
        PlateNumber = "ABC1234",
        PlateYear = "2026",
        PlateState = "TX",
        ModelYear = "2021",
        Make = "Toyota",
        Style = "4DR",
        Color = "Blue",
        OccurredAt = "Legacy Dr near Preston Rd",
        SpeedRangeOver15 = true,
        SpeedMph = "47",
        ZoneMph = "30",
        OtherViolations = "Failure to control speed",
        SourceStateStatute = true,
        ViolationSection = "545.351",
        AcceptedBondNotes = "$250 cash",
        ReceiptNumber = "R-5567",
        AffidavitSignedDate = "05/24/2026",
        ComplainantSignature = "Ofc. J. Doe",
        OfficerNameAndTitle = "Police Officer",
        UnitNumber = "12",
        CourtAppearanceDay = "06/20/2026",
        CourtAppearanceTime = "9:00 AM",
        CourtAddress = "1400 Ave K, Plano, TX",
        DefendantSignature = "",
        AreaResidential = true,
        Highway4LaneDivided = true,
        VisibleFlags = new HashSet<ViolationFlagKey>
        {
            ViolationFlagKey.ImproperLeftTurn,
            ViolationFlagKey.NoSignal,
            ViolationFlagKey.CollisionRearEnd,
            ViolationFlagKey.Ice,
        },
    };

    [Theory]
    [InlineData(CitationPrintSize.Letter)]
    [InlineData(CitationPrintSize.FourInch)]
    public void GeneratePdf_ProducesValidPdfBytes(CitationPrintSize size)
    {
        var pdf = new CitationTexasPdfDocument(SampleModel(), size).GeneratePdf();

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000, $"PDF unexpectedly small ({pdf.Length} bytes).");

        // Valid PDFs start with the "%PDF-" magic header.
        var header = System.Text.Encoding.ASCII.GetString(pdf, 0, 5);
        Assert.Equal("%PDF-", header);
    }

    [Theory]
    [InlineData(CitationPrintSize.Letter)]
    [InlineData(CitationPrintSize.FourInch)]
    public void GeneratePdf_HandlesLongValues_WithoutThrowing(CitationPrintSize size)
    {
        // Guards against layout overflow from oversized field content (long notes, pasted text,
        // long unbreakable tokens). The font-metric overflow that broke prod is covered by the
        // sample/empty tests rendering under the bundled Lato font; this adds a long-content guard.
        var longToken = new string('A', 80);
        var longProse = string.Join(" ", System.Linq.Enumerable.Repeat("failure to maintain a single marked lane and control of speed", 60));
        var model = SampleModel() with
        {
            AcceptedBondNotes = longToken,
            SocialSecurityNumber = longToken,
            OtherViolations = longProse,
            OccurredAt = longProse,
            CourtAddress = longToken,
            AddressStreet = longToken,
            DriversLicenseNumber = longToken,
            FullName = longToken,
            CityState = longToken,
            ComplainantSignature = longToken,
            OfficerNameAndTitle = longToken,
        };

        var pdf = new CitationTexasPdfDocument(model, size).GeneratePdf();

        Assert.True(pdf.Length > 1000);
    }

    [Theory]
    [InlineData(CitationPrintSize.Letter)]
    [InlineData(CitationPrintSize.FourInch)]
    public void GeneratePdf_HandlesEmptyModel_WithoutThrowing(CitationPrintSize size)
    {
        var empty = new CitationTexasPrintModel { RecordNumber = 1, DocumentTitle = "Citation 1" };
        var pdf = new CitationTexasPdfDocument(empty, size).GeneratePdf();

        Assert.True(pdf.Length > 1000);
    }

    [Fact]
    public void GeneratePdf_WritesSamplesToTemp_ForVisualInspection()
    {
        // Emits a real PDF per size to the temp dir so the layout can be eyeballed during development.
        // Not an assertion of pixel layout — just that the files land on disk. Overlay the 4-inch
        // output against the left block of the Letter output to confirm spacing is identical.
        foreach (var (size, name) in new[]
        {
            (CitationPrintSize.Letter, "citation-texas-letter.pdf"),
            (CitationPrintSize.FourInch, "citation-texas-4in.pdf"),
        })
        {
            var pdf = new CitationTexasPdfDocument(SampleModel(), size).GeneratePdf();
            var path = Path.Combine(Path.GetTempPath(), name);
            File.WriteAllBytes(path, pdf);

            Assert.True(File.Exists(path));
        }
    }
}
