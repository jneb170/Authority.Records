using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// A standalone, editable long-form narrative document. Attached to one or more
/// owners (Incident/Arrest/Citation, and future modules) via <see cref="NarrativeLink"/>.
/// Editable and lockable like other records — pessimistic locking guards concurrent
/// edits — but suited to long-form writing (a generous content ceiling, its own page).
/// </summary>
public sealed class Narrative : LockableAggregateRoot<Narrative>, IMultiTenant
{
    /// <summary>Upper bound on the title.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>
    /// Upper bound on the long-form body. Generous (narratives are the long-form home),
    /// but bounded so a single document can't be stuffed without limit.
    /// </summary>
    public const int MaxContentLength = 1_000_000;

    public Guid JurisdictionId { get; private set; }

    /// <summary>DB-generated auto-increment. Use in URLs and display; the GUID is internal identity.</summary>
    public long RecordNumber { get; private set; }

    /// <summary>Short label for the document, e.g. "Initial Report", "Follow-up".</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>The long-form narrative body.</summary>
    public string Content { get; private set; } = string.Empty;

    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>
    /// Transient lock-state field: the agency whose configured narrative-lock timeout governs an
    /// outstanding lock. Narratives have no permanent AgencyId (shared, like Location), but the
    /// timeout is configured per agency, so the cleanup sweep needs the locking agency. Set on
    /// <see cref="AcquireLock(IModificationContext, TimeSpan, Guid)"/>, cleared on release. Null when unlocked.
    /// </summary>
    public Guid? LockedByAgencyId { get; private set; }

    // --- Policy wiring (mirrors Location) ---
    private static readonly NarrativeAuthorizationPolicy _authorizationPolicy = new();
    protected override IAuthorizationPolicy<Narrative> AuthorizationPolicy => _authorizationPolicy;

    private static readonly TimeoutLockExpirationStrategy<Narrative> _lockExpirationStrategy = new();
    protected override ILockExpirationStrategy<Narrative> LockExpirationStrategy => _lockExpirationStrategy;

    private static readonly SystemClock _clock = new();
    protected override IClock Clock => _clock;

    private Narrative() { } // EF Core materialization

    public Narrative(Guid jurisdictionId, string title, string content)
    {
        Validate(title, content);

        Id             = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        Title          = title.Trim();
        Content        = content;

        AddDomainEvent(new NarrativeCreatedDomainEvent(Id, JurisdictionId, Title));
    }

    public void UpdateContent(string title, string content, IModificationContext context)
    {
        EnsureCanModify(context);
        Validate(title, content);

        Title   = title.Trim();
        Content = content;

        AddDomainEvent(new NarrativeContentUpdatedDomainEvent(Id, Title));
    }

    /// <summary>
    /// Acquires the edit lock and records the agency whose configured narrative timeout governs it.
    /// Narratives carry no permanent AgencyId, so the locking agency must be supplied by the caller.
    /// </summary>
    public void AcquireLock(IModificationContext context, TimeSpan lockTimeout, Guid lockingAgencyId)
    {
        base.AcquireLock(context, lockTimeout);
        LockedByAgencyId = lockingAgencyId;
    }

    public override void ReleaseLock(IModificationContext context)
    {
        base.ReleaseLock(context);
        LockedByAgencyId = null;
    }

    public override void SoftDelete(Guid userId)
    {
        base.SoftDelete(userId);
        DeletedBy    = userId;
        DeletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new NarrativeSoftDeletedDomainEvent(Id, userId));
    }

    public override void Restore(Guid userId)
    {
        base.Restore(userId);
        DeletedBy    = null;
        DeletedAtUtc = null;
        AddDomainEvent(new NarrativeRestoredDomainEvent(Id, userId));
    }

    private static void Validate(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("narrative.title.empty", "Narrative title is required.");

        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException(
                "narrative.title.length",
                $"Narrative title must not exceed {MaxTitleLength} characters.");

        if ((content?.Length ?? 0) > MaxContentLength)
            throw new DomainException(
                "narrative.content.length",
                $"Narrative content must not exceed {MaxContentLength:N0} characters.");
    }
}
