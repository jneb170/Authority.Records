namespace Shared.Infrastructure.Identity;

public sealed class Jurisdiction
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Jurisdiction() { }

    public static Jurisdiction Create(string name, string state, string code)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            State = state,
            Code = code,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

    public void Update(string name, string state, string code)
    {
        Name = name;
        State = state;
        Code = code;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
