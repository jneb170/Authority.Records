namespace Modules.Records.Domain.Common.Specifications;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    string ErrorCode { get; }
    string Reason { get; }
}
