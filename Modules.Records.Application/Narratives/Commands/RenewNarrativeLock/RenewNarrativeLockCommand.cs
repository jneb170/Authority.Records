using MediatR;

namespace Modules.Records.Application.Narratives.Commands.RenewNarrativeLock;

public sealed record RenewNarrativeLockCommand(Guid NarrativeId) : IRequest;
