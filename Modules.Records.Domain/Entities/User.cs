using Modules.Records.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Records.Domain.Entities
{
    public sealed class User : IMultiTenant
    {
        public Guid JurisdictionId { get; private set; }
        public Guid Id { get; private set; }
        public string UserName { get; private set; }
        public User(Guid jurisdictionId, Guid id, string userName)
        {
            JurisdictionId = jurisdictionId;
            Id = id;
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }
    }
}
