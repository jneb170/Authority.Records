using System.Globalization;
using Modules.Records.UI.Services;

namespace Modules.Records.UI.Helpers;

public static class DateTimeLocalInputHelper
{
    private const string DateTimeLocalFormat = "yyyy-MM-ddTHH:mm";

    public static string Format(DateTime value) =>
        value.ToString(DateTimeLocalFormat, CultureInfo.InvariantCulture);

    /// <summary>Formats a stored UTC value for a datetime-local input, in the agency's time zone (issue #95).</summary>
    public static string FormatLocal(DateTime? utcValue, IAgencyClock clock) =>
        utcValue.HasValue
            ? clock.ToAgencyTime(utcValue.Value).ToString(DateTimeLocalFormat, CultureInfo.InvariantCulture)
            : string.Empty;

    public static bool TryParse(string? value, out DateTime parsed) =>
        DateTime.TryParseExact(
            value,
            DateTimeLocalFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsed);

    public static bool TryParseNullable(string? value, out DateTime? parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = null;
            return true;
        }

        if (TryParse(value, out var parsedValue))
        {
            parsed = parsedValue;
            return true;
        }

        parsed = null;
        return false;
    }

    /// <summary>Parses a datetime-local input entered in the agency's time zone and converts it to UTC.</summary>
    public static bool TryParseUtcFromLocal(string? value, IAgencyClock clock, out DateTime? parsedUtc)
    {
        if (!TryParseNullable(value, out var parsedLocal))
        {
            parsedUtc = null;
            return false;
        }

        parsedUtc = parsedLocal.HasValue
            ? DateTime.SpecifyKind(clock.ToUtc(parsedLocal.Value), DateTimeKind.Utc)
            : null;

        return true;
    }
}
