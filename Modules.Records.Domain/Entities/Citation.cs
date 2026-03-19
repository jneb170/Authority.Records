using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities
{
    public sealed class Citation : LockableAggregateRoot<Citation>, IMultiTenant
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

        private static readonly CitationAuthorizationPolicy _authorizationPolicy = new();
        protected override IAuthorizationPolicy<Citation> AuthorizationPolicy => _authorizationPolicy;

        private static readonly TimeoutLockExpirationStrategy<Citation> _lockExpirationStrategy = new();
        protected override ILockExpirationStrategy<Citation> LockExpirationStrategy => _lockExpirationStrategy;

        private static readonly SystemClock _clock = new();
        protected override IClock Clock => _clock;

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
            Status = RecordStatus.Draft;

            AddDomainEvent(new CitationCreatedDomainEvent(Id, JurisdictionId, Description, IssueDate, CitationNum));
        }

        public void Open(
            IModificationContext context,
            ILifecyclePolicy<Citation> lifecyclePolicy)
            => ChangeStatus(RecordStatus.Open, context, lifecyclePolicy);

        public void Close(
            IModificationContext context,
            ILifecyclePolicy<Citation> lifecyclePolicy,
            bool force = false)
            => ChangeStatus(RecordStatus.Closed, context, lifecyclePolicy, force);

        public void Archive(
            IModificationContext context,
            ILifecyclePolicy<Citation> lifecyclePolicy)
            => ChangeStatus(RecordStatus.Archived, context, lifecyclePolicy);

        public void Issue(IModificationContext context)
        {
            EnsureCanModify(context);

            if (Status != RecordStatus.Draft)
                EnsureUserOwnsLock(context.UserId);

            IsIssued = true;
            AddDomainEvent(new CitationIssuedDomainEvent(Id, context.UserId));
        }

        public void UpdateDetails(string description, DateTime issueDate, Guid? courtId, string citationNum, IModificationContext context)
        {
            EnsureCanModify(context);

            if (Status != RecordStatus.Draft)
                EnsureUserOwnsLock(context.UserId);

            Description = description;
            IssueDate   = issueDate;
            CourtId     = courtId;
            CitationNum = citationNum;

            AddDomainEvent(new CitationDetailsUpdatedDomainEvent(Id, Description, IssueDate, CourtId, CitationNum, LocationId, context.UserId));
        }

        /// <summary>Sets or clears the linked Master Location Index record for this citation.</summary>
        public void SetLocation(Guid? locationId, IModificationContext context)
        {
            EnsureCanModify(context);

            if (Status != RecordStatus.Draft)
                EnsureUserOwnsLock(context.UserId);

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
    }
}
