let _dotnetRef = null;
let _throttleMs = 15000;
let _lastActivityAt = 0;
let _listeners = [];

function notifyActivity() {
    if (!_dotnetRef) {
        return;
    }

    const now = Date.now();
    if (now - _lastActivityAt < _throttleMs) {
        return;
    }

    _lastActivityAt = now;
    _dotnetRef.invokeMethodAsync('RecordClientActivity').catch(() => {});
}

function addTrackedListener(target, eventName, options) {
    const listener = () => notifyActivity();
    target.addEventListener(eventName, listener, options);
    _listeners.push(() => target.removeEventListener(eventName, listener, options));
}

export function registerActivityTracking(dotnetRef, throttleMs) {
    unregisterActivityTracking();

    _dotnetRef = dotnetRef;
    _throttleMs = throttleMs ?? 15000;
    _lastActivityAt = 0;

    addTrackedListener(document, 'pointerdown', true);
    addTrackedListener(document, 'keydown', true);
    addTrackedListener(document, 'touchstart', true);
    addTrackedListener(window, 'scroll', { passive: true });

    const visibilityListener = () => {
        if (document.visibilityState === 'visible') {
            notifyActivity();
        }
    };

    document.addEventListener('visibilitychange', visibilityListener, true);
    _listeners.push(() => document.removeEventListener('visibilitychange', visibilityListener, true));

    notifyActivity();
}

export function unregisterActivityTracking() {
    for (const unregister of _listeners) {
        unregister();
    }

    _listeners = [];
    _dotnetRef = null;
    _lastActivityAt = 0;
}
