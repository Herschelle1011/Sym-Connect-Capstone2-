// ═══════════════ ENSURE EMAIL VERIFICATION MODAL EXISTS ═══════════════
// Instead of depending on every layout file remembering to include this
// modal's markup, we build it once in JS and inject it into <body> the
// first time it's needed. This guarantees it always exists, regardless
// of which layout the current page happens to use.

function ensureEmailVerificationModal() {

    let modalEl = document.getElementById("emailVerificationModal");

    if (modalEl) {
        return modalEl; // already exists — nothing to do
    }

    const wrapper = document.createElement("div");

    wrapper.innerHTML = `
        <div class="modal fade" id="emailVerificationModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Verify Your New Email</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted small">
                            We sent a 6-digit code to <strong id="verifyEmailTarget"></strong>.
                            Enter it below to confirm this email change.
                        </p>

                        <input type="text"
                               id="verificationCodeInput"
                               class="form-control"
                               maxlength="6"
                               inputmode="numeric"
                               placeholder="Enter 6-digit code" />

                        <div id="verificationCodeError" class="text-danger small mt-2 d-none"></div>

                        <button type="button" id="resendCodeBtn" class="btn btn-link btn-sm px-0 mt-2">
                            Resend code
                        </button>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" id="confirmVerificationBtn" class="btn btn-primary px-4">
                            Verify &amp; Save
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    modalEl = wrapper.firstElementChild;
    document.body.appendChild(modalEl);

    // z-index fix so it stacks correctly above #appModal, which is
    // already open underneath it when this gets triggered.
    modalEl.style.zIndex = "1060";

    modalEl.addEventListener("hidden.bs.modal", function () {
        const appModal = document.getElementById("appModal");
        if (appModal && appModal.classList.contains("show")) {
            document.body.classList.add("modal-open");
        }
    });

    return modalEl;
}


document.addEventListener("submit", async function (e) {

    if (!e.target.classList.contains("modal-ajax-form")) {
        return;
    }

    e.preventDefault();

    const form = e.target;

    if (form.dataset.submitting === "true") {
        return;
    }

    form.dataset.submitting = "true";

    const submitBtn = form.querySelector('button[type="submit"]');

    let originalButtonHtml = "";

    if (submitBtn) {

        // Save original button content
        originalButtonHtml = submitBtn.innerHTML;

        // Disable button
        submitBtn.disabled = true;

        // Show loading inside button
        submitBtn.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2"
                  role="status"
                  aria-hidden="true"></span>
            Loading...
        `;
    }

    try {

        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form)
        });

        const contentType = response.headers.get("content-type");

        // Controller returned JSON
        if (
            contentType &&
            contentType.includes("application/json")
        ) {

            const result = await response.json();

            console.log("Modal form result:", result);

            if (result.success) {
                const modalElement = form.closest(".modal");
                const modal = modalElement ? bootstrap.Modal.getInstance(modalElement) : null;

                if (modal) {
                    modal.hide();
                }

                showAlertDialog(result.message || "Saved successfully.", "success", "Success");
                setTimeout(() => location.reload(), 1200);
            } else {
                showAlertDialog(result.message || "Unable to save.", "error", "Failed");
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalButtonHtml;
                }
            }
        }

        // Controller returned partial view
        else {

            const html = await response.text();

            document.getElementById(
                "appModalBody"
            ).innerHTML = html;
        }

    } catch (error) {

        console.error("Modal form error:", error);

        showAlertDialog(
            "Something went wrong. Please try again.",
            "error",
            "Error"
        );

        // Restore button
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalButtonHtml;
        }

    } finally {

        form.dataset.submitting = "false";
    }
});

function setAppModalSize(size) {
    const dialog = document.querySelector('#appModal .app-modal-dialog');
    if (!dialog) return;
    dialog.classList.remove('modal-size-lg'); // always reset first
    if (size === 'lg') dialog.classList.add('modal-size-lg');
}


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


// site.js — add once, works for every Edit panel injected via AJAX

document.addEventListener("change", function (e) {
    if (e.target.id === "editIsAllDayToggle") {
        toggleEditTimeInputs(e.target);
    }
    if (e.target.id === "editStartDateInput") {
        syncEditEndDate();
    }
});

function toggleEditTimeInputs(toggleEl) {
    toggleEl = toggleEl || document.getElementById("editIsAllDayToggle");
    if (!toggleEl) return;

    const startGroup = document.getElementById("editStartTimeGroup");
    const endGroup = document.getElementById("editEndTimeGroup");
    if (!startGroup || !endGroup) return;

    const hide = toggleEl.checked;
    startGroup.style.display = hide ? "none" : "block";
    endGroup.style.display = hide ? "none" : "block";
}

function syncEditEndDate() {
    const startInput = document.getElementById("editStartDateInput");
    const endInput = document.getElementById("editEndDateInput");
    if (!startInput || !endInput || !startInput.value) return;

    endInput.min = startInput.value;
    if (!endInput.value || endInput.value < startInput.value) {
        endInput.value = startInput.value;
    }
}

// Run once whenever the edit offcanvas is actually shown, so initial state is correct
document.addEventListener("shown.bs.offcanvas", function (e) {
    if (e.target.id === "editEventPanel") {
        toggleEditTimeInputs();
        syncEditEndDate();
    }
});


