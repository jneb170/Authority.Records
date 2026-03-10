namespace Modules.Records.Domain.Common;

public sealed class JurisdictionConfiguration
{
    public Guid Id { get; private set; }

    public Guid JurisdictionId { get; private set; }

    public bool MustCloseAllArrests { get; private set; }

    public bool MustCloseAllCitations { get; private set; }

    public bool MustCloseArrestsBeforeIncidentClose { get; private set; }

    // Keyboard shortcut overrides — null means "use the application default"
    public string? HotkeyNew { get; private set; }
    public string? HotkeyModify { get; private set; }
    public string? HotkeySave { get; private set; }
    public string? HotkeyRelease { get; private set; }

    private JurisdictionConfiguration() { }

    public JurisdictionConfiguration(Guid jurisdictionId, bool mustCloseAllArrests, bool mustCloseAllCitations)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        MustCloseAllArrests = mustCloseAllArrests;
        MustCloseAllCitations = mustCloseAllCitations;
    }

    public void UpdateHotkeys(string? hotkeyNew, string? hotkeyModify, string? hotkeySave, string? hotkeyRelease)
    {
        HotkeyNew     = string.IsNullOrWhiteSpace(hotkeyNew)     ? null : hotkeyNew.Trim();
        HotkeyModify  = string.IsNullOrWhiteSpace(hotkeyModify)  ? null : hotkeyModify.Trim();
        HotkeySave    = string.IsNullOrWhiteSpace(hotkeySave)    ? null : hotkeySave.Trim();
        HotkeyRelease = string.IsNullOrWhiteSpace(hotkeyRelease) ? null : hotkeyRelease.Trim();
    }
}