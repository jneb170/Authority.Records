using MediatR;
using System;

namespace Modules.Records.Application.Locations.Commands.RenewLocationLock
{
    public sealed record RenewLocationLockCommand(Guid LocationId) : IRequest;
}
