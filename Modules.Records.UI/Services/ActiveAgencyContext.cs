using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Shared.Infrastructure.Identity;

namespace Modules.Records.UI.Services;

public sealed record ActiveAgencyOption(Guid Id, string Name, string Code, bool IsDefault);

public interface IActiveAgencyContext
{
    event Action? Changed;

    IReadOnlyList<ActiveAgencyOption> AvailableAgencies { get; }
    Guid ActiveAgencyId { get; }
    Guid DefaultAgencyId { get; }
    string? ActiveAgencyName { get; }
    bool HasLoaded { get; }
    bool HasAvailableAgencies { get; }
    bool HasSelectedAgency { get; }
    bool IsSelectionLocked { get; }

    Task EnsureLoadedAsync();
    Task<bool> SetActiveAgencyAsync(Guid agencyId);
    IDisposable AcquireSelectionLock();
}

public sealed class ActiveAgencyContext(
    AuthenticationStateProvider authStateProvider,
    IUserManagementService userManagementService,
    IAgencyManagementService agencyManagementService,
    IHttpContextAccessor httpContextAccessor,
    IJSRuntime jsRuntime) : IActiveAgencyContext
{
    private const string ActiveAgencyCookieName = "authority-records-active-agency";
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private List<ActiveAgencyOption> _availableAgencies = [];
    private Guid _activeAgencyId;
    private Guid _defaultAgencyId;
    private bool _hasLoaded;
    private int _selectionLockCount;

    public event Action? Changed;

    public IReadOnlyList<ActiveAgencyOption> AvailableAgencies => _availableAgencies;
    public Guid ActiveAgencyId => _activeAgencyId;
    public Guid DefaultAgencyId => _defaultAgencyId;
    public string? ActiveAgencyName => _availableAgencies.FirstOrDefault(x => x.Id == _activeAgencyId)?.Name;
    public bool HasLoaded => _hasLoaded;
    public bool HasAvailableAgencies => _availableAgencies.Count > 0;
    public bool HasSelectedAgency => _activeAgencyId != Guid.Empty;
    public bool IsSelectionLocked => _selectionLockCount > 0;

    public async Task EnsureLoadedAsync()
    {
        if (_hasLoaded)
            return;

        await _loadGate.WaitAsync();
        try
        {
            if (_hasLoaded)
                return;

            await LoadAsync();
            _hasLoaded = true;
        }
        finally
        {
            _loadGate.Release();
        }

        NotifyChanged();
    }

    public async Task<bool> SetActiveAgencyAsync(Guid agencyId)
    {
        await EnsureLoadedAsync();

        if (IsSelectionLocked)
            return false;

        if (agencyId == Guid.Empty || _availableAgencies.All(x => x.Id != agencyId))
            return false;

        if (_activeAgencyId == agencyId)
            return true;

        _activeAgencyId = agencyId;
        await PersistSelectionAsync(agencyId);
        NotifyChanged();
        return true;
    }

    public IDisposable AcquireSelectionLock()
    {
        Interlocked.Increment(ref _selectionLockCount);
        NotifyChanged();
        return new SelectionLockHandle(this);
    }

    private async Task LoadAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        _defaultAgencyId = ParseGuid(user.FindFirst("agency")?.Value);
        _activeAgencyId = _defaultAgencyId;

        if (user.Identity?.IsAuthenticated != true || user.IsInRole("Super"))
        {
            _availableAgencies = [];
            _activeAgencyId = Guid.Empty;
            _defaultAgencyId = Guid.Empty;
            return;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            _availableAgencies = [];
            _activeAgencyId = Guid.Empty;
            return;
        }

        var userProfile = await userManagementService.GetByIdAsync(userId);
        if (userProfile is not null && userProfile.PrimaryAgencyId != Guid.Empty)
            _defaultAgencyId = userProfile.PrimaryAgencyId;

        var agencies = await userManagementService.GetAgenciesForUserAsync(userId);

        if (userProfile is not null &&
            userProfile.PrimaryAgencyId != Guid.Empty &&
            agencies.All(x => x.Id != userProfile.PrimaryAgencyId))
        {
            var primaryAgency = await agencyManagementService.GetByIdAsync(userProfile.PrimaryAgencyId);
            if (primaryAgency is not null)
                agencies.Add(primaryAgency);
        }

        _availableAgencies = agencies
            .Where(x => x.IsActive)
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .OrderBy(x => x.Name)
            .Select(x => new ActiveAgencyOption(x.Id, x.Name, x.Code, x.Id == _defaultAgencyId))
            .ToList();

        var persistedAgencyId = ParseGuid(httpContextAccessor.HttpContext?.Request.Cookies[ActiveAgencyCookieName]);
        var initialAgencyId = persistedAgencyId != Guid.Empty ? persistedAgencyId : _activeAgencyId;
        _activeAgencyId = ResolveInitialAgencySelection(initialAgencyId);
    }

    private Guid ResolveInitialAgencySelection(Guid currentSelection)
    {
        if (_availableAgencies.Count == 0)
            return Guid.Empty;

        if (currentSelection != Guid.Empty && _availableAgencies.Any(x => x.Id == currentSelection))
            return currentSelection;

        if (_defaultAgencyId != Guid.Empty && _availableAgencies.Any(x => x.Id == _defaultAgencyId))
            return _defaultAgencyId;

        return _availableAgencies[0].Id;
    }

    private void ReleaseSelectionLock()
    {
        var updatedValue = Interlocked.Decrement(ref _selectionLockCount);
        if (updatedValue < 0)
            Interlocked.Exchange(ref _selectionLockCount, 0);

        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke();

    private ValueTask PersistSelectionAsync(Guid agencyId)
        => jsRuntime.InvokeVoidAsync("authorityRecordsAgency.setActiveAgency", ActiveAgencyCookieName, agencyId.ToString());

    private static Guid ParseGuid(string? value)
        => Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty;

    private sealed class SelectionLockHandle(ActiveAgencyContext owner) : IDisposable
    {
        private ActiveAgencyContext? _owner = owner;

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.ReleaseSelectionLock();
        }
    }
}
