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

        /// <summary>DB-generated auto-increment number. Use this in URLs and display; the GUID is for internal identity.</summary>
        public long RecordNumber { get; private set; }

        /// <summary>Agency-formatted citation number, e.g. "CT-2026-000001". Auto-generated on create.</summary>
        public string CitationNum { get; private set; } = string.Empty;

        /// <summary>Optional reference to the agency-configured Court picklist item.</summary>
        public Guid? CourtId { get; private set; }

        /// <summary>Optional reference to a Master Location Index record for the citation location.</summary>
        public Guid? LocationId { get; private set; }

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

        public Citation(Guid jurisdictionId, Guid agencyId, string description, DateTime issueDate, string citationNum)
        {
            Id = Guid.NewGuid();
            JurisdictionId = jurisdictionId;
            AgencyId = agencyId;
            Description = description;
            IssueDate = issueDate;
            CitationNum = citationNum;

            AddDomainEvent(new CitationCreatedDomainEvent(Id, JurisdictionId, Description, IssueDate, CitationNum));
        }

        public void Issue() => IsIssued = true;

        public void UpdateDetails(string description, DateTime issueDate, Guid? courtId, string citationNum, IModificationContext context)
        {
            Description = description;
            IssueDate   = issueDate;
            CourtId     = courtId;
            CitationNum = citationNum;

            AddDomainEvent(new CitationDetailsUpdatedDomainEvent(Id, Description, IssueDate, CourtId, CitationNum));
        }

        /// <summary>Sets or clears the linked Master Location Index record for this citation.</summary>
        public void SetLocation(Guid? locationId, IModificationContext context)
        {
            LocationId = locationId;
        }

        // -------------------------------------------------------
        // Soft delete overrides
        // -------------------------------------------------------
        public override void SoftDelete(Guid userId)
        {
            base.SoftDelete(userId);
            AddDomainEvent(new CitationSoftDeletedDomainEvent(Id, userId));
        }

        public override void Restore(Guid userId)
        {
            base.Restore(userId);
            AddDomainEvent(new CitationRestoredDomainEvent(Id, userId));
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
