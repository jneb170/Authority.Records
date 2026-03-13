using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities
{
    public sealed class Arrest : LockableAggregateRoot<Arrest>, IMultiTenant
    {
        public Guid JurisdictionId { get; private set; }
        public Guid AgencyId { get; private set; }
        public string SuspectName { get; private set; }
        public DateTime ArrestedAt { get; private set; }
        public bool IsFinalized { get; private set; }

        /// <summary>DB-generated auto-increment number. Use this in URLs and display; the GUID is for internal identity.</summary>
        public long RecordNumber { get; private set; }

        /// <summary>Agency-formatted arrest number, e.g. "AR-2026-000001". Auto-generated on create.</summary>
        public string ArrestNum { get; private set; } = string.Empty;

        /// <summary>Optional reference to the agency-configured ArrestType picklist item.</summary>
        public Guid? ArrestTypeId { get; private set; }

        /// <summary>Optional reference to a Master Location Index record for the arrest location.</summary>
        public Guid? LocationId { get; private set; }

        private static ILifecyclePolicy<Arrest> _lifecyclePolicy;

        // Inject lifecycle policy from composition root / factory
        public static void SetLifecyclePolicy(ILifecyclePolicy<Arrest> policy)
        {
            _lifecyclePolicy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        private static readonly ArrestAuthorizationPolicy _authorizationPolicy
            = new();
        protected override IAuthorizationPolicy<Arrest> AuthorizationPolicy
            => _authorizationPolicy;

        private static readonly TimeoutLockExpirationStrategy<Arrest> _lockExpirationStrategy
            = new();
        protected override ILockExpirationStrategy<Arrest> LockExpirationStrategy
            => _lockExpirationStrategy;

        private static readonly SystemClock _clock
            = new();
        protected override IClock Clock
            => _clock;

        // -------------------------------
        // Constructor
        // -------------------------------
        private Arrest() { } // EF Core materialization — must NOT raise domain events

        public Arrest(Guid jurisdictionId, Guid agencyId, string suspectName, DateTime arrestedAt, string arrestNum)
        {
            Id = Guid.NewGuid();
            JurisdictionId = jurisdictionId;
            AgencyId = agencyId;
            SuspectName = suspectName ?? string.Empty;

            ArrestedAt = arrestedAt;
            ArrestNum  = arrestNum;

            Status = RecordStatus.Draft;

            AddDomainEvent(new ArrestCreatedDomainEvent(Id, JurisdictionId, SuspectName, ArrestedAt, ArrestNum));
        }

        // ----------------------------------------------------
        // Domain Behavior
        // ----------------------------------------------------
        public void Open(
            IModificationContext context,
            ILifecyclePolicy<Arrest> lifecyclePolicy)
            => ChangeStatus(RecordStatus.Open, context, lifecyclePolicy);

        public void Close(
            IModificationContext context,
            ILifecyclePolicy<Arrest> lifecyclePolicy,
            bool force = false)
            => ChangeStatus(RecordStatus.Closed, context, lifecyclePolicy, force);
        public void Archive(
            IModificationContext context,
            ILifecyclePolicy<Arrest> lifecyclePolicy)
            => ChangeStatus(RecordStatus.Archived, context, lifecyclePolicy);

        public void Finalize() => IsFinalized = true;

        public void UpdateDetails(string suspectName, DateTime arrestedAt, Guid? arrestTypeId, string arrestNum, IModificationContext context)
        {
            SuspectName  = suspectName ?? string.Empty;
            ArrestedAt   = arrestedAt;
            ArrestTypeId = arrestTypeId;
            ArrestNum    = arrestNum;

            AddDomainEvent(new ArrestDetailsUpdatedDomainEvent(Id, SuspectName, ArrestedAt, ArrestTypeId, ArrestNum));
        }

        /// <summary>Sets or clears the linked Master Location Index record for this arrest.</summary>
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
            AddDomainEvent(new ArrestSoftDeletedDomainEvent(Id, userId));
        }

        public override void Restore(Guid userId)
        {
            base.Restore(userId);
            AddDomainEvent(new ArrestRestoredDomainEvent(Id, userId));
        }

    }


}
