using MediatR;

namespace Modules.Records.Application.Names.Commands.AcquireNameLock;

public sealed record AcquireNameLockCommand(Guid NameId) : IRequest;
