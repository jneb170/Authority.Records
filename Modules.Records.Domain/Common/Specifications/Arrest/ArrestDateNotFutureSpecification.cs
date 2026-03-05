using ArrestEntity = Modules.Records.Domain.Entities.Arrest;

namespace Modules.Records.Domain.Common.Specifications.Arrest;

public sealed class ArrestDateNotFutureSpecification : Specification<ArrestEntity>
{
    public override bool IsSatisfiedBy(ArrestEntity entity) =>
        entity.ArrestedAt <= DateTime.UtcNow;

    public override string ErrorCode => "arrest.date.invalid";
    public override string Reason => "Arrest date cannot be in the future.";
}
