using System.Text.RegularExpressions;
using FluentValidation;

namespace Modules.Records.Application.Names.Validators;

/// <summary>
/// Structural checks for a US Social Security Number, per SSA assignment rules (issue #121).
/// Blank is allowed: an unknown SSN is recorded as no value.
/// </summary>
public static partial class SocialSecurityNumberRules
{
    public const string InvalidMessage =
        "SSN must be a valid Social Security Number (XXX-XX-XXXX), or left blank if unknown.";

    /// <summary>
    /// Numbers that pass the structural rules but are publicly known never to belong to a real
    /// person: the 1938 Woolworth wallet-insert number, the 1940s Social Security Board
    /// pamphlet number, and the range SSA reserves for advertisements.
    /// </summary>
    private static readonly HashSet<string> KnownInvalid =
    [
        "078051120",
        "219099999",
        .. Enumerable.Range(4320, 10).Select(serial => $"98765{serial}")
    ];

    [GeneratedRegex(@"^(\d{3})-?(\d{2})-?(\d{4})$")]
    private static partial Regex Pattern();

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var match = Pattern().Match(value.Trim());
        if (!match.Success)
            return false;

        var area = match.Groups[1].Value;
        var group = match.Groups[2].Value;
        var serial = match.Groups[3].Value;

        // SSA never assigns area 000, 666 or 900-999, group 00, or serial 0000.
        if (area is "000" or "666" || area[0] == '9')
            return false;
        if (group == "00" || serial == "0000")
            return false;

        return !KnownInvalid.Contains(area + group + serial);
    }

    public static IRuleBuilderOptions<T, string?> ValidSocialSecurityNumber<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(IsValid).WithMessage(InvalidMessage);
}
