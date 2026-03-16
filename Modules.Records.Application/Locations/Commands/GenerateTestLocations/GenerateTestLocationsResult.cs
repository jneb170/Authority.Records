namespace Modules.Records.Application.Locations.Commands.GenerateTestLocations;

public sealed record GenerateTestLocationsResult(
    int                   Created,
    int                   Failed,
    IReadOnlyList<string> Errors);
