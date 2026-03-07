using MediatR;

namespace Modules.Records.Application.Configurations.Commands.GenerateIncidentNum;

/// <summary>
/// Atomically reserves the next IncidentNum for the current agency using the configured IncidentFormat.
/// The sequence slot is permanently consumed even if the incident is not saved (gaps are acceptable).
/// </summary>
public sealed record GenerateIncidentNumCommand : IRequest<string>;
