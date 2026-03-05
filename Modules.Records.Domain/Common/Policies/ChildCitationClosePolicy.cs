using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Specifications.Incident;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class ChildCitationClosePolicy : IClosePolicy<Incident>
{
    public void ValidateCanClose(Incident incident, bool isForced)
    {
        if (isForced) return;

        var spec = new AllCitationsIssuedSpecification();
        if (!spec.IsSatisfiedBy(incident))
            throw new DomainException(spec.ErrorCode, spec.Reason);
    }
}

