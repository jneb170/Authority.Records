namespace Modules.Records.Domain.Abstractions;

/// <summary>
/// An aggregate that belongs to a single agency and must never be visible to, or changed by,
/// users of another agency in the same jurisdiction: incidents, arrests and citations.
/// Master-index records (names, locations) are deliberately shared by every agency in the
/// jurisdiction and do not implement this (issue #118).
/// </summary>
public interface IAgencyOwned
{
    Guid AgencyId { get; }
}
