using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.Entities
{
    public sealed class Citation : AggregateRoot, IMultiTenant
    {
        public Guid JurisdictionId { get; private set; }
        public Guid AgencyId { get; private set; }
        public string Description { get; private set; }

        // --- Locking ---
        public bool IsLocked => LockedByUserId.HasValue && LockedAtUtc.HasValue;
        public Guid? LockedByUserId { get; private set; }
        public DateTime? LockedAtUtc { get; private set; }

        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        private Citation() { } // EF

        public Citation(Guid jurisdictionId, Guid agencyId, string description)
        {
            Id = Guid.NewGuid();
            JurisdictionId = jurisdictionId;
            AgencyId = agencyId;
            Description = description;
        }

        // --- Locking Methods ---
        public void AcquireLock(Guid userId, TimeSpan lockTimeout)
        {
            if (LockedByUserId.HasValue &&
                LockedByUserId != userId &&
                LockedAtUtc.HasValue &&
                LockedAtUtc.Value.Add(lockTimeout) > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Record is currently locked by another user.");
            }

            LockedByUserId = userId;
            LockedAtUtc = DateTime.UtcNow;

            // Raise domain event
            AddDomainEvent(new IncidentLockAcquiredDomainEvent(Id, userId, LockedAtUtc.Value));
        }

        public void ReleaseLock(Guid userId)
        {
            if (LockedByUserId != userId)
                throw new InvalidOperationException("Only the locking user can release the lock.");

            LockedByUserId = null;
            LockedAtUtc = null;
        }
    }
}
