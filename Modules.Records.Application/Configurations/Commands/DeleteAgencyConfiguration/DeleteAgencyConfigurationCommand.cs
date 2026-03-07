using MediatR;

namespace Modules.Records.Application.Configurations.Commands.DeleteAgencyConfiguration;

/// <summary>Soft-deletes a configuration entry by key for the current agency.</summary>
public sealed record DeleteAgencyConfigurationCommand(string Key) : IRequest;
