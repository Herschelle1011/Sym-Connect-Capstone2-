
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

// ── Search & status filter ────────────────────────────────────
const searchInput = document.getElementById("searchInput");
const statusButtons = document.querySelectorAll(".filter-tab");
const allRows = Array.from(
    document.querySelectorAll("#membersTable tbody tr:not(.empty-row)")
);

let currentStatus = "all";
let currentPage = 1;
const rowsPerPage = 10;

// Returns only the rows that pass the current search + status filter
function getVisibleRows() {
    const query = searchInput.value.toLowerCase();
    return allRows.filter(row => {
        const matchesSearch = row.innerText.toLowerCase().includes(query);
        const matchesStatus = currentStatus === "all" ||
            row.getAttribute("data-status") === currentStatus;
        return matchesSearch && matchesStatus;
    });
}

// ── Pagination renderer ───────────────────────────────────────
function renderPage() {
    const visible = getVisibleRows();
    const totalPages = Math.max(1, Math.ceil(visible.length / rowsPerPage));

    // Clamp page so it never goes out of range
    if (currentPage > totalPages) currentPage = totalPages;

    const start = (currentPage - 1) * rowsPerPage;
    const end = start + rowsPerPage;

    // Show / hide every row
    allRows.forEach(row => row.style.display = "none");
    visible.forEach((row, i) => {
        row.style.display = (i >= start && i < end) ? "" : "none";
    });

    // Update counter and buttons
    const showing = Math.min(end, visible.length);
    const from = visible.length === 0 ? 0 : start + 1;
    document.getElementById("pageInfo").textContent =
        `Showing ${from}–${showing} of ${visible.length} members`;
    document.getElementById("pageNum").textContent =
        `Page ${currentPage} of ${totalPages}`;
    document.getElementById("prevBtn").disabled = currentPage === 1;
    document.getElementById("nextBtn").disabled = currentPage === totalPages;
}

// ── Pagination button events ──────────────────────────────────
document.getElementById("prevBtn").addEventListener("click", () => {
    currentPage--;
    renderPage();
});

document.getElementById("nextBtn").addEventListener("click", () => {
    currentPage++;
    renderPage();
});

// ── Search event ──────────────────────────────────────────────
searchInput.addEventListener("input", () => {
    currentPage = 1;   // reset to page 1 on new search
    renderPage();
});

// ── Status tab events ─────────────────────────────────────────
statusButtons.forEach(button => {
    button.addEventListener("click", function () {
        statusButtons.forEach(btn => btn.classList.remove("active-tab"));
        this.classList.add("active-tab");
        currentStatus = this.getAttribute("data-status");
        currentPage = 1;   // reset to page 1 on filter change
        renderPage();
    });
});

// ── First render ──────────────────────────────────────────────
renderPage();

