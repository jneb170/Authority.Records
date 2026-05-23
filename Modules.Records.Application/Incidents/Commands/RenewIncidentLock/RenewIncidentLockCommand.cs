using MediatR;
using System;

namespace Modules.Records.Application.Incidents.Commands.RenewIncidentLock
{
    public sealed record RenewIncidentLockCommand(Guid IncidentId) : IRequest;
}
