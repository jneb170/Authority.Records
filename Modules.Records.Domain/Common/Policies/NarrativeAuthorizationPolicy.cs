using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class NarrativeAuthorizationPolicy : DefaultAuthorizationPolicy<Narrative>
{
    public override void EnsureCanModify(Narrative aggregate, IModificationContext context)
    {
        base.EnsureCanModify(aggregate, context);
    }
}
