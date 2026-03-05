using ArrestEntity = Modules.Records.Domain.Entities.Arrest;

namespace Modules.Records.Domain.Common.Specifications.Arrest;

public sealed class SuspectNameProvidedSpecification : Specification<ArrestEntity>
{
    public override bool IsSatisfiedBy(ArrestEntity entity) =>
        !string.IsNullOrWhiteSpace(entity.SuspectName);

    public override string ErrorCode => "arrest.close.invalid";
    public override string Reason => "Suspect name required before closing.";
}
