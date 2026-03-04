using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class ChildArrestClosePolicy : IClosePolicy<Incident>
{
    public void ValidateCanClose(Incident incident, bool isForced)
    {
        if (isForced)
            return;

        if (incident.Arrests.Any(a => !a.IsFinalized))
        {
            throw new DomainException(
                "incident.close.arrests.not_finalized",
                "All arrests must be finalized before closing the incident.");
        }
    }
}
