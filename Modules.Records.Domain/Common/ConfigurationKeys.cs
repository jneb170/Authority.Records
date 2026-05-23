namespace Modules.Records.Domain.Common;

/// <summary>Well-known keys for AgencyConfiguration entries.</summary>
public static class ConfigurationKeys
{
    /// <summary>
    /// Format string used to auto-generate IncidentNum values.
    /// Supported tokens: yyyy (4-digit year), yy (2-digit year), mm (month), dd (day),
    /// and any run of x characters (e.g. xxxxx = 5-digit zero-padded sequence).
    /// Example: "yyyy-mmdd-xxxxxx" → "2026-0307-000001"
    /// </summary>
    public const string IncidentFormat = "IncidentFormat";

    /// <summary>
    /// System default format used when no agency-specific IncidentFormat is configured.
    /// Produces values like "2026-000001".
    /// </summary>
    public const string DefaultIncidentFormat = "yyyy-xxxxxx";

    /// <summary>
    /// Format string used to auto-generate ArrestNum values.
    /// Supported tokens: yyyy, yy, mm, dd, and any run of x characters.
    /// Example: "AR-yyyy-xxxxxx" → "AR-2026-000001"
    /// </summary>
    public const string ArrestFormat = "ArrestFormat";

    /// <summary>
    /// System default format used when no agency-specific ArrestFormat is configured.
    /// Produces values like "AR-2026-000001".
    /// </summary>
    public const string DefaultArrestFormat = "AR-yyyy-xxxxxx";

    /// <summary>
    /// Format string used to auto-generate CitationNum values.
    /// Supported tokens: yyyy, yy, mm, dd, and any run of x characters.
    /// Example: "CT-yyyy-xxxxxx" → "CT-2026-000001"
    /// </summary>
    public const string CitationFormat = "CitationFormat";

    /// <summary>
    /// System default format used when no agency-specific CitationFormat is configured.
    /// Produces values like "CT-2026-000001".
    /// </summary>
    public const string DefaultCitationFormat = "CT-yyyy-xxxxxx";
    /// <summary>
    /// Schedule for automatic read model rebuilds.
    /// Valid values: "Off", "Hourly", "TwiceDaily", "Daily".  Default is "Off".
    /// </summary>
    public const string ReadModelRebuildSchedule = "ReadModelRebuildSchedule";

    /// <summary>
    /// UTC timestamp of the last completed read model rebuild (ISO 8601 string).
    /// Written automatically by the background rebuild service.
    /// </summary>
    public const string ReadModelRebuildLastRunUtc = "ReadModelRebuildLastRunUtc";

    /// <summary>
    /// Latitude and longitude used to center the Google Maps picker for this agency.
    /// Format: "latitude,longitude" (decimal degrees), e.g. "41.8781,-87.6298" for Chicago, IL.
    /// If not set, the map defaults to the geographic center of the United States (39.8283,-98.5795).
    /// </summary>
    public const string MapStartCoordinates = "MapStartCoordinates";

    /// <summary>
    /// How long (in whole seconds) a pessimistic record lock is held before it is
    /// considered expired. Governs both lock acquisition (whether another user may
    /// take over a stale lock) and the background lock-cleanup sweep. Stored as a
    /// string integer, e.g. "20". If unset or unparsable, falls back to
    /// <see cref="DefaultLockTimeoutSeconds"/>.
    /// </summary>
    public const string LockTimeoutSeconds = "LockTimeoutSeconds";

    /// <summary>
    /// System default lock timeout used when no agency-specific
    /// <see cref="LockTimeoutSeconds"/> is configured. 600 seconds = 10 minutes.
    /// </summary>
    public const int DefaultLockTimeoutSeconds = 600;
}
