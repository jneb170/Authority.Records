using MediatR;

namespace Modules.Records.Application.Citations.Commands.ReleaseCitationLock;

public sealed record ReleaseCitationLockCommand(Guid CitationId) : IRequest;
