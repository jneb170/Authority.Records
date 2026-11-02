namespace Modules.Records.UI.Helpers;

/// <summary>Masks an SSN for display as ***-**-1234 (issue #121).</summary>
public static class SsnMask
{
    public static string Mask(string? ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
            return "—";

        var digits = new string(ssn.Where(char.IsDigit).ToArray());
        return digits.Length < 4 ? "***-**-??" : $"***-**-{digits[^4..]}";
    }
}
