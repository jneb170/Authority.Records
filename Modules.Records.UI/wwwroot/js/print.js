export function triggerPrint(title) {
    const originalTitle = document.title;

    if (title && typeof title === "string") {
        document.title = title;
    }

    let fallbackTimer = null;

    const restoreTitle = () => {
        document.title = originalTitle;
        window.removeEventListener("afterprint", restoreTitle);
        if (fallbackTimer !== null) {
            clearTimeout(fallbackTimer);
            fallbackTimer = null;
        }
    };

    window.addEventListener("afterprint", restoreTitle);
    window.print();

    // Long-running fallback in case afterprint never fires (e.g. some browsers/environments).
    fallbackTimer = window.setTimeout(restoreTitle, 30000);
}
