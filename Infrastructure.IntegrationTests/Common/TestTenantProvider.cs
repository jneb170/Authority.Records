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

        public TestTenantProvider(Guid tenantId)
        { _tenantId = tenantId; }

        public Guid GetAgencyId()
        {
            throw new NotImplementedException();
        }

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
