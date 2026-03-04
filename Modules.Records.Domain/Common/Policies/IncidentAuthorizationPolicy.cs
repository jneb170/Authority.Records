using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class IncidentAuthorizationPolicy
    : DefaultAuthorizationPolicy<Incident>
{
    public override void EnsureCanModify(
        Incident aggregate,
        IModificationContext context)
    {
        base.EnsureCanModify(aggregate, context);

        // Example: could enforce jurisdiction-specific rules here
    }
}