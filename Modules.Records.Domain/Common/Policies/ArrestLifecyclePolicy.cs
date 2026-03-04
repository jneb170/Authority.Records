using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class ArrestLifecyclePolicy
    : DefaultLifecyclePolicy<Arrest>
{
    public ArrestLifecyclePolicy(IClosePolicy<Arrest> closePolicy)
            : base(closePolicy)
    {

    }

    // Optional: override ValidateAdditionalRules if needed
    protected override void ValidateAdditionalRules(Arrest aggregate, RecordStatus current, RecordStatus target, bool isForced)
    {
        // For example, you could enforce locking rules or extra jurisdiction rules here
    }
}