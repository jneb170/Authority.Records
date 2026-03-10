using MediatR;

namespace Modules.Records.Application.Configurations.Commands.UpdateJurisdictionHotkeys;

/// <summary>Saves keyboard shortcut overrides for the current jurisdiction. Null = reset to application default.</summary>
public sealed record UpdateJurisdictionHotkeysCommand(
    string? HotkeyNew,
    string? HotkeyModify,
    string? HotkeySave,
    string? HotkeyRelease) : IRequest;
