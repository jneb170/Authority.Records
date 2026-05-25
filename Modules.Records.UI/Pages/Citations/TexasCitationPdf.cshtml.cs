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

    public async Task<IActionResult> OnGetAsync(long recordNumber, bool download = false, CancellationToken cancellationToken = default)
    {
        var model = await _builder.BuildAsync(recordNumber, cancellationToken);
        if (model is null)
            return NotFound();

        var pdf = new CitationTexasPdfDocument(model).GeneratePdf();
        var fileName = $"{Sanitize(model.DocumentTitle)}.pdf";

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

    private static string Sanitize(string value)
    {
        var cleaned = string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(cleaned) ? "citation" : cleaned;
    }
}
