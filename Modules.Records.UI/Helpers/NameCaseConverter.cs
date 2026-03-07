namespace Modules.Records.UI.Helpers;

/// <summary>
/// Converts name strings to proper name casing, handling common
/// prefixes (Mc, Mac, O', D') and hyphenated names.
/// </summary>
public static class NameCaseConverter
{
    public static string? ToNameCase(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var parts = input.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new string[parts.Length];

        for (var i = 0; i < parts.Length; i++)
            result[i] = CapitalizePart(parts[i]);

        return string.Join(" ", result);
    }

    private static string CapitalizePart(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        // Handle hyphenated parts: Smith-Jones
        if (word.Contains('-'))
        {
            var hyphenParts = word.Split('-');
            return string.Join("-", hyphenParts.Select(CapitalizePart));
        }

        // Handle O' prefix: O'Brien, O'Neill
        if (word.Length > 2 && word.StartsWith("o'"))
            return "O'" + Capitalize(word[2..]);

        // Handle D' prefix: D'Angelo
        if (word.Length > 2 && word.StartsWith("d'"))
            return "D'" + Capitalize(word[2..]);

        // Handle Mac prefix (checked before Mc):
        // MacPherson, MacDonald — but NOT Mack, Macon (too short after prefix)
        if (word.Length > 4 && word.StartsWith("mac") && IsNameSuffix(word[3..]))
            return "Mac" + Capitalize(word[3..]);

        // Handle Mc prefix: McNeil, McDonald, McBride
        if (word.Length > 3 && word.StartsWith("mc"))
            return "Mc" + Capitalize(word[2..]);

        // Default: capitalize first letter
        return Capitalize(word);
    }

    /// <summary>
    /// Returns true when the suffix after "Mac" looks like a proper name stem
    /// (at least 2 chars so "Mack" stays "Mack" and not "MacK").
    /// </summary>
    private static bool IsNameSuffix(string suffix) =>
        suffix.Length >= 2 && char.IsLetter(suffix[0]);

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}
