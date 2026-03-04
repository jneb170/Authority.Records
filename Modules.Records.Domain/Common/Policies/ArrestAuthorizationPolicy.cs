using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class ArrestAuthorizationPolicy
    : DefaultAuthorizationPolicy<Arrest>
{
    public override void EnsureCanModify(
        Arrest aggregate,
        IModificationContext context)
    {
        base.EnsureCanModify(aggregate, context);

        // Example: could enforce jurisdiction-specific rules here
    }
}