namespace Modules.Records.UI.Services;

public interface IKeyboardShortcutService
{
    event Action? OnNew;
    event Action? OnModify;
    event Action? OnRelease;
    event Action? OnSave;

    /// <summary>Called by <see cref="KeyboardShortcutHandler"/> when the JS layer detects a shortcut keystroke.</summary>
    void Invoke(string key);
}
