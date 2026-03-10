// shortcuts.js — Dynamic keyboard shortcut handler for Authority.Records.
// Call registerShortcuts(dotnetRef, bindings) once from Blazor; unregisterShortcuts() on dispose.
// bindings = { new: "Alt+N", modify: "F2", save: "Alt+S", release: "Escape" }

let _dotnetRef      = null;
let _listener       = null;
let _parsedBindings = {};

function parseBinding(str) {
    if (!str) return null;
    const parts  = str.split('+');
    const rawKey = parts[parts.length - 1].trim();
    const mods   = parts.slice(0, -1).map(m => m.trim().toLowerCase());

    // Normalize known special keys; everything else upper-cased for comparison
    const keyMap = { 'escape': 'Escape', 'enter': 'Enter', 'tab': 'Tab', 'space': ' ' };
    const key = keyMap[rawKey.toLowerCase()] ?? rawKey.toUpperCase();

    return {
        key,
        alt:   mods.includes('alt'),
        ctrl:  mods.includes('ctrl'),
        shift: mods.includes('shift'),
    };
}

function handleKeyDown(e) {
    if (!_dotnetRef) return;

    for (const [action, parsed] of Object.entries(_parsedBindings)) {
        if (!parsed) continue;
        const keyMatch = e.key === parsed.key || e.key.toUpperCase() === parsed.key.toUpperCase();
        if (keyMatch && e.altKey === parsed.alt && e.ctrlKey === parsed.ctrl && e.shiftKey === parsed.shift) {
            e.preventDefault();
            e.stopPropagation();
            _dotnetRef.invokeMethodAsync('InvokeShortcut', action);
            return;
        }
    }
}

export function registerShortcuts(dotnetRef, bindings) {
    _dotnetRef = dotnetRef;
    _parsedBindings = {
        new:     parseBinding(bindings?.new),
        modify:  parseBinding(bindings?.modify),
        save:    parseBinding(bindings?.save),
        release: parseBinding(bindings?.release),
    };
    _listener = handleKeyDown;
    document.addEventListener('keydown', _listener, true);
}

export function unregisterShortcuts() {
    if (_listener) {
        document.removeEventListener('keydown', _listener, true);
        _listener = null;
    }
    _dotnetRef      = null;
    _parsedBindings = {};
}

