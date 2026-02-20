using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Records.Application.Incidents.Commands.AcquireIncidentLock
{
    public sealed record AcquireIncidentLockCommand(Guid IncidentId) : IRequest;
}
