using Modules.Records.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Records.Domain.Entities
{
    public sealed class Statute : IMultiTenant
    {
        public Guid JurisdictionId { get; private set; }
        public Guid Id { get; private set; }
        public Statute() { }
    }
}
