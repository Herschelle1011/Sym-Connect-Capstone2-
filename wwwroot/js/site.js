
// ── Modal opener ──────────────────────────────────────────────
document.querySelectorAll(".open-modal").forEach(button => {
    button.addEventListener("click", async function () {
        const url = this.getAttribute("data-url");
        const title = this.getAttribute("data-title");

        document.getElementById("appModalLabel").innerText = title;

        const response = await fetch(url);
        const html = await response.text();

        document.getElementById("appModalBody").innerHTML = html;

        const modal = new bootstrap.Modal(document.getElementById("appModal"));
        modal.show();

        if (window.jQuery && $.validator) {
            $.validator.unobtrusive.parse("#appModalBody");
        }
    });
});