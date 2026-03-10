// keyCapture.js — Single keypress capture helper for hotkey configuration.

let _captureResolver = null;
let _captureListener = null;

const IGNORED_KEYS = new Set(['Control', 'Alt', 'Shift', 'Meta', 'CapsLock', 'NumLock', 'ScrollLock']);

function captureKeyDown(e) {
    if (IGNORED_KEYS.has(e.key)) return;

    e.preventDefault();
    e.stopPropagation();

    const parts = [];
    if (e.ctrlKey)  parts.push('Ctrl');
    if (e.altKey)   parts.push('Alt');
    if (e.shiftKey) parts.push('Shift');

    // Format the key portion
    let key = e.key;
    if (key === ' ') key = 'Space';
    // Single printable letter → upper case
    if (key.length === 1) key = key.toUpperCase();

    parts.push(key);
    const binding = parts.join('+');

    document.removeEventListener('keydown', _captureListener, true);
    _captureListener = null;

    if (_captureResolver) {
        _captureResolver(binding);
        _captureResolver = null;
    }
}

export function captureNextKey() {
    return new Promise(resolve => {
        _captureResolver = resolve;
        _captureListener = captureKeyDown;
        document.addEventListener('keydown', _captureListener, true);
    });
}

export function cancelCapture() {
    if (_captureListener) {
        document.removeEventListener('keydown', _captureListener, true);
        _captureListener = null;
    }
    _captureResolver = null;
}
