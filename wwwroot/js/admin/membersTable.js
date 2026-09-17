// ============================================================
// MEMBERS PAGE
// ============================================================

console.log("MEMBERS JS FILE LOADED");

document.addEventListener("DOMContentLoaded", function () {

    // ========================================================
    // 1. TOOLTIP
    // ========================================================

    document
        .querySelectorAll('[data-bs-toggle="tooltip"]')
        .forEach(function (element) {

            bootstrap.Tooltip.getOrCreateInstance(element);

        });


    // ========================================================
    // 2. TOP TABS
    // ALL MEMBERS / APPLICANTS
    // ========================================================

    const topTabs =
        document.querySelectorAll(".top-tab");

    const sections =
        document.querySelectorAll(".tab-section");


    topTabs.forEach(function (tab) {

        tab.addEventListener("click", function () {

            const targetSection =
                this.dataset.section;


            topTabs.forEach(function (button) {

                button.classList.remove("active-tab");

            });


            this.classList.add("active-tab");


            sections.forEach(function (section) {

                section.style.display =
                    section.id === targetSection
                        ? "block"
                        : "none";

            });

        });

    });


    // ========================================================
    // 3. PAGINATION + SEARCH + STATUS FILTER
    // ========================================================

    const ROWS_PER_PAGE = 10;


    function setupTable(options) {

        const table =
            document.getElementById(options.tableId);

        if (!table) {
            return;
        }


        const section =
            table.closest(".tab-section");

        if (!section) {
            return;
        }


        const rows = Array.from(
            table.querySelectorAll(
                "tbody tr[data-status]"
            )
        );


        const searchInput =
            section.querySelector(
                `.search-input[data-target="${options.tableId}"]`
            );


        const filterButtons =
            section.querySelectorAll(".filter-tab");


        const pageInfo =
            document.getElementById(
                options.pageInfoId
            );


        const pageNum =
            document.getElementById(
                options.pageNumId
            );


        const prevBtn =
            document.getElementById(
                options.prevBtnId
            );


        const nextBtn =
            document.getElementById(
                options.nextBtnId
            );


        let currentPage = 1;
        let currentStatus = "all";


        // ----------------------------------------------------
        // GET FILTERED ROWS
        // ----------------------------------------------------

        function getFilteredRows() {

            const query =
                searchInput
                    ? searchInput.value
                        .trim()
                        .toLowerCase()
                    : "";


            return rows.filter(function (row) {

                const rowText =
                    row.textContent
                        .toLowerCase();


                const rowStatus =
                    (row.dataset.status || "")
                        .toLowerCase();


                const matchesSearch =
                    rowText.includes(query);


                const matchesStatus =
                    currentStatus === "all" ||
                    rowStatus === currentStatus;


                return (
                    matchesSearch &&
                    matchesStatus
                );

            });

        }


        // ----------------------------------------------------
        // RENDER PAGE
        // ----------------------------------------------------

        function renderPage() {

            const filteredRows =
                getFilteredRows();


            const totalPages =
                Math.max(
                    1,
                    Math.ceil(
                        filteredRows.length /
                        ROWS_PER_PAGE
                    )
                );


            if (currentPage > totalPages) {

                currentPage = totalPages;

            }


            if (currentPage < 1) {

                currentPage = 1;

            }


            const start =
                (currentPage - 1) *
                ROWS_PER_PAGE;


            const end =
                start + ROWS_PER_PAGE;


            // Hide every normal row first
            rows.forEach(function (row) {

                row.style.display = "none";

            });


            // Show only current page
            filteredRows
                .slice(start, end)
                .forEach(function (row) {

                    row.style.display = "";

                });


            const firstItem =
                filteredRows.length === 0
                    ? 0
                    : start + 1;


            const lastItem =
                Math.min(
                    end,
                    filteredRows.length
                );


            // Pagination may not exist yet.
            // This prevents JavaScript errors.
            if (pageInfo) {

                pageInfo.textContent =
                    `Showing ${firstItem}-${lastItem} of ${filteredRows.length} ${options.label}`;

            }


            if (pageNum) {

                pageNum.textContent =
                    `Page ${currentPage} of ${totalPages}`;

            }


            if (prevBtn) {

                prevBtn.disabled =
                    currentPage <= 1;

            }


            if (nextBtn) {

                nextBtn.disabled =
                    currentPage >= totalPages;

            }

        }


        // ----------------------------------------------------
        // SEARCH
        // ----------------------------------------------------

        if (searchInput) {

            searchInput.addEventListener(
                "input",
                function () {

                    currentPage = 1;

                    renderPage();

                }
            );

        }


        // ----------------------------------------------------
        // STATUS FILTER
        // ----------------------------------------------------

        filterButtons.forEach(
            function (button) {

                button.addEventListener(
                    "click",
                    function () {

                        filterButtons.forEach(
                            function (item) {

                                item.classList.remove(
                                    "active-tab"
                                );

                            }
                        );


                        this.classList.add(
                            "active-tab"
                        );


                        currentStatus =
                            (
                                this.dataset.status ||
                                "all"
                            ).toLowerCase();


                        currentPage = 1;

                        renderPage();

                    }
                );

            }
        );


        // ----------------------------------------------------
        // PREVIOUS
        // ----------------------------------------------------

        if (prevBtn) {

            prevBtn.addEventListener(
                "click",
                function () {

                    if (currentPage > 1) {

                        currentPage--;

                        renderPage();

                    }

                }
            );

        }


        // ----------------------------------------------------
        // NEXT
        // ----------------------------------------------------

        if (nextBtn) {

            nextBtn.addEventListener(
                "click",
                function () {

                    const filteredRows =
                        getFilteredRows();


                    const totalPages =
                        Math.max(
                            1,
                            Math.ceil(
                                filteredRows.length /
                                ROWS_PER_PAGE
                            )
                        );


                    if (currentPage < totalPages) {

                        currentPage++;

                        renderPage();

                    }

                }
            );

        }


        // Initial display
        renderPage();

    }


    // ========================================================
    // MEMBERS TABLE
    // ========================================================

    setupTable({

        tableId: "membersTable",

        pageInfoId: "membersPageInfo",

        pageNumId: "membersPageNum",

        prevBtnId: "membersPrevBtn",

        nextBtnId: "membersNextBtn",

        label: "members"

    });


    // ========================================================
    // APPLICANTS TABLE
    // ========================================================

    setupTable({

        tableId: "applicantsTable",

        pageInfoId: "applicantsPageInfo",

        pageNumId: "applicantsPageNum",

        prevBtnId: "applicantsPrevBtn",

        nextBtnId: "applicantsNextBtn",

        label: "applicants"

    });


    // ========================================================
    // 4. EXPORT MEMBERS PDF
    // MEMBERS ONLY — NEVER APPLICANTS
    // ========================================================

    const exportBtn =
        document.getElementById(
            "exportMembersPdfBtn"
        );


    if (exportBtn) {

        exportBtn.addEventListener(
            "click",
            function (event) {

                event.preventDefault();
                event.stopPropagation();


                console.log(
                    "EXPORT MEMBERS CLICKED"
                );


                // --------------------------------------------
                // CHECK JSPDF
                // --------------------------------------------

                if (
                    typeof window.jspdf ===
                    "undefined"
                ) {

                    console.error(
                        "jsPDF library is not loaded."
                    );

                    alert(
                        "PDF library could not be loaded. Please refresh the page and try again."
                    );

                    return;

                }


                const jsPDF =
                    window.jspdf.jsPDF;


                if (!jsPDF) {

                    console.error(
                        "window.jspdf.jsPDF not found."
                    );

                    return;

                }


                // --------------------------------------------
                // CREATE PDF
                // --------------------------------------------

                const doc =
                    new jsPDF(
                        "landscape",
                        "mm",
                        "a4"
                    );


                // --------------------------------------------
                // CHECK AUTOTABLE
                // --------------------------------------------

                if (
                    typeof doc.autoTable !==
                    "function"
                ) {

                    console.error(
                        "jsPDF AutoTable is not loaded."
                    );

                    alert(
                        "PDF table library could not be loaded. Please refresh the page and try again."
                    );

                    return;

                }


                // --------------------------------------------
                // TITLE
                // --------------------------------------------

                doc.setTextColor(
                    0,
                    0,
                    0
                );


                doc.setFontSize(16);


                doc.text(
                    "Members Report",
                    14,
                    16
                );


                // --------------------------------------------
                // GENERATED DATE
                // --------------------------------------------

                const generatedDate =
                    new Date()
                        .toLocaleString();


                doc.setFontSize(9);


                doc.setTextColor(
                    100,
                    116,
                    139
                );


                doc.text(
                    "Generated: " +
                    generatedDate,
                    14,
                    22
                );


                // --------------------------------------------
                // GET MEMBERS
                // IMPORTANT:
                // We export ALL member rows.
                // Pagination does not affect the PDF.
                // --------------------------------------------

                const memberRows =
                    document.querySelectorAll(
                        "#membersTable tbody tr[data-status]"
                    );


                const pdfRows = [];


                memberRows.forEach(
                    function (row) {

                        const cells =
                            row.querySelectorAll(
                                "td"
                            );


                        // Members table has 9 cells.
                        // We export first 8 and ignore Action.
                        if (cells.length < 8) {

                            return;

                        }


                        pdfRows.push([

                            cells[0]
                                .textContent
                                .trim(),

                            cells[1]
                                .textContent
                                .trim(),

                            cells[2]
                                .textContent
                                .trim(),

                            cells[3]
                                .textContent
                                .trim(),

                            cells[4]
                                .textContent
                                .trim(),

                            cells[5]
                                .textContent
                                .trim(),

                            cells[6]
                                .textContent
                                .trim(),

                            cells[7]
                                .textContent
                                .trim()

                        ]);

                    }
                );


                console.log(
                    "Members being exported:",
                    pdfRows.length
                );


                if (pdfRows.length === 0) {

                    alert(
                        "There are no members to export."
                    );

                    return;

                }


                // --------------------------------------------
                // TABLE
                // --------------------------------------------

                doc.autoTable({

                    startY: 28,


                    head: [[

                        "ID",

                        "First Name",

                        "Middle Name",

                        "Last Name",

                        "Group",

                        "Role",

                        "Email",

                        "Status"

                    ]],


                    body: pdfRows,


                    theme: "striped",


                    styles: {

                        fontSize: 8,

                        cellPadding: 3,

                        textColor: [
                            30,
                            41,
                            59
                        ]

                    },


                    headStyles: {

                        fillColor: [
                            37,
                            99,
                            235
                        ],

                        textColor: [
                            255,
                            255,
                            255
                        ],

                        fontStyle:
                            "bold"

                    },


                    alternateRowStyles: {

                        fillColor: [
                            248,
                            250,
                            252
                        ]

                    },


                    margin: {

                        left: 14,

                        right: 14

                    }

                });


                // --------------------------------------------
                // SAVE PDF
                // --------------------------------------------

                const today =
                    new Date()
                        .toISOString()
                        .slice(
                            0,
                            10
                        );


                doc.save(
                    `members-report-${today}.pdf`
                );

            }
        );

    }
    else {

        console.warn(
            "exportMembersPdfBtn was not found."
        );

    }


    // ========================================================
    // 5. ARCHIVE MEMBER BUTTON
    // ========================================================

    document.addEventListener(
        "click",
        async function (event) {

            const archiveButton =
                event.target.closest(
                    ".archive-member-btn"
                );


            if (!archiveButton) {

                return;

            }


            event.preventDefault();
            event.stopPropagation();


            const url =
                archiveButton.dataset.url;


            const title =
                archiveButton.dataset.title ||
                "Archive Member";


            if (!url) {

                return;

            }


            try {

                const response =
                    await fetch(url);


                if (!response.ok) {

                    throw new Error(
                        "Unable to load archive modal."
                    );

                }


                const html =
                    await response.text();


                const modalElement =
                    document.getElementById(
                        "appModal"
                    );


                if (!modalElement) {

                    return;

                }


                const modalTitle =
                    modalElement.querySelector(
                        ".modal-title"
                    );


                const modalBody =
                    modalElement.querySelector(
                        ".modal-body"
                    );


                if (modalTitle) {

                    modalTitle.textContent =
                        title;

                }


                if (modalBody) {

                    modalBody.innerHTML =
                        html;

                }


                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .show();

            }
            catch (error) {

                console.error(
                    "Unable to open archive modal:",
                    error
                );

            }

        }
    );


    // ========================================================
    // 6. GENERIC MEMBER / APPLICANT MODAL
    // ========================================================

    document.addEventListener(
        "click",
        async function (event) {

            const trigger =
                event.target.closest(
                    ".open-modal"
                );


            if (!trigger) {

                return;

            }


            // Archive has its own handler
            if (
                event.target.closest(
                    ".archive-member-btn"
                )
            ) {

                return;

            }


            event.preventDefault();


            const url =
                trigger.dataset.url;


            const title =
                trigger.dataset.title ||
                "Information";


            if (!url) {

                return;

            }


            try {

                const response =
                    await fetch(url);


                if (!response.ok) {

                    throw new Error(
                        `Request failed: ${response.status}`
                    );

                }


                const html =
                    await response.text();


                const modalElement =
                    document.getElementById(
                        "appModal"
                    );


                if (!modalElement) {

                    console.error(
                        "appModal not found."
                    );

                    return;

                }


                const modalTitle =
                    modalElement.querySelector(
                        ".modal-title"
                    );


                const modalBody =
                    modalElement.querySelector(
                        ".modal-body"
                    );


                if (modalTitle) {

                    modalTitle.textContent =
                        title;

                }


                if (modalBody) {

                    modalBody.innerHTML =
                        html;

                }


                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .show();

            }
            catch (error) {

                console.error(
                    "Unable to open modal:",
                    error
                );

            }

        }
    );


    // ========================================================
    // 7. OPEN APPLICANT FROM EMAIL / REDIRECT
    // ========================================================

    // This value can be supplied from Razor if needed.
    // Your Razor page already handles this separately,
    // so nothing else is required here.

});


