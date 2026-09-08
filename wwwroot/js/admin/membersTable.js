// ======================================================
// TEST APPROVE / REJECT BUTTONS
// ======================================================
document.addEventListener("click", async function (event) {

    const trigger = event.target.closest(".open-modal");

    if (!trigger) {
        return;
    }

    event.preventDefault();
    event.stopPropagation();

    const url = trigger.dataset.url;
    const title = trigger.dataset.title;

    console.log("OPEN MODAL CLICKED");
    console.log("URL:", url);

    if (!url) {
        console.error("No data-url found.");
        return;
    }

    try {

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(
                `Request failed: ${response.status}`
            );
        }

        const html = await response.text();

        const modalElement =
            document.getElementById("appModal");

        const modalBody =
            document.getElementById("appModalBody");

        const modalTitle =
            document.getElementById("appModalLabel");

        if (!modalElement || !modalBody) {
            console.error(
                "appModal or appModalBody was not found."
            );
            return;
        }

        if (modalTitle) {
            modalTitle.textContent =
                title || "Information";
        }

        // Replace current modal content
        modalBody.innerHTML = html;

        const modal =
            bootstrap.Modal.getOrCreateInstance(
                modalElement
            );

        modal.show();

    }
    catch (error) {

        console.error(
            "Unable to load modal:",
            error
        );
    }
});


console.log("MEMBERS JS FILE LOADED");

window.approveApplicant = async function (button) {

    const id = button.dataset.id;

    const cancelButton =
        document.getElementById("cancelApproveBtn");

    const tokenInput = document.querySelector(
        '#approveModalForm input[name="__RequestVerificationToken"]'
    );

    if (!tokenInput) {
        console.error("Anti-forgery token not found.");
        return;
    }

    const token = tokenInput.value;

    // Save original button text
    const originalText = button.innerHTML;

    // ============================
    // SHOW LOADING STATE
    // ============================

    button.disabled = true;

    if (cancelButton) {
        cancelButton.disabled = true;
    }

    button.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2"
              role="status"
              aria-hidden="true">
        </span>
        Approving...
    `;

    try {

        const response = await fetch(
            `/YouthMembers/ApproveApplicant/${id}`,
            {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token
                }
            }
        );

        const result = await response.json();

        if (result.success) {

            // Close approval modal
            bootstrap.Modal
                .getOrCreateInstance(
                    document.getElementById("appModal")
                )
                .hide();

            // Show success message
            showAlertDialog(
                result.message,
                "success",
                "Membership Approved"
            );

            document
                .getElementById("appAlertModal")
                .addEventListener(
                    "hidden.bs.modal",
                    () => window.location.reload(),
                    { once: true }
                );
        }
        else {

            showAlertDialog(
                result.message,
                "error",
                "Approval Failed"
            );
        }

    }
    catch (error) {

        console.error(error);

        showAlertDialog(
            "Something went wrong while approving the applicant.",
            "error",
            "Error"
        );

    }
    finally {

        // ============================
        // REMOVE LOADING STATE
        // ============================

        button.disabled = false;
        button.innerHTML = originalText;

        if (cancelButton) {
            cancelButton.disabled = false;
        }
    }
};



window.rejectApplicant = async function (button) {

    const id = button.dataset.id;

    const reasonInput =
        document.getElementById("rejectionReason");

    const errorElement =
        document.getElementById("rejectionReasonError");

    if (!reasonInput) {
        console.error("Membership rejection reason input not found.");
        return;
    }

    const reason = reasonInput.value.trim();

    if (!reason) {

        if (errorElement) {
            errorElement.classList.remove("d-none");
        }

        reasonInput.classList.add("is-invalid");
        reasonInput.focus();

        return;
    }

    if (errorElement) {
        errorElement.classList.add("d-none");
    }

    reasonInput.classList.remove("is-invalid");

    const tokenInput = document.querySelector(
        '#rejectModalForm input[name="__RequestVerificationToken"]'
    );

    if (!tokenInput) {
        console.error("Anti-forgery token not found.");
        return;
    }

    const token = tokenInput.value;

    const originalText = button.innerHTML;

    button.disabled = true;

    button.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2"></span>
        Rejecting...
    `;

    try {

        const body = new URLSearchParams();

        body.append("reason", reason);

        const response = await fetch(
            `/YouthMembers/RejectApplicant/${id}`,
            {
                method: "POST",

                headers: {
                    "RequestVerificationToken": token,
                    "Content-Type":
                        "application/x-www-form-urlencoded"
                },

                body: body
            }
        );

        const result = await response.json();

        if (result.success) {

            bootstrap.Modal
                .getOrCreateInstance(
                    document.getElementById("appModal")
                )
                .hide();

            showAlertDialog(
                result.message,
                "success",
                "Application Rejected"
            );

            document
                .getElementById("appAlertModal")
                ?.addEventListener(
                    "hidden.bs.modal",
                    () => window.location.reload(),
                    { once: true }
                );
        }
        else {

            showAlertDialog(
                result.message,
                "error",
                "Rejection Failed"
            );
        }

    }
    catch (error) {

        console.error(
            "Membership rejection error:",
            error
        );

        showAlertDialog(
            "Something went wrong while rejecting the applicant.",
            "error",
            "Error"
        );
    }
    finally {

        button.disabled = false;
        button.innerHTML = originalText;
    }
};




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

    if (currentPage > totalPages) currentPage = totalPages;

    const start = (currentPage - 1) * rowsPerPage;
    const end = start + rowsPerPage;

    allRows.forEach(row => row.style.display = "none");
    visible.forEach((row, i) => {
        row.style.display = (i >= start && i < end) ? "" : "none";
    });

    const showing = Math.min(end, visible.length);
    const from = visible.length === 0 ? 0 : start + 1;
    document.getElementById("pageInfo").textContent =
        `Showing ${from}–${showing} of ${visible.length} members`;
    document.getElementById("pageNum").textContent =
        `Page ${currentPage} of ${totalPages}`;
    document.getElementById("prevBtn").disabled = currentPage === 1;
    document.getElementById("nextBtn").disabled = currentPage === totalPages;
}

