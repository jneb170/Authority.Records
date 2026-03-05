using CitationEntity = Modules.Records.Domain.Entities.Citation;

namespace Modules.Records.Domain.Common.Specifications.Citation;

public sealed class IssueDateNotFutureSpecification : Specification<CitationEntity>
{
    public override bool IsSatisfiedBy(CitationEntity entity) =>
        entity.IssueDate <= DateTime.UtcNow;

    public override string ErrorCode => "citation.date.invalid";
    public override string Reason => "Citation Issue Date cannot be in the future.";
}
