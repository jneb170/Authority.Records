namespace Modules.Records.Domain.Common.Specifications;

public sealed class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _inner;

    public NotSpecification(Specification<T> inner)
    {
        _inner = inner;
    }

    public override bool IsSatisfiedBy(T entity) => !_inner.IsSatisfiedBy(entity);
    public override string ErrorCode => _inner.ErrorCode;
    public override string Reason => $"Must not: {_inner.Reason}";
}
