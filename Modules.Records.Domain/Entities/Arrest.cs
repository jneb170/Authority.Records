using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Records.Domain.Entities
{
    public sealed class Arrest : AggregateRoot, IMultiTenant
    {
        public Guid JurisdictionId { get; private set; }
        public Guid AgencyId { get; private set; }
        public Guid IncidentId { get; private set; }
        public string SuspectName { get; private set; }
        public DateTime ArrestedAt { get; private set; }

        public Arrest(Guid agencyId, Guid incidentId, string suspectName, DateTime arrestedAt)
        {
            Id = Guid.NewGuid();
            AgencyId = agencyId;
            IncidentId = incidentId;
            SuspectName = suspectName ?? throw new ArgumentNullException(nameof(suspectName));
            ArrestedAt = arrestedAt;
        }
    }


}
