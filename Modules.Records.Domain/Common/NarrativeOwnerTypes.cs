namespace Modules.Records.Domain.Common;

/// <summary>
/// The record types a narrative document can be attached to. Narratives are a
/// standalone, reusable aggregate linked to owners via <see cref="Entities.NarrativeLink"/>
/// (the same polymorphic pattern as Mugshot/MugshotLink), so a future module
/// only needs to add its constant here to participate.
/// </summary>
public static class NarrativeOwnerTypes
{
    public const string Incident = "Incident";
    public const string Arrest = "Arrest";
    public const string Citation = "Citation";

    public static bool IsSupported(string? ownerType) =>
        ownerType is Incident or Arrest or Citation;
}
