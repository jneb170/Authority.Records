using CitationEntity = Modules.Records.Domain.Entities.Citation;

namespace Modules.Records.Domain.Common.Specifications.Citation;

public sealed class CitationIssuedSpecification : Specification<CitationEntity>
{
    public override bool IsSatisfiedBy(CitationEntity entity) =>
        entity.IsIssued;

    public override string ErrorCode => "citation.close.invalid";
    public override string Reason => "Citation must be issued before closing.";
}
