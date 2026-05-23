using MediatR;
using System;

namespace Modules.Records.Application.Names.Commands.RenewNameLock
{
    public sealed record RenewNameLockCommand(Guid NameId) : IRequest;
}
