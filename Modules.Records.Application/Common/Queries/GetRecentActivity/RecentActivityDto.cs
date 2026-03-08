namespace Modules.Records.Application.Common.Queries.GetRecentActivity;

/// <summary>
/// A single entry in the home-page "recent activity" feed.
/// Represents one record (Incident, Arrest, Citation, or Name) that
/// the requesting user created or last modified.
/// </summary>
public sealed record RecentActivityDto(
    string   RecordType,        // "Incident" | "Arrest" | "Citation" | "Name"
    Guid     RecordId,
    string   DisplayIdentifier, // e.g. "INC-2025-0001", "John Smith", "#42"
    string   NavigationUrl,     // e.g. "/incidents/{id}"
    string?  Status,            // null for Name records (no status field)
    string   ActivityKind,      // "Created" | "Modified"
    DateTime LastActivityAt);
