using MediatR;

namespace Modules.Records.Application.Narratives.Commands.AcquireNarrativeLock;

public sealed record AcquireNarrativeLockCommand(Guid NarrativeId) : IRequest;
