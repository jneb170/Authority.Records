// Global keyboard shortcut handler for Authority.Records.
// Call registerShortcuts(dotnetRef) once from Blazor; unregisterShortcuts() on dispose.

let _dotnetRef = null;
let _listener  = null;

const BLOCKED_TAGS = new Set(['INPUT', 'TEXTAREA', 'SELECT']);

function handleKeyDown(e) {
    if (!_dotnetRef) return;

    const inField = BLOCKED_TAGS.has(document.activeElement?.tagName);

    let key = null;

    if (e.altKey && !e.ctrlKey && !e.shiftKey) {
        if (e.key === 'n' || e.key === 'N') key = 'new';
        else if (e.key === 's' || e.key === 'S') key = 'save';
    } else if (!e.ctrlKey && !e.shiftKey && !e.altKey) {
        if (e.key === 'F2')          key = 'modify';
        else if (e.key === 'Escape') key = 'release';
    }

    if (!key) return;

    e.preventDefault();
    e.stopPropagation();

    _dotnetRef.invokeMethodAsync('InvokeShortcut', key);
}

export function registerShortcuts(dotnetRef) {
    _dotnetRef = dotnetRef;
    _listener  = handleKeyDown;
    document.addEventListener('keydown', _listener, true);
}

export function unregisterShortcuts() {
    if (_listener) {
        document.removeEventListener('keydown', _listener, true);
        _listener = null;
    }
    _dotnetRef = null;
}