document.getElementById("prevBtn").addEventListener("click", () => {
    currentPage--;
    renderPage();
});

document.getElementById("nextBtn").addEventListener("click", () => {
    currentPage++;
    renderPage();
});

searchInput.addEventListener("input", () => {
    currentPage = 1;
    renderPage();
});

statusButtons.forEach(button => {
    button.addEventListener("click", function () {
        statusButtons.forEach(btn => btn.classList.remove("active-tab"));
        this.classList.add("active-tab");
        currentStatus = this.getAttribute("data-status");
        currentPage = 1;
        renderPage();
    });
});

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
        photoUrlInput.value = '';
    } else {
        uploadWrapper.style.display = 'none';
        urlWrapper.style.display = 'block';
        photoInput.value = '';
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

// ── Create member form submit ──
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
            submitBtn.innerHTML = `<i class="bi bi-check-circle me-1"></i> Email Sent`;

            showAlertDialog(result.message, "success", "Member Created");

            createMemberForm.style.display = "none";
            document.getElementById("appAlertModal").addEventListener(
                "hidden.bs.modal",
                () => window.location.reload(),
                { once: true }
            );
        }
        else {
            showAlertDialog(result.message, "error", "Creation Failed");

            submitBtn.disabled = false;
            submitBtn.innerHTML = "Create";
        }
    }
    catch (error) {
        console.error(error);

        showAlertDialog("Something went wrong while creating the member.", "error", "Error");

        submitBtn.disabled = false;
        submitBtn.innerHTML = "Create";
    }
});

