namespace Modules.Records.Application.Abstractions;

/// <summary>
/// Names of the EF Core global query filters on AppDbContext, so a query can switch off one
/// filter (<c>IgnoreQueryFilters([QueryFilterNames.Agency])</c>) while keeping the others.
/// </summary>
public static class QueryFilterNames
{
    /// <summary>Hides soft-deleted aggregates.</summary>
    public const string SoftDelete = "SoftDelete";

    /// <summary>Restricts every aggregate to the current jurisdiction (tenant).</summary>
    public const string Jurisdiction = "Jurisdiction";

    /// <summary>
    /// Restricts agency-owned aggregates (incidents, arrests, citations) to the signed-in user's
    /// active agency. Only jurisdiction-wide maintenance, such as the read-model rebuild, may
    /// switch it off.
    /// </summary>
    public const string Agency = "Agency";
}
