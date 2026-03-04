using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class ChildCitationClosePolicy : IClosePolicy<Incident>
{
    public void ValidateCanClose(Incident incident, bool isForced)
    {
        if (isForced)
            return;

        if (incident.Citations.Any(c => !c.IsIssued))
        {
            throw new DomainException(
                "incident.close.citations.not_issued",
                "All citations must be issued before closing the incident.");
        }
    }
}
