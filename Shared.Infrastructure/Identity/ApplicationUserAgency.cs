namespace Shared.Infrastructure.Identity;

public sealed class ApplicationUserAgency
{
    public string UserId { get; private set; } = string.Empty;
    public Guid AgencyId { get; private set; }

    private ApplicationUserAgency() { }

    public static ApplicationUserAgency Create(string userId, Guid agencyId)
        => new() { UserId = userId, AgencyId = agencyId };
}
