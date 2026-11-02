using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Records.UI.Pages;

namespace Modules.Records.UI.Tests.ErrorHandling;

/// <summary>
/// Issue #94: unknown URLs rendered a blank page, and UseExceptionHandler("/Error") pointed at a page that
/// didn't exist (so exceptions turned into a raw 500). This wires the same middleware as Program.cs around
/// the real Pages/Error.cshtml.
/// </summary>
public sealed class ErrorPageTests : IAsyncLifetime
{
    private WebApplication _app = default!;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddRazorPages().AddApplicationPart(typeof(ErrorModel).Assembly);
        builder.Services.AddAuthorization();

        _app = builder.Build();
        _app.UseExceptionHandler("/Error");
        _app.UseStatusCodePagesWithReExecute("/Error/{0}");
        _app.UseRouting();
        _app.UseAuthorization();
        _app.MapGet("/boom", (HttpContext _) => { throw new InvalidOperationException("kaboom"); });
        _app.MapPost("/boom", (HttpContext _) => { throw new InvalidOperationException("kaboom"); });
        _app.MapRazorPages();

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task UnknownUrl_Returns404_WithFriendlyPage()
    {
        var response = await _client.GetAsync("/does-not-exist");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("<title>Page not found — Authority Records</title>", html);
        Assert.Contains("Back to the dashboard", html);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public async Task UnhandledException_Returns500_WithFriendlyPageAndRequestId(string method)
    {
        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "/boom"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("Something went wrong", html);
        Assert.Contains("Request ID:", html);
        Assert.DoesNotContain("kaboom", html);
    }

    [Fact]
    public async Task DirectVisitToErrorPage_DoesNotReturn200()
    {
        var response = await _client.GetAsync("/Error");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
