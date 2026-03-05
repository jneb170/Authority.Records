namespace Modules.Records.Domain.Common.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    public abstract bool IsSatisfiedBy(T entity);
    public abstract string ErrorCode { get; }
    public abstract string Reason { get; }

    public Specification<T> And(Specification<T> other) =>
        new AndSpecification<T>(this, other);

    public Specification<T> Or(Specification<T> other) =>
        new OrSpecification<T>(this, other);

    public Specification<T> Not() =>
        new NotSpecification<T>(this);
}
