using MediatR;

namespace Modules.Records.Application.Locations.Commands.GenerateTestLocations;

public sealed record GenerateTestLocationsCommand(
    string Keyword,
    int    Count,
    string ApiKey) : IRequest<GenerateTestLocationsResult>;
