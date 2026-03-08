namespace Modules.Records.UI.Services;

public sealed class KeyboardShortcutService : IKeyboardShortcutService
{
    public event Action? OnNew;
    public event Action? OnModify;
    public event Action? OnRelease;
    public event Action? OnSave;

    public void Invoke(string key)
    {
        switch (key)
        {
            case "new":     OnNew?.Invoke();     break;
            case "modify":  OnModify?.Invoke();  break;
            case "release": OnRelease?.Invoke(); break;
            case "save":    OnSave?.Invoke();    break;
        }
    }
}
