namespace Modules.Records.UI.Helpers;

/// <summary>
/// Produces realistic random values for fake Name record generation.
/// All methods are thread-safe for single-threaded sequential Blazor use.
/// </summary>
public static class FakeNameGenerator
{
    private static readonly Random _rng = new();

    // ── Sex ─────────────────────────────────────────────────────────────────────

    /// <summary>Returns "M", "F", or "U" using weighted distribution (49/49/2%).</summary>
    public static string GenerateSex()
    {
        var roll = _rng.Next(100);
        return roll < 49 ? "M" : roll < 98 ? "F" : "U";
    }

    // ── Race ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a race Value ("W","H","B","A","I","P","M","U") weighted by
    /// approximate 2020 US Census / ACS percentages.
    /// </summary>
    public static string GenerateRaceValue()
    {
        var roll = _rng.Next(1000);
        return roll switch
        {
            < 590 => "W",   // White ~59%
            < 780 => "H",   // Hispanic ~19%
            < 900 => "B",   // Black ~12%
            < 960 => "A",   // Asian ~6%
            < 970 => "I",   // American Indian / Alaska Native ~1%
            < 975 => "P",   // Pacific Islander ~0.5%
            < 990 => "M",   // Multiracial ~1.5%
            _     => "U",   // Unknown ~1% (normalised to ~4% of assigned)
        };
    }

    // ── Date of Birth ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a DOB using weighted age-bucket distribution:
    ///   0–3 = 2%, 4–15 = 10%, 16–55 = 60%, 56–84 = 25%, 85–100 = 3%
    /// </summary>
    public static DateTime GenerateDob()
    {
        int minAge, maxAge;
        var bucket = _rng.Next(100);

        if      (bucket < 2)  { minAge = 0;  maxAge = 3;   }
        else if (bucket < 12) { minAge = 4;  maxAge = 15;  }
        else if (bucket < 72) { minAge = 16; maxAge = 55;  }
        else if (bucket < 97) { minAge = 56; maxAge = 84;  }
        else                  { minAge = 85; maxAge = 100; }

        var ageDays = _rng.Next(minAge * 365, maxAge * 365 + 364);
        return DateTime.Today.AddDays(-ageDays);
    }

    /// <summary>Returns age in whole years for a given DOB.</summary>
    public static int GetAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age   = today.Year - dob.Year;
        if (dob.AddYears(age) > today) age--;
        return age;
    }

    // ── Driver's License ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a DL number in one of the four most common US formats:
    ///   A1234567 | A12345678 | AB123456 | 123456789
    /// </summary>
    public static string GenerateDlNumber()
    {
        return _rng.Next(4) switch
        {
            0 => $"{RandLetter()}{RandDigits(7)}",
            1 => $"{RandLetter()}{RandDigits(8)}",
            2 => $"{RandLetter()}{RandLetter()}{RandDigits(6)}",
            _ => RandDigits(9),
        };
    }

    // ── Height (inches) ─────────────────────────────────────────────────────────

    /// <summary>Returns height in inches weighted by sex (M ≈ 69 in, F ≈ 64 in).</summary>
    public static int GenerateHeight(string sex)
    {
        return sex switch
        {
            "M" => Clamp(NormalApprox(69, 3), 60, 84),
            "F" => Clamp(NormalApprox(64, 3), 56, 78),
            _   => Clamp(NormalApprox(66, 4), 56, 84),
        };
    }

    // ── Weight (lbs) ────────────────────────────────────────────────────────────

    /// <summary>Returns weight in lbs weighted by sex (M ≈ 190 lbs, F ≈ 165 lbs).</summary>
    public static int GenerateWeight(string sex)
    {
        return sex switch
        {
            "M" => Clamp(NormalApprox(190, 30), 120, 320),
            "F" => Clamp(NormalApprox(165, 28), 100, 270),
            _   => Clamp(NormalApprox(178, 32), 100, 320),
        };
    }

    // ── Name helpers ────────────────────────────────────────────────────────────

    public static string PickFirstName(string sex)
    {
        return sex == "M"
            ? FakeNameData.MaleFirstNames[_rng.Next(FakeNameData.MaleFirstNames.Length)]
            : sex == "F"
                ? FakeNameData.FemaleFirstNames[_rng.Next(FakeNameData.FemaleFirstNames.Length)]
                : (_rng.Next(2) == 0
                    ? FakeNameData.MaleFirstNames[_rng.Next(FakeNameData.MaleFirstNames.Length)]
                    : FakeNameData.FemaleFirstNames[_rng.Next(FakeNameData.FemaleFirstNames.Length)]);
    }

    public static string PickLastName()
        => FakeNameData.LastNames[_rng.Next(FakeNameData.LastNames.Length)];

    /// <summary>
    /// Returns a middle name (~70% populated). Of those, ~20% are single-letter initials.
    /// Returns null when not populated.
    /// </summary>
    public static string? PickMiddleName()
    {
        if (_rng.Next(10) >= 7) return null; // 30% chance no middle name
        if (_rng.Next(5) == 0)               // 20% of populated = initial only
            return ((char)('A' + _rng.Next(26))).ToString();
        return FakeNameData.MiddleNames[_rng.Next(FakeNameData.MiddleNames.Length)];
    }

    /// <summary>Picks a random item from a list, or null if the list is empty.</summary>
    public static T? PickRandom<T>(IReadOnlyList<T> items) where T : struct
    {
        if (items.Count == 0) return null;
        return items[_rng.Next(items.Count)];
    }

    public static T PickRandomRequired<T>(IReadOnlyList<T> items)
        => items[_rng.Next(items.Count)];

    // ── Private helpers ─────────────────────────────────────────────────────────

    private static char   RandLetter()          => (char)('A' + _rng.Next(26));
    private static string RandDigits(int count) => string.Concat(Enumerable.Range(0, count).Select(_ => _rng.Next(10).ToString()));
    private static int    Clamp(int v, int min, int max) => v < min ? min : v > max ? max : v;

    /// <summary>
    /// Approximate normal distribution using the Irwin-Hall method (sum of 6 uniforms).
    /// Returns an integer near <paramref name="mean"/> with spread ≈ <paramref name="stdDev"/>.
    /// </summary>
    private static int NormalApprox(double mean, double stdDev)
    {
        double sum = 0;
        for (int i = 0; i < 6; i++) sum += _rng.NextDouble();
        // sum ~ N(3, 0.5); scale to desired distribution
        var z = (sum - 3.0) / 0.5;
        return (int)Math.Round(mean + z * stdDev);
    }
}
