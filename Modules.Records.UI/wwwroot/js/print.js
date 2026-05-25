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

// Loads a PDF URL into a hidden iframe and invokes the browser's native print dialog on it. That
// dialog is the OS print UI: it lists every installed printer and offers "Save as PDF" — so a single
// "Print" button gives the user both options without the app enumerating printers itself.
export function printPdf(url) {
    const existing = document.getElementById("pdf-print-frame");
    if (existing) existing.remove();

    const iframe = document.createElement("iframe");
    iframe.id = "pdf-print-frame";
    iframe.style.position = "fixed";
    iframe.style.right = "0";
    iframe.style.bottom = "0";
    iframe.style.width = "0";
    iframe.style.height = "0";
    iframe.style.border = "0";
    iframe.src = url;
    iframe.onload = () => {
        try {
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
        } catch (e) {
            // Fallback: open the PDF in a new tab so the user can print from the viewer's controls.
            window.open(url, "_blank", "noopener");
        }
    };
    document.body.appendChild(iframe);
}
