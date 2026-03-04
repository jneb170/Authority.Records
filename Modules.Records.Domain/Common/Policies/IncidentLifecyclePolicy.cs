using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Common.Policies;

public sealed class IncidentLifecyclePolicy
    : DefaultLifecyclePolicy<Incident>
{
    public IncidentLifecyclePolicy(IClosePolicy<Incident> closePolicy)
        : base(closePolicy)
    {
    }

    protected override void ValidateAdditionalRules(
        Incident aggregate,
        RecordStatus current,
        RecordStatus target,
        bool isForced)
    {
        // No extra rules for now
    }
}