// ── Modal opener (global function for inline onclick calls) ──
function openModal(title, url, size, options = {}) {
    document.getElementById("appModalLabel").innerText = title;

    const iconWrap = document.getElementById("appModalIcon");
    const iconEl = iconWrap.querySelector("i");
    if (options.icon) {
        iconEl.className = `bi ${options.icon}`;
        iconWrap.classList.remove("d-none");
    } else {
        iconWrap.classList.add("d-none");
    }

    const subtitleEl = document.getElementById("appModalSubtitle");
    if (options.subtitle) {
        subtitleEl.innerText = options.subtitle;
        subtitleEl.classList.remove("d-none");
    } else {
        subtitleEl.classList.add("d-none");
        subtitleEl.innerText = "";
    }


    const dialog = document.querySelector("#appModal .modal-dialog");
    dialog.classList.remove("modal-lg", "modal-xl");
    if (size) dialog.classList.add(`modal-${size}`);

    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
        .then(response => response.text())
        .then(html => {
            document.getElementById("appModalBody").innerHTML = html;

            const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById("appModal"));
            modal.show();

            if (window.jQuery && $.validator) {
                $.validator.unobtrusive.parse("#appModalBody");
            }

            initPsgcAddress();
        });
}

// =====================================================
// GENERIC MODAL OPENER
// Works for Admin, Leader, Member, and AJAX content
// =====================================================
document.addEventListener("click", function (e) {

    const button = e.target.closest(".open-modal");

    if (!button) {
        return;
    }

    e.preventDefault();

    const url = button.getAttribute("data-url");
    const title = button.getAttribute("data-title");
    const size = button.getAttribute("data-modal-size");
    const icon = button.getAttribute("data-icon");
    const subtitle = button.getAttribute("data-subtitle");

    if (!url) {
        console.error("open-modal button has no data-url.");
        return;
    }

    openModal(
        title,
        url,
        size,
        {
            icon: icon,
            subtitle: subtitle
        }
    );
});



// ── Facility schedule: month navigation inside the modal ──
document.addEventListener("click", function (e) {
    const navBtn = e.target.closest(".schedule-nav");
    if (!navBtn) return;

    const scopeEl = navBtn.closest(".sched-modal-body");
    if (!scopeEl) return;

    const facilityId = scopeEl.dataset.facilityId;
    const year = navBtn.dataset.year;
    const month = navBtn.dataset.month;

    fetch(`/Events/Schedule/${facilityId}?year=${year}&month=${month}`)
        .then(res => {
            if (!res.ok) {
                throw new Error(`Schedule request failed: ${res.status}`);
            }
            return res.text();
        })
        .then(html => {
            document.getElementById("appModalBody").innerHTML = html;
        })
        .catch(err => {
            console.error(err);
            showAlertDialog('Unable to load the facility schedule. Please try again.', 'error');
        });
});



