document.addEventListener("DOMContentLoaded", () => {

    const textSizeToggle = document.getElementById("text-size");

    // Load saved setting
    const savedTextSize = localStorage.getItem("largeText") === "true";

    // Always apply the setting globally
    applyTextSize(savedTextSize);

    // Only attach event listener if toggle exists on this page
    if (textSizeToggle) {
        textSizeToggle.checked = savedTextSize;

        textSizeToggle.addEventListener("change", () => {
            const enabled = textSizeToggle.checked;
            localStorage.setItem("largeText", enabled);
            applyTextSize(enabled);
        });
    }
});

function applyTextSize(enabled) {
    if (enabled) {
        document.body.classList.add("large-text");
    } else {
        document.body.classList.remove("large-text");
    }
}
