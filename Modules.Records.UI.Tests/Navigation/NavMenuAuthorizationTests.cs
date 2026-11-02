using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Modules.Records.UI.Tests.Navigation;

/// <summary>
/// Guards against sidebar links that lead straight to "access denied" (issue #97): every role that can see a
/// <c>NavLink</c> (through its enclosing <c>AuthorizeView Roles="…"</c> blocks) must be allowed by the
/// target page's <see cref="AuthorizeAttribute"/> roles.
/// </summary>
public sealed partial class NavMenuAuthorizationTests
{
    [Fact]
    public void EveryNavLink_IsReachable_ByEveryRoleThatCanSeeIt()
    {
        var links = ParseNavLinks(File.ReadAllText(FindNavMenuPath()));
        Assert.NotEmpty(links);

        var routes = DiscoverPageRoles();
        var problems = new List<string>();

        foreach (var (href, visibleTo) in links)
        {
            var route = "/" + href.TrimStart('/');
            if (!routes.TryGetValue(route, out var pageRoles))
            {
                problems.Add($"NavLink '{href}' has no matching @page route.");
                continue;
            }

            // Null = page has no role restriction (any authenticated user).
            if (pageRoles is null || visibleTo is null)
            {
                if (pageRoles is not null && visibleTo is null)
                    problems.Add($"NavLink '{href}' is visible to everyone but the page requires {string.Join(",", pageRoles)}.");
                continue;
            }

            var denied = visibleTo.Except(pageRoles, StringComparer.OrdinalIgnoreCase).ToList();
            if (denied.Count > 0)
                problems.Add($"NavLink '{href}' is shown to {string.Join(",", denied)} but the page only allows {string.Join(",", pageRoles)}.");
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    /// <summary>
    /// Returns each NavLink href with the roles allowed to see it (intersection of nested AuthorizeViews;
    /// null when not inside any role-restricted AuthorizeView).
    /// </summary>
    private static List<(string Href, HashSet<string>? Roles)> ParseNavLinks(string razor)
    {
        var result = new List<(string, HashSet<string>?)>();
        var stack = new Stack<HashSet<string>?>();

        foreach (Match m in TokenRegex().Matches(razor))
        {
            if (m.Groups["close"].Success)
            {
                if (stack.Count > 0) stack.Pop();
                continue;
            }

            if (m.Groups["open"].Success)
            {
                var roles = m.Groups["roles"].Success
                    ? m.Groups["roles"].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase)
                    : null;
                var parent = stack.Count > 0 ? stack.Peek() : null;
                stack.Push(Intersect(parent, roles));
                continue;
            }

            if (m.Groups["href"].Success)
                result.Add((m.Groups["href"].Value, stack.Count > 0 ? stack.Peek() : null));
        }

        return result;
    }

    private static HashSet<string>? Intersect(HashSet<string>? a, HashSet<string>? b)
    {
        if (a is null) return b;
        if (b is null) return a;
        return a.Intersect(b, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string[]?> DiscoverPageRoles()
    {
        var map = new Dictionary<string, string[]?>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in typeof(Modules.Records.UI.Shared.NavMenu).Assembly.GetTypes())
        {
            var routes = type.GetCustomAttributes<RouteAttribute>().ToList();
            if (routes.Count == 0) continue;

            var roles = type.GetCustomAttributes<AuthorizeAttribute>(inherit: true)
                .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
                .Select(a => a.Roles!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .ToList();

            // Multiple [Authorize(Roles=…)] attributes must all be satisfied: intersect them.
            string[]? effective = roles.Count == 0
                ? null
                : roles.Aggregate((x, y) => x.Intersect(y, StringComparer.OrdinalIgnoreCase).ToArray());

            foreach (var route in routes)
                map[route.Template.TrimEnd('/') is "" ? "/" : route.Template.TrimEnd('/')] = effective;
        }

        return map;
    }

    private static string FindNavMenuPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Authority.Records.sln")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "Modules.Records.UI", "Shared", "NavMenu.razor");
    }

    [GeneratedRegex("""(?<close></AuthorizeView>)|(?<open><AuthorizeView\b(?:[^>]*?\bRoles="(?<roles>[^"]*)")?[^>]*>)|<NavLink\b[^>]*?\bhref="(?<href>[^"]*)""", RegexOptions.Singleline)]
    private static partial Regex TokenRegex();
}
