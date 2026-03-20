export function triggerPrint(title) {
    const originalTitle = document.title;

    if (title && typeof title === "string") {
        document.title = title;
    }

    const restoreTitle = () => {
        document.title = originalTitle;
        window.removeEventListener("afterprint", restoreTitle);
    };

    window.addEventListener("afterprint", restoreTitle);

    try {
        window.print();
    } finally {
        window.setTimeout(restoreTitle, 1000);
    }
}
