using MediatR;
using System;

namespace Modules.Records.Application.Arrests.Commands.RenewArrestLock
{
    public sealed record RenewArrestLockCommand(Guid ArrestId) : IRequest;
}
