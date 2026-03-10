namespace Modules.Records.UI.Services;

public interface IHotkeyConfigService
{
    /// <summary>Returns the effective bindings for the current jurisdiction (null DB values replaced by defaults).</summary>
    Task<HotkeyBindings> GetBindingsAsync();

    /// <summary>Clears the cached bindings so the next call re-reads from the database.</summary>
    void InvalidateCache();
}
