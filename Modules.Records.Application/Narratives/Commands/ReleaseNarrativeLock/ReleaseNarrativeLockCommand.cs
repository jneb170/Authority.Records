using MediatR;

namespace Modules.Records.Application.Narratives.Commands.ReleaseNarrativeLock;

public sealed record ReleaseNarrativeLockCommand(Guid NarrativeId) : IRequest;
