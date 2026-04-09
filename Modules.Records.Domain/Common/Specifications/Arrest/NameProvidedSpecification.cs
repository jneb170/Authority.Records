using ArrestEntity = Modules.Records.Domain.Entities.Arrest;

namespace Modules.Records.Domain.Common.Specifications.Arrest;

public sealed class NameProvidedSpecification : Specification<ArrestEntity>
{
    public override bool IsSatisfiedBy(ArrestEntity entity) =>
        entity.NameId.HasValue;

    public override string ErrorCode => "arrest.close.invalid";
    public override string Reason => "Linked name required before closing.";
}
