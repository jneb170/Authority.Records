using System.Globalization;

namespace Modules.Records.UI.Helpers;

public static class DateTimeLocalInputHelper
{
    private const string DateTimeLocalFormat = "yyyy-MM-ddTHH:mm";

    public static string Format(DateTime value) =>
        value.ToString(DateTimeLocalFormat, CultureInfo.InvariantCulture);

    public static string FormatLocal(DateTime? value) =>
        value.HasValue
            ? value.Value.ToLocalTime().ToString(DateTimeLocalFormat, CultureInfo.InvariantCulture)
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

    public static bool TryParseUtcFromLocal(string? value, out DateTime? parsedUtc)
    {
        if (!TryParseNullable(value, out var parsedLocal))
        {
            parsedUtc = null;
            return false;
        }

        parsedUtc = parsedLocal.HasValue
            ? DateTime.SpecifyKind(parsedLocal.Value, DateTimeKind.Local).ToUniversalTime()
            : null;

        return true;
    }
}
