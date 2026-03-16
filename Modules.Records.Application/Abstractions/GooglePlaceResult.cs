namespace Modules.Records.Application.Abstractions;

public sealed record GooglePlaceResult(
    string  PlaceName,
    string  FormattedAddress,
    string? StreetNumber,
    string? StreetAddress,
    string? AptSuite,
    string? City,
    string? Zip,
    string? StateAbbreviation,
    string? CountryCode,
    double? Lat,
    double? Lng);
