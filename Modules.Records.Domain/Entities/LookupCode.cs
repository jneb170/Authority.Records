using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Domain.Entities
{
    public sealed class LookupCode : IMultiTenant
    {
        public Guid JurisdictionId { get; private set; }
        public Guid Id { get; private set; }
        public LookupCode() { }
    }
}
