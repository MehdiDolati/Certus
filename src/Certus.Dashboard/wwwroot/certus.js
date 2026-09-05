// Certus host helpers for the data-validation integration (spec 006).
// The validator boundary hands the host typed data; this helper only
// delivers it to the user.
window.certus = window.certus || {};

// Streams a report export to the browser as a file download.
window.certus.downloadReport = function (runId, representation, text) {
    const mime = representation.startsWith("Json")
        ? "application/json"
        : "text/plain";
    const extension = representation.startsWith("Json") ? ".json" : ".txt";
    const blob = new Blob([text], { type: mime + ";charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = "validation-" + runId + "-" + representation + extension;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
};