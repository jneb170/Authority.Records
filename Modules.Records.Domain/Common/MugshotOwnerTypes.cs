namespace Modules.Records.Domain.Common;

public static class MugshotOwnerTypes
{
    public const string Name = "Name";
    public const string Arrest = "Arrest";

    public static bool IsSupported(string? ownerType) =>
        ownerType is Name or Arrest;
}
