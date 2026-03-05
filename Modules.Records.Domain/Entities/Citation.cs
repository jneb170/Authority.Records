using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities
{
    public sealed class Citation : AggregateRoot, IMultiTenant
    {
        public Guid JurisdictionId { get; private set; }
        public Guid AgencyId { get; private set; }
        public string Description { get; private set; }
        public bool IsFinalized { get; private set; }

        public DateTime IssueDate { get; private set; }

        private static ILifecyclePolicy<Citation> _lifecyclePolicy;

        // Inject lifecycle policy from composition root / factory
        public static void SetLifecyclePolicy(ILifecyclePolicy<Citation> policy)
        {
            _lifecyclePolicy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        // --- Locking ---
        public bool IsLocked => LockedByUserId.HasValue && LockedAtUtc.HasValue;
        public Guid? LockedByUserId { get; private set; }
        public DateTime? LockedAtUtc { get; private set; }

        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        public bool IsIssued { get; private set; }

        private Citation() { } // EF

        public Citation(Guid jurisdictionId, Guid agencyId, string description)
        {
            Id = Guid.NewGuid();
            JurisdictionId = jurisdictionId;
            AgencyId = agencyId;
            Description = description;

            AddDomainEvent(new CitationCreatedDomainEvent(Id, JurisdictionId, AgencyId, Description, IssueDate));
        }

        public void Issue() => IsIssued = true;

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
            AddDomainEvent(new IncidentLockAcquiredDomainEvent(Id, userId));
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