// ============================================================
// APPROVE APPLICANT
// Called from dynamically loaded modal
// ============================================================

window.approveApplicant =
    async function (button) {

        const id =
            button.dataset.id;


        const tokenInput =
            document.querySelector(
                '#approveModalForm input[name="__RequestVerificationToken"]'
            );


        if (!tokenInput) {

            console.error(
                "Approve anti-forgery token not found."
            );

            return;

        }


        const cancelButton =
            document.getElementById(
                "cancelApproveBtn"
            );


        const originalText =
            button.innerHTML;


        button.disabled = true;


        if (cancelButton) {

            cancelButton.disabled = true;

        }


        button.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2"></span>
            Approving...
        `;


        try {

            const response =
                await fetch(
                    `/YouthMembers/ApproveApplicant/${id}`,
                    {
                        method: "POST",

                        headers: {

                            "RequestVerificationToken":
                                tokenInput.value

                        }
                    }
                );


            const result =
                await response.json();


            const modalElement =
                document.getElementById(
                    "appModal"
                );


            if (modalElement) {

                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .hide();

            }


            showAlertDialog(

                result.message,

                result.success
                    ? "success"
                    : "error",

                result.success
                    ? "Membership Approved"
                    : "Approval Failed"

            );


            if (result.success) {

                document
                    .getElementById(
                        "appAlertModal"
                    )
                    ?.addEventListener(
                        "hidden.bs.modal",
                        function () {

                            window.location.reload();

                        },
                        {
                            once: true
                        }
                    );

            }

        }
        catch (error) {

            console.error(
                "Approval error:",
                error
            );


            showAlertDialog(

                "Something went wrong while approving the applicant.",

                "error",

                "Error"

            );

        }
        finally {

            button.disabled = false;

            button.innerHTML =
                originalText;


            if (cancelButton) {

                cancelButton.disabled =
                    false;

            }

        }

    };


// ============================================================
// REJECT APPLICANT
// Called from dynamically loaded modal
// ============================================================

window.rejectApplicant =
    async function (button) {

        const id =
            button.dataset.id;


        const reasonInput =
            document.getElementById(
                "rejectionReason"
            );


        const errorElement =
            document.getElementById(
                "rejectionReasonError"
            );


        if (!reasonInput) {

            console.error(
                "Rejection reason input not found."
            );

            return;

        }


        const reason =
            reasonInput.value.trim();


        if (!reason) {

            errorElement
                ?.classList
                .remove("d-none");


            reasonInput
                .classList
                .add("is-invalid");


            reasonInput.focus();

            return;

        }


        errorElement
            ?.classList
            .add("d-none");


        reasonInput
            .classList
            .remove("is-invalid");


        const tokenInput =
            document.querySelector(
                '#rejectModalForm input[name="__RequestVerificationToken"]'
            );


        if (!tokenInput) {

            console.error(
                "Reject anti-forgery token not found."
            );

            return;

        }


        const originalText =
            button.innerHTML;


        button.disabled = true;


        button.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2"></span>
            Rejecting...
        `;


        try {

            const body =
                new URLSearchParams();


            body.append(
                "reason",
                reason
            );


            const response =
                await fetch(
                    `/YouthMembers/RejectApplicant/${id}`,
                    {
                        method: "POST",

                        headers: {

                            "RequestVerificationToken":
                                tokenInput.value,

                            "Content-Type":
                                "application/x-www-form-urlencoded"

                        },

                        body: body

                    }
                );


            const result =
                await response.json();


            const modalElement =
                document.getElementById(
                    "appModal"
                );


            if (modalElement) {

                bootstrap.Modal
                    .getOrCreateInstance(
                        modalElement
                    )
                    .hide();

            }


            showAlertDialog(

                result.message,

                result.success
                    ? "success"
                    : "error",

                result.success
                    ? "Application Rejected"
                    : "Rejection Failed"

            );


            if (result.success) {

                document
                    .getElementById(
                        "appAlertModal"
                    )
                    ?.addEventListener(
                        "hidden.bs.modal",
                        function () {

                            window.location.reload();

                        },
                        {
                            once: true
                        }
                    );

            }

        }
        catch (error) {

            console.error(
                "Rejection error:",
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

            button.innerHTML =
                originalText;

        }

    };


// ============================================================
// PROFILE PHOTO HELPERS
// ============================================================

function togglePhotoMode(mode) {

    const uploadWrapper =
        document.getElementById(
            "uploadInputWrapper"
        );

    const urlWrapper =
        document.getElementById(
            "urlInputWrapper"
        );

    const photoInput =
        document.getElementById(
            "photoInput"
        );

    const photoUrlInput =
        document.getElementById(
            "photoUrlInput"
        );

    const preview =
        document.getElementById(
            "photoPreview"
        );

    const placeholder =
        document.getElementById(
            "placeholderIcon"
        );


    if (
        !uploadWrapper ||
        !urlWrapper ||
        !photoInput ||
        !photoUrlInput ||
        !preview ||
        !placeholder
    ) {

        return;

    }


    if (mode === "upload") {

        uploadWrapper.style.display =
            "block";

        urlWrapper.style.display =
            "none";

        photoUrlInput.value = "";

    }
    else {

        uploadWrapper.style.display =
            "none";

        urlWrapper.style.display =
            "block";

        photoInput.value = "";

    }


    preview.src = "";

    preview.style.display =
        "none";

    placeholder.style.display =
        "block";

}


function previewProfilePhoto(input) {

    const preview =
        document.getElementById(
            "photoPreview"
        );

    const placeholder =
        document.getElementById(
            "placeholderIcon"
        );


    if (!preview || !placeholder) {

        return;

    }


    if (
        input.files &&
        input.files[0]
    ) {

        const blobUrl =
            URL.createObjectURL(
                input.files[0]
            );


        preview.src =
            blobUrl;


        preview.style.display =
            "block";


        placeholder.style.display =
            "none";


        preview.onload =
            function () {

                URL.revokeObjectURL(
                    blobUrl
                );

            };

    }
    else {

        preview.src = "";

        preview.style.display =
            "none";

        placeholder.style.display =
            "block";

    }

}


function previewFromUrl(url) {

    const preview =
        document.getElementById(
            "photoPreview"
        );

    const placeholder =
        document.getElementById(
            "placeholderIcon"
        );


    if (!preview || !placeholder) {

        return;

    }


    if (url) {

        preview.src = url;

        preview.style.display =
            "block";

        placeholder.style.display =
            "none";

    }
    else {

        preview.src = "";

        preview.style.display =
            "none";

        placeholder.style.display =
            "block";

    }

}


// ============================================================
// CREATE MEMBER FORM
// ============================================================

document.addEventListener(
    "submit",
    async function (event) {

        if (
            event.target.id !==
            "createMemberForm"
        ) {

            return;

        }


        event.preventDefault();


        const form =
            event.target;


        const submitButton =
            form.querySelector(
                "#submitBtn"
            );


        if (!submitButton) {

            return;

        }


        const originalText =
            submitButton.innerHTML;


        submitButton.disabled =
            true;


        submitButton.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2"></span>
            Sending verification email...
        `;


        try {

            const formData =
                new FormData(form);


            const response =
                await fetch(
                    form.action,
                    {
                        method: "POST",
                        body: formData
                    }
                );


            const result =
                await response.json();


            if (result.success) {

                submitButton.innerHTML = `
                    <i class="bi bi-check-circle me-1"></i>
                    Email Sent
                `;


                showAlertDialog(
                    result.message,
                    "success",
                    "Member Created"
                );


                form.style.display =
                    "none";


                document
                    .getElementById(
                        "appAlertModal"
                    )
                    ?.addEventListener(
                        "hidden.bs.modal",
                        function () {

                            window.location.reload();

                        },
                        {
                            once: true
                        }
                    );

            }
            else {

                showAlertDialog(
                    result.message,
                    "error",
                    "Creation Failed"
                );


                submitButton.disabled =
                    false;


                submitButton.innerHTML =
                    originalText;

            }

        }
        catch (error) {

            console.error(
                "Create member error:",
                error
            );


            showAlertDialog(
                "Something went wrong while creating the member.",
                "error",
                "Error"
            );


            submitButton.disabled =
                false;


            submitButton.innerHTML =
                originalText;

        }

    }
);