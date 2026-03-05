using MediatR;

namespace Modules.Records.Application.Citations.Commands.AcquireCitationLock;

public sealed record AcquireCitationLockCommand(Guid CitationId) : IRequest;
