using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class LocationAuthorizationPolicy : DefaultAuthorizationPolicy<Location>
{
    public override void EnsureCanModify(Location aggregate, IModificationContext context)
    {
        base.EnsureCanModify(aggregate, context);
    }
}
