using MediatR;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Configurations.Commands.DeleteAgencyConfiguration;

/// <summary>Soft-deletes a configuration entry by key for the current agency.</summary>
[RequiresRole(RoleNames.Admin)]
public sealed record DeleteAgencyConfigurationCommand(string Key) : IRequest;
