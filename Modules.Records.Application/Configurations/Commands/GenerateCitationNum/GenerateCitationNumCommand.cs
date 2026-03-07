using MediatR;

namespace Modules.Records.Application.Configurations.Commands.GenerateCitationNum;

/// <summary>
/// Atomically reserves the next CitationNum for the current agency using the configured CitationFormat.
/// The sequence slot is permanently consumed even if the citation is not saved (gaps are acceptable).
/// </summary>
public sealed record GenerateCitationNumCommand : IRequest<string>;
