namespace Modules.Records.UI.Services;

/// <summary>Effective keyboard shortcut bindings for the current jurisdiction (with defaults applied).</summary>
public sealed record HotkeyBindings(
    string New,
    string Modify,
    string Save,
    string Release)
{
    public static readonly HotkeyBindings Default = new("Alt+N", "F2", "Alt+S", "Escape");

    public string ForAction(string action) => action switch
    {
        "new"     => New,
        "modify"  => Modify,
        "save"    => Save,
        "release" => Release,
        _         => string.Empty
    };
}
