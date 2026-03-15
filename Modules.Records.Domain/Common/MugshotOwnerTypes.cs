namespace Modules.Records.Domain.Common;

public static class MugshotOwnerTypes
{
    public const string Name = nameof(Name);
    public const string Arrest = nameof(Arrest);

    public static bool IsSupported(string? ownerType) =>
        ownerType is Name or Arrest;
}
