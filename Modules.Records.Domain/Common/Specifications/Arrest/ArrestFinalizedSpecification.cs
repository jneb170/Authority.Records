using ArrestEntity = Modules.Records.Domain.Entities.Arrest;

namespace Modules.Records.Domain.Common.Specifications.Arrest;

public sealed class ArrestFinalizedSpecification : Specification<ArrestEntity>
{
    public override bool IsSatisfiedBy(ArrestEntity entity) =>
        entity.IsFinalized;

    public override string ErrorCode => "arrest.close.invalid";
    public override string Reason => "All arrests must be finalized before closing.";
}
