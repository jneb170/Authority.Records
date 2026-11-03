using MediatR;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Configurations.Commands.SetAgencyConfiguration;

/// <summary>Creates or updates a configuration entry for the current agency.</summary>
[RequiresRole(RoleNames.Admin)]
public sealed record SetAgencyConfigurationCommand(string Key, string Value) : IRequest<Guid>;
