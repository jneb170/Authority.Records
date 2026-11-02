using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Modules.Records.UI.Pages;

/// <summary>
/// Friendly error page (issue #94). Serves two pipelines configured in Program.cs:
/// <list type="bullet">
/// <item><c>UseExceptionHandler("/Error")</c>: unhandled exceptions (shown as a 500 with a request id).</item>
/// <item><c>UseStatusCodePagesWithReExecute("/Error/{0}")</c>: empty-bodied error status codes, most
/// commonly 404 for unknown URLs, which never reach the Blazor Router.</item>
/// </list>
/// The exception handler re-executes the original request method, so POSTs must be accepted without an
/// antiforgery token, and the page must be reachable while signed out.
/// </summary>
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class ErrorModel : PageModel
{
    public int StatusCode { get; private set; }
    public string Title { get; private set; } = "Something went wrong";
    public string Message { get; private set; } = "";
    public string Icon { get; private set; } = "bi-exclamation-octagon";
    public string RequestId { get; private set; } = "";
    public bool ShowRequestId { get; private set; }

    public void OnGet(int? statusCode) => Populate(statusCode);

    public void OnPost(int? statusCode) => Populate(statusCode);

    private void Populate(int? statusCode)
    {
        var isException = HttpContext.Features.Get<IExceptionHandlerPathFeature>() is not null;

        // Only honour the route value for real error codes; /Error/200 etc. render as a generic error.
        StatusCode = isException
            ? StatusCodes.Status500InternalServerError
            : statusCode is >= 400 and <= 599 ? statusCode.Value : StatusCodes.Status500InternalServerError;

        // Re-executed status code pages keep the original status; a direct visit to /Error must not return 200.
        Response.StatusCode = StatusCode;

        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        switch (StatusCode)
        {
            case StatusCodes.Status404NotFound:
                Title = "Page not found";
                Message = "There's nothing at this address. Check the link, or head back to the dashboard.";
                Icon = "bi-signpost-split";
                break;
            case StatusCodes.Status400BadRequest:
                Title = "Bad request";
                Message = "The request couldn't be processed. Reload the page and try again.";
                Icon = "bi-exclamation-triangle";
                break;
            case StatusCodes.Status503ServiceUnavailable:
                Title = "Temporarily unavailable";
                Message = "Authority Records is briefly unavailable. Please try again in a moment.";
                Icon = "bi-hourglass-split";
                ShowRequestId = true;
                break;
            default:
                Title = "Something went wrong";
                Message = "An unexpected error occurred. Try again, and if it keeps happening, report it with the request ID below.";
                ShowRequestId = true;
                break;
        }
    }
}
