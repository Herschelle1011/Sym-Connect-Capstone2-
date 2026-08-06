// ── Modal opener (global function for inline onclick calls) ──
function openModal(title, url) {
    document.getElementById("appModalLabel").innerText = title;

    fetch(url)
        .then(response => response.text())
        .then(html => {
            document.getElementById("appModalBody").innerHTML = html;

            const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById("appModal"));
            modal.show();

            if (window.jQuery && $.validator) {
                $.validator.unobtrusive.parse("#appModalBody");
            }
        });
}

// ── Modal opener (data-attribute buttons, e.g. "Create Partylist") ──
document.querySelectorAll(".open-modal").forEach(button => {
    button.addEventListener("click", function () {
        const url = this.getAttribute("data-url");
        const title = this.getAttribute("data-title");
        openModal(title, url);
    });
});

// ── Generic AJAX submit handler for any modal form ────────────
// Add class="modal-ajax-form" to any form loaded into the modal
// to have it auto-submit via fetch and refresh/close on success.
document.addEventListener("submit", async function (e) {
    if (!e.target.classList.contains("modal-ajax-form")) return;

    e.preventDefault();
    const form = e.target;

    if (form.dataset.submitting === "true") return; // guard against double-fire
    form.dataset.submitting = "true";

    try {
        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form)
        });

        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
            const result = await response.json();
            if (result.success) {
                location.reload();
            }
        } else {
            const html = await response.text();
            document.getElementById("appModalBody").innerHTML = html;
        }
    } finally {
        form.dataset.submitting = "false";
    }
});


// ── Position status toggle (instant AJAX, no modal) ──
document.addEventListener("change", function (e) {
    if (!e.target.classList.contains("toggle-status")) return;

    const id = e.target.dataset.positionId;
    const isActive = e.target.checked;

    fetch("/Position/ToggleStatus", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `id=${id}&isActive=${isActive}`
    })
        .then(res => res.json())
        .then(result => {
            if (!result.success) {
                e.target.checked = !isActive; // revert on failure
            }
        });
});