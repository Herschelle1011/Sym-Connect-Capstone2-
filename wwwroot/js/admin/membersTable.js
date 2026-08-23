

        // ── Search & status filter ────────────────────────────────────
        const searchInput = document.getElementById("searchInput");
        const statusButtons = document.querySelectorAll(".filter-tab");
        const allRows = Array.from(
        document.querySelectorAll("#membersTable tbody tr:not(.empty-row)")
        );

        let currentStatus = "all";
        let currentPage   = 1;
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
        const end   = start + rowsPerPage;

            // Show / hide every row
            allRows.forEach(row => row.style.display = "none");
            visible.forEach((row, i) => {
            row.style.display = (i >= start && i < end) ? "" : "none";
            });

        // Update counter and buttons
        const showing = Math.min(end, visible.length);
        const from    = visible.length === 0 ? 0 : start + 1;
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

function togglePhotoMode(mode) {
    const uploadWrapper = document.getElementById('uploadInputWrapper');
    const urlWrapper = document.getElementById('urlInputWrapper');
    const photoInput = document.getElementById('photoInput');
    const photoUrlInput = document.getElementById('photoUrlInput');
    const preview = document.getElementById('photoPreview');
    const placeholder = document.getElementById('placeholderIcon');

    if (mode === 'upload') {
        uploadWrapper.style.display = 'block';
        urlWrapper.style.display = 'none';
        photoUrlInput.value = '';           // clear so only one source is submitted
    } else {
        uploadWrapper.style.display = 'none';
        urlWrapper.style.display = 'block';
        photoInput.value = '';              // clear file input
    }

    preview.src = '';
    preview.style.display = 'none';
    placeholder.style.display = 'block';
}

function previewProfilePhoto(input) {
    const preview = document.getElementById('photoPreview');
    const placeholder = document.getElementById('placeholderIcon');

    if (input.files && input.files[0]) {
        const blobUrl = URL.createObjectURL(input.files[0]);
        preview.src = blobUrl;
        preview.style.display = 'block';
        placeholder.style.display = 'none';

        preview.onload = function () {
            URL.revokeObjectURL(blobUrl);
        };
    } else {
        preview.src = "";
        preview.style.display = 'none';
        placeholder.style.display = 'block';
    }
}

function previewFromUrl(url) {
    const preview = document.getElementById('photoPreview');
    const placeholder = document.getElementById('placeholderIcon');

    if (url) {
        preview.src = url;
        preview.style.display = 'block';
        placeholder.style.display = 'none';
    } else {
        preview.src = '';
        preview.style.display = 'none';
        placeholder.style.display = 'block';
    }
}



//LOADING INDICATOR


document.addEventListener("submit", async function (e) {
    if (e.target.id !== "createMemberForm")
        return;

    e.preventDefault();


const createMemberForm = e.target;
const submitBtn = createMemberForm.querySelector("#submitBtn");


    submitBtn.disabled = true;

    submitBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>
            Sending verification email...`;

    const formData = new FormData(createMemberForm);
    try {
        const response = await fetch(createMemberForm.action, {
            method: "POST",
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            submitBtn.innerHTML = `<i class="bi bi-check-circle me-1"></i>
                    Email Sent`;

            alert(result.message);
        }
        else {
            alert(result.message);

            submitBtn.disabled = false;
            submitBtn.innerHTML = "Create";

        }
    }
    catch (error) {
        console.error(error);

        alert("Something went wrong while creating the member.");

        submitBtn.disabled = false;
        submitBtn.innerHTML = "Create";
    }
});
