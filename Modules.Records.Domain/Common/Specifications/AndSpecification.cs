namespace Modules.Records.Domain.Common.Specifications;

public sealed class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(T entity) =>
        _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);

    public override string ErrorCode => _left.IsSatisfiedBy(default!) ? _right.ErrorCode : _left.ErrorCode;
    public override string Reason => _left.IsSatisfiedBy(default!) ? _right.Reason : _left.Reason;
}
