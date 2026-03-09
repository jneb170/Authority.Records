namespace Shared.Infrastructure.Identity;

public sealed class Agency
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Agency() { }

    public static Agency Create(Guid jurisdictionId, string name, string code)
        => new()
        {
            Id = Guid.NewGuid(),
            JurisdictionId = jurisdictionId,
            Name = name,
            Code = code,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

    public void Update(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
