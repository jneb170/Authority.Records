namespace Modules.Records.Domain.Abstractions;

public interface IMultiTenant
{
    Guid JurisdictionId { get; }
}

