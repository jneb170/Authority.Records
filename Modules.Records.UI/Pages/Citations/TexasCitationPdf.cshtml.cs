using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Net.Http.Headers;
using Modules.Records.UI.Printing;
using QuestPDF.Fluent;

namespace Modules.Records.UI.Pages.Citations;

/// <summary>
/// Serves the Texas UTC citation as a server-generated PDF. The form opens this inline (so the
/// browser's PDF viewer provides the native Save-as-PDF + printer-selection dialog); appending
/// <c>?download=1</c> returns it as a file attachment instead.
/// </summary>
[Authorize(Roles = "Admin,Supervisor,Dispatcher,Officer")]
public class TexasCitationPdfModel : PageModel
{
    private readonly ICitationTexasPrintModelBuilder _builder;

    public TexasCitationPdfModel(ICitationTexasPrintModelBuilder builder) => _builder = builder;

    public async Task<IActionResult> OnGetAsync(long recordNumber, bool download = false, string? size = null, CancellationToken cancellationToken = default)
    {
        var model = await _builder.BuildAsync(recordNumber, cancellationToken);
        if (model is null)
            return NotFound();

        var printSize = ParseSize(size);
        var pdf = new CitationTexasPdfDocument(model, printSize).GeneratePdf();
        var suffix = printSize == CitationPrintSize.FourInch ? "-4in" : string.Empty;
        var fileName = $"{Sanitize(model.DocumentTitle)}{suffix}.pdf";

        // Inline by default so the browser PDF viewer (with its native print/save controls) renders
        // it; attachment only when explicitly downloading.
        var disposition = new ContentDispositionHeaderValue(download ? "attachment" : "inline")
        {
            FileNameStar = fileName,
            FileName = fileName,
        };
        Response.Headers[HeaderNames.ContentDisposition] = disposition.ToString();

        return File(pdf, "application/pdf");
    }

    // Anything that isn't an explicit 4-inch request falls back to the Letter (left-anchored) default.
    private static CitationPrintSize ParseSize(string? size) => size?.Trim().ToLowerInvariant() switch
    {
        "4" or "4in" or "four" or "fourinch" or "thermal" => CitationPrintSize.FourInch,
        _ => CitationPrintSize.Letter,
    };

    private static string Sanitize(string value)
    {
        var cleaned = string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(cleaned) ? "citation" : cleaned;
    }
}
