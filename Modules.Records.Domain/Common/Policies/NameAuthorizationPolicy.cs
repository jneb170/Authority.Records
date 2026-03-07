using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class NameAuthorizationPolicy : DefaultAuthorizationPolicy<Name>
{
    public override void EnsureCanModify(Name aggregate, IModificationContext context)
    {
        base.EnsureCanModify(aggregate, context);
    }
}
