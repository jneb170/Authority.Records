using MediatR;

namespace Modules.Records.Application.Names.Commands.ReleaseNameLock;

public sealed record ReleaseNameLockCommand(Guid NameId) : IRequest;
