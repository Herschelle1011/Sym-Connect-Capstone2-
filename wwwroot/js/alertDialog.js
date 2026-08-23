// showAlertDialog(message, type, title)
// type: "success" | "error" | "info" (default) — controls icon + color
function showAlertDialog(message, type = "info", title = null) {
    const icon = document.getElementById("appAlertIconGlyph");
    const iconWrapper = document.getElementById("appAlertIcon");
    const titleEl = document.getElementById("appAlertTitle");
    const messageEl = document.getElementById("appAlertMessage");

    const configs = {
        success: {
            glyph: "bi-check-circle-fill",
            bg: "#dcfce7",
            color: "#166534"
        },

        error: {
            glyph: "bi-x-circle-fill",
            bg: "#fee2e2",
            color: "#991b1b"
        },

        info: {
            glyph: "bi-info-circle-fill",
            bg: "#dbeafe",
            color: "#1d4ed8"
        }
    };

    const config = configs[type] || configs.info;

    icon.className = `bi ${config.glyph}`;
    icon.style.color = config.color;
    iconWrapper.style.background = config.bg;

    titleEl.textContent = title || config.defaultTitle;
    messageEl.textContent = message;

    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById("appAlertModal"));
    modal.show();
}