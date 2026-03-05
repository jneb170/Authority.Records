namespace Modules.Records.Domain.DomainInvariants;

public sealed record DomainInvariantResult(IReadOnlyList<DomainInvariantViolation> Violations)
{
    public bool IsValid => Violations.Count == 0;

    public static DomainInvariantResult Valid() =>
        new(Array.Empty<DomainInvariantViolation>());

    public static DomainInvariantResult Fail(string errorCode, string reason) =>
        new([new DomainInvariantViolation(errorCode, reason)]);
}
