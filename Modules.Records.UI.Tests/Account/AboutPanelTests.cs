using Microsoft.Extensions.Configuration;
using Modules.Records.UI.Pages.Account;

namespace Modules.Records.UI.Tests.Account;

public class AboutPanelTests
{
    private static LoginModel.AboutPanel From(params (string Key, string? Value)[] settings)
        => LoginModel.AboutPanel.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)))
            .Build());

    [Fact]
    public void Defaults_ShowPanelWithRepositoryLinkOnly()
    {
        var about = From();

        Assert.True(about.Enabled);
        Assert.Equal(LoginModel.AboutPanel.DefaultGitHubUrl, about.GitHubUrl);
        Assert.Null(about.LinkedInUrl);
        Assert.Null(about.WebsiteUrl);
        Assert.Null(about.AuthorName);
    }

    [Fact]
    public void ConfiguredValues_AreUsed()
    {
        var about = From(
            ("About:AuthorName", "  Jane Doe "),
            ("About:LinkedInUrl", "https://www.linkedin.com/in/jane"),
            ("About:WebsiteUrl", "https://jane.example"),
            ("About:GitHubUrl", "https://github.com/jane/rms"));

        Assert.Equal("Jane Doe", about.AuthorName);
        Assert.Equal("https://www.linkedin.com/in/jane", about.LinkedInUrl);
        Assert.Equal("https://jane.example/", about.WebsiteUrl);
        Assert.Equal("https://github.com/jane/rms", about.GitHubUrl);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("not a url")]
    [InlineData("/relative/path")]
    [InlineData("")]
    public void NonHttpLinks_AreDropped(string value)
    {
        var about = From(("About:LinkedInUrl", value), ("About:GitHubUrl", value));

        Assert.Null(about.LinkedInUrl);
        Assert.Null(about.GitHubUrl);
    }

    [Fact]
    public void Panel_CanBeDisabled()
        => Assert.False(From(("About:Enabled", "false")).Enabled);
}
