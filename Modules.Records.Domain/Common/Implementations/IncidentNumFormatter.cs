using System.Text.RegularExpressions;

namespace Modules.Records.Domain.Common.Implementations;

/// <summary>
/// Formats a record number using a template string with substitution tokens.
/// Tokens: yyyy, yy, mm, dd, and any run of 'x' characters (zero-padded sequence).
/// Example: Format("yyyy-mmdd-xxxxxx", 2026, 3, 7, 42) → "2026-0307-000042"
/// </summary>
public static partial class IncidentNumFormatter
{
    [GeneratedRegex(@"x+", RegexOptions.IgnoreCase)]
    private static partial Regex SeqTokenRegex();

    public static string Format(string template, int year, int month, int day, long sequence)
    {
        var result = template
            .Replace("yyyy", year.ToString("D4"))
            .Replace("yy", (year % 100).ToString("D2"))
            .Replace("mm", month.ToString("D2"))
            .Replace("dd", day.ToString("D2"));

        result = SeqTokenRegex().Replace(result, m =>
            sequence.ToString().PadLeft(m.Length, '0'));

        return result;
    }

    /// <summary>Returns a sample value using today's date and sequence 1. Useful for config previews.</summary>
    public static string Preview(string template)
    {
        var now = DateTime.UtcNow;
        return Format(template, now.Year, now.Month, now.Day, 1);
    }
}
