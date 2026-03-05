namespace Modules.Records.Domain.DomainInvariants;

public sealed record DomainInvariantViolation(string ErrorCode, string Reason);
