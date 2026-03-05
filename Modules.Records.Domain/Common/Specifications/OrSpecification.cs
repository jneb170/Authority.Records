namespace Modules.Records.Domain.Common.Specifications;

public sealed class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(T entity) =>
        _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);

    public override string ErrorCode => _left.ErrorCode;
    public override string Reason => $"{_left.Reason} or {_right.Reason}";
}
