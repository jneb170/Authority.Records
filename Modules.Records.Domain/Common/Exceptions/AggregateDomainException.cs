using System.Collections.ObjectModel;

namespace Modules.Records.Domain.Common.Exceptions;

public sealed class AggregateDomainException : DomainException
{
    public IReadOnlyCollection<DomainException> Errors { get; }

    public AggregateDomainException(
        IEnumerable<DomainException> errors)
        : base(
            "domain.validation.failed",
            "One or more domain validation errors occurred.")
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors));

        var errorList = errors.ToList();

        if (!errorList.Any())
            throw new ArgumentException(
                "At least one error must be provided.",
                nameof(errors));

        Errors = new ReadOnlyCollection<DomainException>(errorList);
    }
}