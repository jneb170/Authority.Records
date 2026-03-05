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
        public Guid IncidentId { get; private set; }
        public string SuspectName { get; private set; }
        public DateTime ArrestedAt { get; private set; }
        public bool IsFinalized { get; private set; }

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
        public Arrest(Guid jurisdictionId, Guid agencyId, Guid incidentId, string suspectName, DateTime arrestedAt)
        {
            Id = Guid.NewGuid();
            JurisdictionId = jurisdictionId;
            AgencyId = agencyId;
            IncidentId = incidentId;

            SuspectName = !string.IsNullOrWhiteSpace(suspectName)
                ? suspectName
                : throw new DomainException("arrest.suspect.empty", "Suspect name cannot be empty.");

            ArrestedAt = arrestedAt;

            Status = RecordStatus.Draft;

            AddDomainEvent(new ArrestCreatedDomainEvent(Id, IncidentId, JurisdictionId, AgencyId, SuspectName, ArrestedAt));
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