document.addEventListener("submit", async function (e) {
    //EXISTING GLOBAL MODAL SUBMIT FORMS
    const modalFormIds = [ "removeMemberForm", "transferMemberForm"];
    if (!modalFormIds.includes(e.target.id)) return;

    e.preventDefault();
    const form = e.target;

    const response = await fetch(form.action, {
        method: "POST",
        body: new FormData(form)
    });

    const contentType = response.headers.get("content-type");

    if (contentType && contentType.includes("application/json")) {
        const result = await response.json();

        if (result.success) {
            var modalEl = document.getElementById('appModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();

            showToast(result.message, true);
            setTimeout(() => location.reload(), 900);
        } else {
            showToast(result.message, false);
        }
    } else {
        document.getElementById("appModalBody").innerHTML = await response.text();
    }
});


function initPsgcAddress() {

    const provinceSelect = document.getElementById("provinceSelect");
    const citySelect = document.getElementById("citySelect");
    const barangaySelect = document.getElementById("barangaySelect");

    const provinceName = document.getElementById("provinceName");
    const cityName = document.getElementById("cityName");
    const barangayName = document.getElementById("barangayName");

    // This modal does not contain address fields.
    if (!provinceSelect || !citySelect || !barangaySelect) {
        return;
    }

    // Prevent initializing the same form more than once.
    if (provinceSelect.dataset.loaded === "true") {
        return;
    }

    provinceSelect.dataset.loaded = "true";


    // ==========================================
    // EXISTING VALUES FROM DATABASE
    // ==========================================

    const savedProvince =
        document.getElementById("savedProvince")?.value || "";

    const savedCity =
        document.getElementById("savedCity")?.value || "";

    const savedBarangay =
        document.getElementById("savedBarangay")?.value || "";


    console.log("EDIT ADDRESS VALUES:", {
        savedProvince,
        savedCity,
        savedBarangay
    });


    // ==========================================
    // NORMALIZE NAMES FOR COMPARISON
    // ==========================================

    function normalize(name) {

        return (name || "")
            .trim()
            .toLowerCase()
            .replace(/^city of\s+/i, "")
            .replace(/\s+city$/i, "")
            .replace(/\s+/g, " ");
    }


    // ==========================================
    // LOAD BARANGAYS
    // ==========================================

    async function loadBarangaysFor(
        cityCode,
        presetBarangay = ""
    ) {
        barangaySelect.innerHTML =
            '<option value="">Loading barangays...</option>';

        barangaySelect.disabled = true;

        if (!cityCode) {
            barangaySelect.innerHTML =
                '<option value="">-- Select Barangay --</option>';
            return;
        }

        try {

            console.log("Loading barangays for city code:", cityCode);

            // Get all barangays
            const response = await fetch(
                "https://psgc.gitlab.io/api/barangays/"
            );

            if (!response.ok) {
                throw new Error(
                    `Barangay request failed: ${response.status}`
                );
            }

            const allBarangays = await response.json();

            // Find barangays belonging to the selected
            // city / municipality.
            const barangays = allBarangays.filter(function (barangay) {

                return barangay.cityCode === cityCode ||
                    barangay.municipalityCode === cityCode;
            });

            console.log(
                "Barangays found:",
                barangays.length,
                barangays
            );

            barangaySelect.innerHTML =
                '<option value="">-- Select Barangay --</option>';


            barangays
                .sort(function (a, b) {
                    return a.name.localeCompare(b.name);
                })
                .forEach(function (barangay) {

                    const option =
                        document.createElement("option");

                    option.value = barangay.code;
                    option.textContent = barangay.name;

                    barangaySelect.appendChild(option);
                });


            barangaySelect.disabled = false;


            // Restore saved barangay when editing.
            if (presetBarangay) {

                console.log(
                    "Looking for saved barangay:",
                    presetBarangay
                );

                const match =
                    Array.from(barangaySelect.options)
                        .find(function (option) {

                            return normalize(option.textContent) ===
                                normalize(presetBarangay);
                        });


                if (match) {

                    barangaySelect.value = match.value;

                    if (barangayName) {
                        barangayName.value =
                            match.textContent.trim();
                    }

                } else {

                    console.warn(
                        "Saved barangay was not found:",
                        presetBarangay
                    );
                }
            }

        } catch (error) {

            console.error(
                "Failed to load barangays:",
                error
            );

            barangaySelect.innerHTML =
                '<option value="">Unable to load barangays</option>';

            barangaySelect.disabled = true;
        }
    }


    // ==========================================
    // LOAD CITIES
    // ==========================================

    async function loadCitiesFor(
        provinceCode,
        presetCity = "",
        presetBarangay = ""
    ) {
        citySelect.innerHTML =
            '<option value="">-- Select City/Municipality --</option>';

        barangaySelect.innerHTML =
            '<option value="">-- Select City First --</option>';

        citySelect.disabled = true;
        barangaySelect.disabled = true;

        if (!provinceCode) {
            return;
        }

        try {

            const response = await fetch(
                `https://psgc.gitlab.io/api/provinces/${provinceCode}/cities-municipalities/`
            );

            if (!response.ok) {
                throw new Error(
                    `Unable to load cities: ${response.status}`
                );
            }

            const cities = await response.json();

            cities
                .sort((a, b) =>
                    a.name.localeCompare(b.name)
                )
                .forEach(city => {

                    const option =
                        document.createElement("option");

                    option.value = city.code;
                    option.textContent = city.name;

                    citySelect.appendChild(option);
                });

            citySelect.disabled = false;


            // =====================================
            // RESTORE EXISTING CITY
            // =====================================

            if (presetCity) {

                const match =
                    Array.from(citySelect.options)
                        .find(option =>
                            normalize(option.textContent) ===
                            normalize(presetCity)
                        );

                if (match) {

                    // Select existing city
                    citySelect.value = match.value;

                    // Store city NAME for ASP.NET
                    if (cityName) {
                        cityName.value =
                            match.textContent.trim();
                    }

                    console.log(
                        "Selected city:",
                        match.textContent,
                        "Code:",
                        match.value
                    );


                    // =====================================
                    // IMPORTANT:
                    // Automatically load barangays
                    // =====================================

                    await loadBarangaysFor(
                        match.value,
                        presetBarangay
                    );

                    return;
                }

                console.warn(
                    "Saved city not found:",
                    presetCity
                );
            }


            // No existing city
            barangaySelect.innerHTML =
                '<option value="">-- Select City First --</option>';

            barangaySelect.disabled = true;

        }
        catch (error) {

            console.error(
                "Failed to load cities:",
                error
            );

            citySelect.innerHTML =
                '<option value="">Unable to load cities</option>';

            citySelect.disabled = true;
        }
    }


    // ==========================================
    // LOAD PROVINCES
    // ==========================================

    async function loadProvinces() {

        provinceSelect.innerHTML =
            '<option value="">-- Select Province --</option>';

        provinceSelect.disabled = true;


        try {

            const response = await fetch(
                "https://psgc.gitlab.io/api/provinces/"
            );

            if (!response.ok) {
                throw new Error(
                    "Unable to load provinces."
                );
            }

            const provinces =
                await response.json();


            provinces
                .sort((a, b) =>
                    a.name.localeCompare(b.name)
                )
                .forEach(province => {

                    const option =
                        document.createElement("option");

                    option.value = province.code;
                    option.textContent = province.name;

                    provinceSelect.appendChild(option);
                });


            provinceSelect.disabled = false;


            // No saved province.
            if (!savedProvince) {
                return;
            }


            const match =
                Array.from(provinceSelect.options)
                    .find(option =>
                        normalize(option.textContent) ===
                        normalize(savedProvince)
                    );


            if (match) {

                provinceSelect.value =
                    match.value;

                if (provinceName) {
                    provinceName.value =
                        match.textContent.trim();
                }


                // Province found.
                // Load saved city + barangay.
                await loadCitiesFor(
                    match.value,
                    savedCity,
                    savedBarangay
                );

            } else {

                console.warn(
                    "Saved province not found:",
                    savedProvince
                );
            }

        }
        catch (error) {

            console.error(
                "Failed to load provinces:",
                error
            );

            provinceSelect.innerHTML =
                '<option value="">Unable to load provinces</option>';

            provinceSelect.disabled = true;
        }
    }


    // ==========================================
    // PROVINCE CHANGED
    // ==========================================

    provinceSelect.addEventListener(
        "change",
        function () {

            if (provinceName) {

                provinceName.value =
                    this.value
                        ? this.options[
                            this.selectedIndex
                        ].textContent.trim()
                        : "";
            }


            if (cityName) {
                cityName.value = "";
            }

            if (barangayName) {
                barangayName.value = "";
            }


            loadCitiesFor(
                this.value,
                "",
                ""
            );
        }
    );


    // ==========================================
    // CITY CHANGED
    // ==========================================

    citySelect.addEventListener(
        "change",
        function () {

            if (cityName) {

                cityName.value =
                    this.value
                        ? this.options[
                            this.selectedIndex
                        ].textContent.trim()
                        : "";
            }


            if (barangayName) {
                barangayName.value = "";
            }


            loadBarangaysFor(
                this.value,
                ""
            );
        }
    );


    // ==========================================
    // BARANGAY CHANGED
    // ==========================================

    barangaySelect.addEventListener(
        "change",
        function () {

            if (!barangayName) {
                return;
            }


            if (!this.value) {

                barangayName.value = "";

                return;
            }


            barangayName.value =
                this.options[
                    this.selectedIndex
                ].textContent.trim();
        }
    );


    // Start loading.
    loadProvinces();
}

// document.addEventListener('submit', function (e) {
//     if (e.target && e.target.id === 'editEventForm') {
//         e.preventDefault();
//         submitEditEventForm(e.target);
//     }
// });



// ═══════════════ EVENT REQUESTS: FILTER TABS ═══════════════
document.addEventListener('click', function (e) {
    const filterBtn = e.target.closest('.filter-tabs [data-filter]');
    if (!filterBtn) return;

    document.querySelectorAll('.filter-tabs button').forEach(btn => {
        btn.classList.remove('active', 'btn-dark');
        btn.classList.add('btn-outline-secondary');
    });
    filterBtn.classList.add('active', 'btn-dark');
    filterBtn.classList.remove('btn-outline-secondary');

    applyEventRequestFilters();
});

// ═══════════════ EVENT REQUESTS: SEARCH ═══════════════
document.addEventListener('input', function (e) {
    if (e.target.id !== 'eventRequestSearch') return;
    applyEventRequestFilters();
});


// ======== ADMIN SIDE event request OPENER ============

document.addEventListener('click', function (e) {
    const row = e.target.closest('.event-request-row');
    if (!row) return;
    if (e.target.closest('.btn-approve-event') || e.target.closest('.btn-reject-event')) return;

    const url = row.dataset.url;
    const title = row.querySelector('.fw-semibold').textContent;
    openModal(title, url);
});

// ═══════════════ LEADER: EDIT REQUEST FROM DETAILS MODAL ═══════════════
let editEventRequestLoading = false;

document.addEventListener('click', async function (e) {

    const btn = e.target.closest('.edit-event-from-modal-btn');

    if (!btn) return;

    // Prevent another request while one is already running
    if (editEventRequestLoading) return;

    editEventRequestLoading = true;
    btn.disabled = true;

    const originalHtml = btn.innerHTML;

    btn.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2"></span>
        Loading...
    `;

    try {

        const eventId = btn.dataset.eventId;

        const response = await fetch(
            `/Leader/Events/GetEditForm?eventId=${eventId}`
        );

        if (!response.ok) {
            throw new Error("Unable to load edit form.");
        }

        const html = await response.text();


        // Remove old edit panel
        const existingPanelEl =
            document.getElementById('editEventPanel');

        if (existingPanelEl) {

            const existingInstance =
                bootstrap.Offcanvas.getInstance(existingPanelEl);

            if (existingInstance) {
                existingInstance.dispose();
            }

            existingPanelEl.remove();
        }


        // Add the new edit panel
        document.body.insertAdjacentHTML(
            'beforeend',
            html
        );


        // Reinitialize validation
        const editForm =
            document.getElementById("editEventForm");

        if (
            editForm &&
            window.jQuery &&
            $.validator &&
            $.validator.unobtrusive
        ) {
            $.validator.unobtrusive.parse(editForm);
        }


        // Close details modal
        const detailsModalElement =
            document.getElementById('eventDetailsModal');

        if (detailsModalElement) {

            const detailsModal =
                bootstrap.Modal.getInstance(detailsModalElement);

            if (detailsModal) {
                detailsModal.hide();
            }
        }


        // Open edit panel
        const editPanel =
            document.getElementById('editEventPanel');

        if (editPanel) {

            const panel =
                bootstrap.Offcanvas.getOrCreateInstance(editPanel);

            panel.show();
        }

    }
    catch (error) {

        console.error(
            "Error loading edit event form:",
            error
        );

        showAlertDialog(
            "Unable to load event details.",
            "error",
            "Error"
        );
    }
    finally {

        editEventRequestLoading = false;

        // Button may have disappeared because modal was closed
        if (document.body.contains(btn)) {
            btn.disabled = false;
            btn.innerHTML = originalHtml;
        }
    }
});

function applyEventRequestFilters() {
    const activeFilter = document.querySelector('.filter-tabs .active')?.dataset.filter || 'all';
    const searchTerm = (document.getElementById('eventRequestSearch')?.value || '').toLowerCase().trim();
    const rows = document.querySelectorAll('.event-request-row');
    let visibleCount = 0;

    rows.forEach(row => {
        const matchesFilter = activeFilter === 'all' || row.dataset.status === activeFilter;
        const matchesSearch = !searchTerm || row.dataset.title.includes(searchTerm);
        const show = matchesFilter && matchesSearch;

        row.classList.toggle('d-none', !show);
        if (show) visibleCount++;
    });

    document.getElementById('noResultsMessage')?.classList.toggle('d-none', visibleCount > 0);
}

// ═══════════════ EVENT REQUESTS: APPROVE ═══════════════
document.addEventListener('click', async function (e) {
    const approveBtn = e.target.closest('.btn-approve-event');
    if (!approveBtn) return;

    const eventId = approveBtn.dataset.eventId;
    approveBtn.disabled = true;

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const response = await fetch('/Events/ApproveEvent', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: new URLSearchParams({
                eventId,
                __RequestVerificationToken: token
            })
        });

        const result = await response.json();
        showToast(result.message, result.success);

        if (result.success) {
            setTimeout(() => window.location.reload(), 800);
        } else {
            approveBtn.disabled = false;
        }
    } catch (err) {
        showToast('Something went wrong. Please try again.', false);
        approveBtn.disabled = false;
    }
});

// ═══════════════ EVENT REQUESTS: REJECT (opens modal) ═══════════════
let pendingRejectEventId = null;

document.addEventListener('click', function (e) {
    const rejectBtn = e.target.closest('.btn-reject-event');
    if (!rejectBtn) return;

    pendingRejectEventId = rejectBtn.dataset.eventId;
    document.getElementById('rejectEventTitle').textContent = rejectBtn.dataset.eventTitle;
    document.getElementById('rejectReasonInput').value = '';
    document.getElementById('rejectReasonError').classList.add('d-none');

    bootstrap.Modal.getOrCreateInstance(document.getElementById('rejectReasonModal')).show();
});

document.addEventListener('click', async function (e) {
    if (e.target.id !== 'confirmRejectBtn') return;

    const reasonInput = document.getElementById('rejectReasonInput');
    const reason = reasonInput.value.trim();

    if (!reason) {
        document.getElementById('rejectReasonError').classList.remove('d-none');
        return;
    }

    const confirmBtn = e.target;
    confirmBtn.disabled = true;

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const response = await fetch('/Events/RejectAnEvent', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: new URLSearchParams({
                eventId: pendingRejectEventId,
                reason,
                __RequestVerificationToken: token
            })
        });

        const result = await response.json();
        showToast(result.message, result.success);

        if (result.success) {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('rejectReasonModal')).hide();
            setTimeout(() => window.location.reload(), 800);
        }
    } catch (err) {
        showToast('Something went wrong. Please try again.', false);
    } finally {
        confirmBtn.disabled = false;
    }
});


/// ============= SELECT GROUP LEADER CODE ===========

// ═══════════════ OPEN LEADER PICKER ═══════════════
document.addEventListener('click', function (e) {
    const trigger = e.target.closest('#leaderPickerTrigger');
    if (!trigger) return;

    const excludeGroupId = trigger.dataset.excludeGroupId || '';
    const url = excludeGroupId
        ? `/Groups/SelectLeaderModal?excludeGroupId=${excludeGroupId}`
        : '/Groups/SelectLeaderModal';

    fetch(url)
        .then(res => res.text())
        .then(html => {
            document.getElementById('leaderPickerModalBody').innerHTML = html;

            //hides modal form
            document.getElementById('appModal').style.visibility = 'hidden';

            bootstrap.Modal.getOrCreateInstance(document.getElementById('leaderPickerModal')).show();
        });


    // Restore the Create/Edit Group modal's visibility once the leader picker closes
    document.getElementById('leaderPickerModal')?.addEventListener('hidden.bs.modal', function () {
        const appModal = document.getElementById('appModal');
        if (appModal) appModal.style.visibility = 'visible';
    });
});


// ═══════════════ SEARCH INSIDE LEADER PICKER ═══════════════
let leaderSearchTimeout;
document.addEventListener('input', function (e) {
    if (e.target.id !== 'leaderSearchInput') return;

    clearTimeout(leaderSearchTimeout);
    leaderSearchTimeout = setTimeout(async () => {
        const search = e.target.value;
        const excludeGroupId = document.getElementById('leaderPickerTrigger')?.dataset.excludeGroupId || '';
        const url = `/Groups/SelectLeaderModal?search=${encodeURIComponent(search)}` +
            (excludeGroupId ? `&excludeGroupId=${excludeGroupId}` : '');

        const response = await fetch(url);
        const html = await response.text();
        document.getElementById('leaderPickerModalBody').innerHTML = html;

        setTimeout(() => {
            const input = document.getElementById('leaderSearchInput');
            input?.focus();
            input?.setSelectionRange(input.value.length, input.value.length);
        }, 0);
    }, 300);
});

// ═══════════════ SELECT A LEADER — updates the form UNDERNEATH, closes only the picker ═══════════════
document.addEventListener('click', function (e) {
    const row = e.target.closest('.leader-candidate-row');
    if (!row) return;

    const memberId = row.dataset.memberId;
    const memberName = row.dataset.memberName;
    const email = row.querySelector('.text-muted.small')?.textContent ?? '';
    const avatarSrc = row.querySelector('img')?.src ?? '';

    // These elements live in #appModalBody (the Create/Edit Group form),
    // which was never touched — so this just fills it in, no re-fetch needed.
    document.getElementById('selectedLeaderId').value = memberId;
    document.getElementById('selectedLeaderName').textContent = memberName;
    document.getElementById('selectedLeaderName').classList.remove('text-muted');
    document.getElementById('selectedLeaderEmail').textContent = email;

    const avatarPlaceholder = document.getElementById('leaderAvatarPlaceholder');
    if (avatarPlaceholder) {
        avatarPlaceholder.innerHTML = `<img src="${avatarSrc}" class="rounded-circle" width="40" height="40" />`;
        avatarPlaceholder.style.background = 'transparent';
    } else {
        // Edit form case: avatar is a plain <img id="leaderAvatarImg">, not a placeholder div
        const existingImg = document.getElementById('leaderAvatarImg');
        if (existingImg) existingImg.src = avatarSrc;
    }

    // Close ONLY the picker modal — #appModal and the form inside it are untouched
    bootstrap.Modal.getOrCreateInstance(document.getElementById('leaderPickerModal')).hide();
});











// ============== APPROVE AND REJECTED IN _ViewDetailsModal.cshtml

// ═══════════════ ADMIN EVENT DETAILS: APPROVE ═══════════════
document.addEventListener("click", async function (e) {

    const approveBtn = e.target.closest(".admin-approve-btn");

    if (!approveBtn)
        return;

    const eventId = approveBtn.dataset.eventId;

    const modalContent = approveBtn.closest(".event-request-modal");

    const token =
        modalContent.querySelector(
            'input[name="__RequestVerificationToken"]'
        )?.value;

    approveBtn.disabled = true;
    approveBtn.innerHTML = "Approving...";

    try {

        const response = await fetch("/Events/ApproveEvent", {
            method: "POST",

            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },

            body: new URLSearchParams({
                eventId: eventId,
                __RequestVerificationToken: token
            })
        });

        const result = await response.json();

        if (result.success) {

            const modalElement =
                document.getElementById("appModal");

            const modal =
                bootstrap.Modal.getInstance(modalElement);

            if (modal)
                modal.hide();

            showAlertDialog(
                result.message,
                "success"
            );

            setTimeout(() => {
                location.reload();
            }, 1200);
        }
        else {

            showAlertDialog(
                result.message,
                "error"
            );

            approveBtn.disabled = false;
            approveBtn.innerHTML = "Approve";
        }

    }
    catch (error) {

        console.error(error);

        showAlertDialog(
            "Something went wrong while approving the event.",
            "error"
        );

        approveBtn.disabled = false;
        approveBtn.innerHTML = "Approve";
    }
});




// ═══════════════ ADMIN EVENT DETAILS: REJECT ═══════════════
document.addEventListener("click", async function (e) {

    const rejectBtn = e.target.closest(".admin-reject-btn");

    if (!rejectBtn)
        return;

    const eventId = rejectBtn.dataset.eventId;

    const modalContent =
        rejectBtn.closest(".event-request-modal");

    const noteInput =
        modalContent.querySelector("#adminDecisionNote");

    const errorElement =
        modalContent.querySelector("#rejectNoteError");

    const reason = noteInput.value.trim();


    // Rejection requires a reason
    if (!reason) {

        errorElement.classList.remove("d-none");

        noteInput.focus();

        return;
    }

    errorElement.classList.add("d-none");


    const token =
        modalContent.querySelector(
            'input[name="__RequestVerificationToken"]'
        )?.value;


    rejectBtn.disabled = true;
    rejectBtn.innerHTML = "Rejecting...";


    try {

        const response =
            await fetch("/Events/RejectAnEvent", {

                method: "POST",

                headers: {
                    "Content-Type":
                        "application/x-www-form-urlencoded"
                },

                body: new URLSearchParams({
                    eventId: eventId,
                    reason: reason,
                    __RequestVerificationToken: token
                })
            });


        const result = await response.json();


        if (result.success) {

            const modalElement =
                document.getElementById("appModal");

            const modal =
                bootstrap.Modal.getInstance(modalElement);

            if (modal)
                modal.hide();


            showAlertDialog(
                result.message,
                "success"
            );


            setTimeout(() => {
                location.reload();
            }, 1200);
        }
        else {

            showAlertDialog(
                result.message,
                "error"
            );

            rejectBtn.disabled = false;
            rejectBtn.innerHTML = "Reject";
        }

    }
    catch (error) {

        console.error(error);

        showAlertDialog(
            "Something went wrong while rejecting the event.",
            "error"
        );

        rejectBtn.disabled = false;
        rejectBtn.innerHTML = "Reject";
    }
});



// ═══════════════ CLICK GROUP ROW ═══════════════
document.addEventListener("click", function (e) {

    const btn = e.target.closest(".open-group-participation-modal");

    if (!btn) return;


    const url = btn.getAttribute("data-url");

    const title = btn.getAttribute("data-title");


    openGroupParticipationModal(title, url);
});


// ── Global loading overlay helpers ──
function showLoading() {
    const overlay = document.getElementById("globalLoadingOverlay");

    if (overlay) {
        overlay.classList.add("show");
    }
}

function hideLoading() {
    const overlay = document.getElementById("globalLoadingOverlay");

    if (overlay) {
        overlay.classList.remove("show");
    }
}

window.addEventListener("load", function () {
    hideLoading();
});


// Also fixes browser back/forward navigation
window.addEventListener("pageshow", function () {
    hideLoading();
});



// Client-side filtering for the All Events table
function filterAllEventsTable() {
    const search = (document.getElementById('allEventsSearch')?.value || '').toLowerCase();
    const group = document.getElementById('filterGroup')?.value || '';
    const facility = document.getElementById('filterFacility')?.value || '';

    document.querySelectorAll('#allEventsTable .all-events-row').forEach(row => {
        const matchesSearch = !search || row.dataset.search.includes(search);
        const matchesGroup = !group || row.dataset.group.includes(group);
        const matchesFacility = !facility || row.dataset.facility.includes(facility);
        row.style.display = (matchesSearch && matchesGroup && matchesFacility) ? '' : 'none';
    });
}

document.addEventListener('input', e => {
    if (e.target.id === 'allEventsSearch') filterAllEventsTable();
});
document.addEventListener('change', e => {
    if (e.target.id === 'filterGroup' || e.target.id === 'filterFacility') filterAllEventsTable();
});



// ═══════════════ READ-ONLY EVENT DETAILS MODAL (All Events / Assigned Events) ═══════════════
document.addEventListener('click', function (e) {
    const trigger = e.target.closest('.all-events-row, .view-event-details-btn');
    if (!trigger) return;

    const eventId = trigger.dataset.eventId;


    document.getElementById('appModalLabel').innerText = '';
    document.getElementById('appModalIcon').classList.add('d-none');
    document.getElementById('appModalSubtitle').classList.add('d-none');

    fetch(`/Leader/Events/AllEventsDetails/${eventId}`, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
        .then(res => res.text())
        .then(html => {
            document.getElementById('appModalBody').innerHTML = html;
            bootstrap.Modal.getOrCreateInstance(document.getElementById('appModal')).show();
        })
        .catch(err => {
            console.error(err);
            showAlertDialog('Unable to load event details. Please try again.', 'error');
        });
});


// ═══════════════ ATTENDANCE ROW: toggle "Scanned by" popover ═══════════════
document.addEventListener('click', function (e) {
    const toggleBtn = e.target.closest('.attendance-details-toggle');

    // Clicked outside any toggle/popover — close everything
    if (!toggleBtn) {
        if (!e.target.closest('.attendance-details-popover')) {
            closeAllAttendancePopovers();
        }
        return;
    }

    const cell = toggleBtn.closest('.attendance-actions-cell');
    const popover = cell.querySelector('.attendance-details-popover');
    const isOpen = !popover.classList.contains('d-none');

    closeAllAttendancePopovers();

    if (!isOpen) {
        popover.classList.remove('d-none');
        toggleBtn.classList.add('open');
    }
});

function closeAllAttendancePopovers() {
    document.querySelectorAll('.attendance-details-popover').forEach(p => p.classList.add('d-none'));
    document.querySelectorAll('.attendance-details-toggle.open').forEach(b => b.classList.remove('open'));
}




// ═══════════════ ATTENDANCE TABLE: search + status filter + pagination ═══════════════
(function () {
    const PAGE_SIZE = 5;
    let currentPage = 1;

    function getAttendanceRows() {
        return Array.from(document.querySelectorAll('#attendanceTable tbody tr[data-status]'));
    }

    function getFilteredRows() {
        const searchInput = document.getElementById('memberSearch');
        const statusSelect = document.getElementById('statusFilter');
        const searchTerm = (searchInput?.value || '').toLowerCase().trim();
        const statusValue = statusSelect?.value || 'all';

        return getAttendanceRows().filter(row => {
            const matchesSearch = !searchTerm || row.dataset.name.includes(searchTerm);
            const matchesStatus = statusValue === 'all' || row.dataset.status === statusValue;
            return matchesSearch && matchesStatus;
        });
    }

    function renderAttendanceTable() {
        const allRows = getAttendanceRows();
        const filtered = getFilteredRows();
        const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));

        if (currentPage > totalPages) currentPage = totalPages;

        const start = (currentPage - 1) * PAGE_SIZE;
        const end = start + PAGE_SIZE;
        const pageRows = filtered.slice(start, end);

        allRows.forEach(row => row.classList.add('pagination-hidden'));
        pageRows.forEach(row => row.classList.remove('pagination-hidden'));

        const info = document.getElementById('paginationInfo');
        const firstBtn = document.getElementById('paginationFirstBtn');
        const nextBtn = document.getElementById('paginationNextBtn');

        if (info) {
            info.textContent = filtered.length === 0
                ? 'No members found'
                : `Showing ${start + 1}–${Math.min(end, filtered.length)} of ${filtered.length}`;
        }

        if (firstBtn) firstBtn.disabled = currentPage === 1;
        if (nextBtn) nextBtn.disabled = currentPage >= totalPages;
    }

    document.addEventListener('input', function (e) {
        if (e.target.id !== 'memberSearch') return;
        currentPage = 1;
        renderAttendanceTable();
    });

    document.addEventListener('change', function (e) {
        if (e.target.id !== 'statusFilter') return;
        currentPage = 1;
        renderAttendanceTable();
    });

    document.addEventListener('click', function (e) {
        if (e.target.id === 'paginationFirstBtn') {
            currentPage = 1;
            renderAttendanceTable();
        }
        if (e.target.id === 'paginationNextBtn') {
            currentPage += 1;
            renderAttendanceTable();
        }
    });

    // Run once on load, and again whenever this partial gets swapped into a modal/tab via AJAX
    document.addEventListener('DOMContentLoaded', renderAttendanceTable);
    document.addEventListener('shown.bs.tab', renderAttendanceTable);
    document.addEventListener('shown.bs.modal', renderAttendanceTable);
})();



document.addEventListener('click', async function (e) {
    const btn = e.target.closest('.send-reminder-btn');

    if (!btn) return;

    const eventId = btn.dataset.eventId;
    const reminderType = btn.dataset.reminderType;

    try {
        btn.disabled = true;

        const response = await fetch(
            `/Events/SendAttendanceReminder?eventId=${eventId}&reminderType=${reminderType}`,
            {
                method: 'POST'
            }
        );

        const data = await response.json();

        if (data.success) {
            showAlertDialog(
                data.message,
                "success",
                "Reminder Sent"
            );
        } else {
            showAlertDialog(
                data.message,
                "error",
                "Unable to Send Reminder"
            );
        }
    }
    catch (error) {
        console.error(error);

        showAlertDialog(
            "Something went wrong while sending the attendance reminder.",
            "error",
            "Error"
        );
    }
    finally {
        btn.disabled = false;
    }
});

document.addEventListener('click', function (e) {
    const btn = e.target.closest('.cancel-event-btn');
    if (!btn) return;

    e.preventDefault();
    const eventId = btn.getAttribute('data-id');

    fetch('/Events/GetCancelForm?eventId=' + eventId)
        .then(r => r.text())
        .then(html => {
            document.getElementById('appModalLabel').textContent = 'Cancel Event';
            document.getElementById('appModalBody').innerHTML = html;
            bootstrap.Modal.getOrCreateInstance(document.getElementById('appModal')).show();
        })
        .catch(err => {
            console.error(err);
            showAlertDialog('Unable to load the cancel form. Please try again.', 'error');
        });
});




document.addEventListener("submit", async function (e) {

    const form = e.target.closest(".evaluation-close-form");

    if (!form) {
        return;
    }

    // Stop the normal form submission.
    e.preventDefault();

    const submitButton = form.querySelector(".close-evaluation-btn");

    if (submitButton) {
        submitButton.disabled = true;
    }

    try {

        const formData = new FormData(form);

        const response = await fetch(form.action, {
            method: "POST",
            body: formData
        });

        const result = await response.json();

        if (result.success) {

            // Close your existing shared modal.
            const modalElement = document.getElementById("appModal");

            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);

                if (modal) {
                    modal.hide();
                }
            }

            showAlertDialog(
                result.message || "Evaluation closed successfully.",
                "success",
                "Evaluation Closed"
            );

            setTimeout(function () {
                window.location.reload();
            }, 1200);

        } else {

            showAlertDialog(
                result.message || "Unable to close evaluation.",
                "error",
                "Close Failed"
            );

            if (submitButton) {
                submitButton.disabled = false;
            }
        }

    } catch (error) {

        console.error(error);

        showAlertDialog(
            "Something went wrong while closing the evaluation.",
            "error",
            "Close Error"
        );

        if (submitButton) {
            submitButton.disabled = false;
        }
    }
});




document.addEventListener("input", function (e) {

    if (e.target.id !== "emailInput") {
        return;
    }

    updateEmailVerifyState(e.target);
});

document.addEventListener("DOMContentLoaded", function () {
    const emailInput = document.getElementById("emailInput");
    if (emailInput) updateEmailVerifyState(emailInput);
});

function updateEmailVerifyState(emailInput) {
    const form = emailInput.closest("form");
    if (!form) return;

    const originalEmail = form.querySelector("#originalEmailInput")?.value.trim().toLowerCase() || "";
    const currentEmail = emailInput.value.trim().toLowerCase();

    const verifyBtn = form.querySelector("#verifyEmailBtn");
    const verifiedBadge = form.querySelector("#emailVerifiedBadge");
    const codeField = form.querySelector("#emailVerificationCodeInput");
    const saveBtn = form.querySelector("#saveProfileBtn");

    const emailChanged = currentEmail !== originalEmail && currentEmail.length > 0;

    if (emailChanged) {
        verifyBtn.classList.remove("d-none");

        // Any further edit to the email after a code was verified
        // invalidates that verification — clear it and require re-verify.
        if (codeField.dataset.verifiedFor !== currentEmail) {
            verifiedBadge.classList.add("d-none");
            codeField.value = "";
            saveBtn.disabled = true;
        }
    } else {
        // Email reverted back to original — no verification needed
        verifyBtn.classList.add("d-none");
        verifiedBadge.classList.add("d-none");
        codeField.value = "";
        saveBtn.disabled = false;
    }
}


let pendingVerifyForm = null;

document.addEventListener("click", async function (e) {

    if (e.target.id !== "verifyEmailBtn") {
        return;
    }

    const form = e.target.closest("form");
    const emailInput = form.querySelector("#emailInput");
    const newEmail = emailInput.value.trim();

    if (!newEmail) {
        return;
    }

    const btn = e.target;
    const originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm"></span>`;

    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch("/EmailVerification/SendCode", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({ email: newEmail, __RequestVerificationToken: token })
        });

        const result = await response.json();

        if (!result.success) {
            showAlertDialog(result.message || "Unable to send verification code.", "error", "Failed");
            return;
        }

        showAlertDialog(result.message || "Verification code sent.", "success", "Code Sent");

        pendingVerifyForm = form;

        document.getElementById("verifyEmailTarget").textContent = newEmail;
        document.getElementById("verificationCodeInput").value = "";
        document.getElementById("verificationCodeError").classList.add("d-none");

        bootstrap.Modal.getOrCreateInstance(document.getElementById("emailVerificationModal")).show();

    } catch (error) {
        console.error("Send code error:", error);
        showAlertDialog("Something went wrong sending the verification code.", "error", "Error");
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalHtml;
    }
});


