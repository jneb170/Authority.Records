using MediatR;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Locations.Commands.GenerateTestLocations;

[DemoOnly]
[RequiresRole(RoleNames.Admin, RoleNames.Supervisor)]
public sealed record GenerateTestLocationsCommand(
    string Keyword,
    int    Count,
    string ApiKey) : IRequest<GenerateTestLocationsResult>;
