using Modules.Records.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Common
{
    public sealed class TestTenantProvider : ITenantProvider
    {
        private Guid _tenantId;
        private readonly Guid _agencyId;

        public TestTenantProvider(Guid tenantId, Guid agencyId = default)
        {
            _tenantId = tenantId;
            _agencyId = agencyId;
        }

        public Guid GetAgencyId() => _agencyId;

        public Guid GetJurisdictionId() => _tenantId;

        public Guid GetUserId()
        {
            throw new NotImplementedException();
        }

        public void SetJurisdictionId(Guid jurisdictionId)
        {
            _tenantId = jurisdictionId;
        }
    }

}