document.addEventListener("click", async function (e) {

    if (e.target.id !== "confirmVerificationBtn") {
        return;
    }

    if (!pendingVerifyForm) {
        return;
    }

    const code = document.getElementById("verificationCodeInput").value.trim();
    const errorEl = document.getElementById("verificationCodeError");
    const emailInput = pendingVerifyForm.querySelector("#emailInput");
    const currentEmail = emailInput.value.trim();

    if (!code) {
        errorEl.textContent = "Please enter the code.";
        errorEl.classList.remove("d-none");
        return;
    }


    const codeField = pendingVerifyForm.querySelector("#emailVerificationCodeInput");
    codeField.value = code;
    codeField.dataset.verifiedFor = currentEmail.toLowerCase();

    pendingVerifyForm.querySelector("#emailVerifiedBadge").classList.remove("d-none");
    pendingVerifyForm.querySelector("#saveProfileBtn").disabled = false;

    bootstrap.Modal.getInstance(document.getElementById("emailVerificationModal"))?.hide();

    pendingVerifyForm = null;
});




document.addEventListener("click", async function (e) {

    if (e.target.id !== "verifyEmailBtn") {
        return;
    }

    const form = e.target.closest("form");
    const emailInput = form.querySelector("#emailInput");
    const newEmail = emailInput.value.trim();

    if (!newEmail) {
        return;
    }

    const btn = e.target;
    const originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm"></span>`;

    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch("/EmailVerification/SendCode", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({ email: newEmail, __RequestVerificationToken: token })
        });

        const result = await response.json();

        if (!result.success) {
            showAlertDialog(result.message || "Unable to send verification code.", "error", "Failed");
            return;
        }

        showAlertDialog(result.message || "Verification code sent.", "success", "Code Sent");

        pendingVerifyForm = form;

        const modalEl = ensureEmailVerificationModal();

        modalEl.querySelector("#verifyEmailTarget").textContent = newEmail;
        modalEl.querySelector("#verificationCodeInput").value = "";
        modalEl.querySelector("#verificationCodeError").classList.add("d-none");

        bootstrap.Modal.getOrCreateInstance(modalEl).show();

    } catch (error) {
        console.error("Send code error:", error);
        showAlertDialog("Something went wrong sending the verification code.", "error", "Error");
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalHtml;
    }
});

function goBack(fallbackUrl) {
    if (document.referrer && new URL(document.referrer).origin === location.origin) {
        history.back();
    } else {
        window.location.href = fallbackUrl;
    }
}