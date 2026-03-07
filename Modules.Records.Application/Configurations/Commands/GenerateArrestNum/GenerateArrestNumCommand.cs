using MediatR;

namespace Modules.Records.Application.Configurations.Commands.GenerateArrestNum;

/// <summary>
/// Atomically reserves the next ArrestNum for the current agency using the configured ArrestFormat.
/// The sequence slot is permanently consumed even if the arrest is not saved (gaps are acceptable).
/// </summary>
public sealed record GenerateArrestNumCommand : IRequest<string>;
