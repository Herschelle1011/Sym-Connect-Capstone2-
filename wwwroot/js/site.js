// ── Modal opener (global function for inline onclick calls) ──
// function openModal(title, url) {
//     document.getElementById("appModalLabel").innerText = title;

//     fetch(url)
//         .then(response => response.text())
//         .then(html => {
//             document.getElementById("appModalBody").innerHTML = html;

//             const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById("appModal"));
//             modal.show();

//             if (window.jQuery && $.validator) {
//                 $.validator.unobtrusive.parse("#appModalBody");
//             }
//         });
// }

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
function openModal(title, url, size) {
    document.getElementById("appModalLabel").innerText = title;

    const dialog = document.querySelector("#appModal .modal-dialog");
    dialog.classList.remove("modal-lg", "modal-xl");
    if (size) dialog.classList.add(`modal-${size}`);

    fetch(url)
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

// ── Modal opener (data-attribute buttons, e.g. "Create Partylist") ──
document.querySelectorAll(".open-modal").forEach(button => {
    button.addEventListener("click", function () {
        const url = this.getAttribute("data-url");
        const title = this.getAttribute("data-title");
        const size = this.getAttribute("data-modal-size");
        openModal(title, url, size);
    });
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
        });
});



document.addEventListener("submit", async function (e) {
    const modalFormIds = ["createGroupForm", "removeMemberForm", "transferMemberForm", "editGroupForm", "createMemberForm"];
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
    const provinceSelect = document.getElementById('provinceSelect');
    const citySelect = document.getElementById('citySelect');
    const barangaySelect = document.getElementById('barangaySelect');

    if (!provinceSelect) return; // form not in DOM yet / not this modal

    // Avoid re-fetching if already populated (e.g. modal reopened)
    if (provinceSelect.dataset.loaded === "true") return;

    fetch('https://psgc.gitlab.io/api/provinces/')
        .then(res => res.json()) 
        .then(provinces => {
            provinces
                .sort((a, b) => a.name.localeCompare(b.name))
                .forEach(p => {
                    const opt = document.createElement('option');
                    opt.value = p.code;
                    opt.textContent = p.name;
                    provinceSelect.appendChild(opt);
                });
            provinceSelect.dataset.loaded = "true";
        })
        .catch(err => console.error('Failed to load provinces:', err));

    provinceSelect.addEventListener('change', function () {
        citySelect.innerHTML = '<option value="">-- Select City/Municipality --</option>';
        barangaySelect.innerHTML = '<option value="">-- Select City First --</option>';
        citySelect.disabled = true;
        barangaySelect.disabled = true;

        if (!this.value) return;

        fetch(`https://psgc.gitlab.io/api/provinces/${this.value}/cities-municipalities/`)
            .then(res => res.json())
            .then(cities => {
                cities
                    .sort((a, b) => a.name.localeCompare(b.name))
                    .forEach(c => {
                        const opt = document.createElement('option');
                        opt.value = c.code;
                        opt.textContent = c.name;
                        citySelect.appendChild(opt);
                    });
                citySelect.disabled = false;
            })
            .catch(err => console.error('Failed to load cities:', err));
    });

    citySelect.addEventListener('change', function () {
        barangaySelect.innerHTML = '<option value="">-- Select Barangay --</option>';
        barangaySelect.disabled = true;

        if (!this.value) return;

        fetch(`https://psgc.gitlab.io/api/cities-municipalities/${this.value}/barangays/`)
            .then(res => res.json())
            .then(barangays => {
                barangays
                    .sort((a, b) => a.name.localeCompare(b.name))
                    .forEach(b => {
                        const opt = document.createElement('option');
                        opt.value = b.name;
                        opt.textContent = b.name;
                        barangaySelect.appendChild(opt);
                    });
                barangaySelect.disabled = false;
            })
            .catch(err => console.error('Failed to load barangays:', err));
    });
}