document.addEventListener("DOMContentLoaded", function () {

    // ---- 1. Global modal loader (.open-modal) + Approve/Reject confirm actions ----
    document.addEventListener("click", async function (e) {

        // ==========================
        // APPROVE
        // ==========================
        const approveBtn = e.target.closest("#confirmApproveBtn");

        if (approveBtn) {

            console.log("Approve button clicked");

            const id = approveBtn.dataset.id;

            const tokenInput = document.querySelector(
                '#approveModalForm input[name="__RequestVerificationToken"]'
            );

            if (!tokenInput) {
                console.error("Approve anti-forgery token not found.");
                return;
            }

            const token = tokenInput.value;

            approveBtn.disabled = true;
            const originalText = approveBtn.innerHTML;
            approveBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span> Approving...`;

            try {
                console.log("Approving Member ID:", id);

                const response = await fetch(`/YouthMembers/ApproveApplicant/${id}`, {
                    method: "POST",
                    headers: { "RequestVerificationToken": token }
                });

                console.log("Approve response:", response.status);

                const result = await response.json();

                const appModalEl = document.getElementById("appModal");

                const showResultAlert = () => {
                    showAlertDialog(
                        result.message,
                        result.success ? "success" : "error",
                        result.success ? "Rejected" : "Rejection Failed"
                    );

                    if (result.success) {
                        document.getElementById("appAlertModal").addEventListener(
                            "hidden.bs.modal",
                            () => window.location.reload(),
                            { once: true }
                        );
                    }
                };

                if (appModalEl) {
                    appModalEl.addEventListener("hidden.bs.modal", showResultAlert, { once: true });
                    bootstrap.Modal.getOrCreateInstance(appModalEl).hide();
                } else {
                    showResultAlert();
                }
            }
            catch (error) {
                console.error("Approve error:", error);
                showAlertDialog("Something went wrong while approving the member.", "error", "Error");
            }
            finally {
                approveBtn.disabled = false;
                approveBtn.innerHTML = originalText;
            }

            return;
        }

        // ==========================
        // REJECT
        // ==========================
        const rejectBtn = e.target.closest("#confirmRejectBtn");

        if (rejectBtn) {

            console.log("Reject button clicked");

            const id = rejectBtn.dataset.id;
            const reasonInput = document.getElementById("rejectionReason");
            const errorEl = document.getElementById("rejectionReasonError");

            if (!reasonInput) {
                console.error("Rejection reason input not found.");
                return;
            }

            const reason = reasonInput.value.trim();

            if (!reason) {
                errorEl?.classList.remove("d-none");
                reasonInput.classList.add("is-invalid");
                reasonInput.focus();
                return;
            }

            errorEl?.classList.add("d-none");
            reasonInput.classList.remove("is-invalid");

            const tokenInput = document.querySelector(
                '#rejectModalForm input[name="__RequestVerificationToken"]'
            );

            if (!tokenInput) {
                console.error("Reject anti-forgery token not found.");
                return;
            }

            const token = tokenInput.value;

            rejectBtn.disabled = true;
            const originalText = rejectBtn.innerHTML;
            rejectBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span> Rejecting...`;

            try {
                console.log("Rejecting Member ID:", id);

                const body = new URLSearchParams();
                body.append("reason", reason);

                const response = await fetch(`/YouthMembers/RejectApplicant/${id}`, {
                    method: "POST",
                    headers: {
                        "RequestVerificationToken": token,
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: body
                });

                console.log("Reject response:", response.status);

                const result = await response.json();

                const appModalEl = document.getElementById("appModal");
                if (appModalEl) {
                    bootstrap.Modal.getOrCreateInstance(appModalEl).hide();
                }

                showAlertDialog(
                    result.message,
                    result.success ? "success" : "error",
                    result.success ? "Rejected" : "Rejection Failed"
                );

                if (result.success) {
                    document.getElementById("appAlertModal").addEventListener(
                        "hidden.bs.modal",
                        () => window.location.reload(),
                        { once: true }
                    );
                }
            }
            catch (error) {
                console.error("Reject error:", error);
                showAlertDialog("Something went wrong while rejecting the member.", "error", "Error");
            }
            finally {
                rejectBtn.disabled = false;
                rejectBtn.innerHTML = originalText;
            }

            return;
        }

        // -- Generic modal loader (.open-modal) --
        const clickable = e.target.closest(".open-modal");
        if (!clickable) {
            return;
        }

        const url = clickable.dataset.url;
        const title = clickable.dataset.title;

        if (!url) {
            return;
        }

        try {
            const response = await fetch(url);

            if (!response.ok) {
                throw new Error("Failed to load applicant information.");
            }

            const html = await response.text();

            const modalElement = document.getElementById("appModal");
            const modalTitle = modalElement.querySelector(".modal-title");
            const modalBody = modalElement.querySelector(".modal-body");

            modalTitle.textContent = title || "Applicant Information";
            modalBody.innerHTML = html;

            const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
            modal.show();
        }
        catch (error) {
            console.error(error);
            alert("Unable to load applicant information.");
        }
    });

    // ---- 2. Status filter tabs (works for BOTH Members and Applicants tables) ----
    document.querySelectorAll(".filter-bar").forEach(bar => {
        const tableCard = bar.nextElementSibling;
        const rows = tableCard.querySelectorAll("tbody tr[data-status]");

        bar.querySelectorAll(".filter-tab").forEach(btn => {
            btn.addEventListener("click", function () {
                bar.querySelectorAll(".filter-tab").forEach(b => b.classList.remove("active-tab"));
                this.classList.add("active-tab");

                const status = this.dataset.status;
                rows.forEach(row => {
                    row.style.display = (status === "all" || row.dataset.status === status)
                        ? ""
                        : "none";
                });
            });
        });
    });

    // ---- 3. Search filter ----
    document.querySelectorAll(".search-input").forEach(input => {
        input.addEventListener("input", function () {
            const table = document.getElementById(this.dataset.target);
            const query = this.value.trim().toLowerCase();

            table.querySelectorAll("tbody tr").forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(query) ? "" : "none";
            });
        });
    });

});


