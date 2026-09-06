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

//FINDS ALL class = .open-modal
document.querySelectorAll(".open-modal").forEach(button => {
    button.addEventListener("click", function () {
        const url = this.getAttribute("data-url"); //GET ALL URL NOT DATA ACTION 
        const title = this.getAttribute("data-title"); //get all data title in a button
        openModal(title, url); //open modal with data-url to get the get action in controller
    });
});


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

    const submitBtn =
        form.querySelector('button[type="submit"]');

    if (submitBtn) {
        submitBtn.disabled = true;
    }

    try {

        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form)
        });

        const contentType =
            response.headers.get("content-type");

        // Controller returned JSON
        if (
            contentType &&
            contentType.includes("application/json")
        ) {

            const result = await response.json();

            console.log("Modal form result:", result);

            if (result.success) {

                // Close normal app modal
                const modalElement =
                    document.getElementById("appModal");

                const modal =
                    bootstrap.Modal.getInstance(modalElement);

                if (modal) {
                    modal.hide();
                }

                // Show success alert
                showAlertDialog(
                    result.message || "Saved successfully.",
                    "success",
                    "Success"
                );

                // Give user time to see alert
                setTimeout(() => {
                    location.reload();
                }, 1200);

            } else {

                showAlertDialog(
                    result.message || "Unable to save.",
                    "error",
                    "Failed"
                );

                if (submitBtn) {
                    submitBtn.disabled = false;
                }
            }
        }

        // Controller returned another partial view
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

        if (submitBtn) {
            submitBtn.disabled = false;
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

// ── Modal opener (data-attribute buttons, e.g. "Create Partylist") ──
document.querySelectorAll(".open-modal").forEach(button => {
    button.addEventListener("click", function () {
        const url = this.getAttribute("data-url");
        const title = this.getAttribute("data-title");
        const size = this.getAttribute("data-modal-size");
        const icon = this.getAttribute("data-icon");
        const subtitle = this.getAttribute("data-subtitle");
        openModal(title, url, size, {icon, subtitle});
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
            showAlertDialog('Unable to load the facility schedule. Please try again.', 'error');
        });
});



document.addEventListener("submit", async function (e) {
    //EXISTING GLOBAL MODAL SUBMIT FORMS
    const modalFormIds = ["createGroupForm", "removeMemberForm", "transferMemberForm", "editGroupForm"];
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
document.addEventListener('click', async function (e) {
    const btn = e.target.closest('.edit-event-from-modal-btn');
    if (!btn) return;

    const eventId = btn.dataset.eventId;

    const response = await fetch(`/Leader/Events/GetEditForm?eventid=${eventId}`);
    if (!response.ok) {
        showAlertDialog('Unable to load event details.', 'error');
        return;
    }

    const html = await response.text();

    const existingPanelEl = document.getElementById('editEventPanel');
    if (existingPanelEl) {
        const existingInstance = bootstrap.Offcanvas.getInstance(existingPanelEl);
        if (existingInstance) existingInstance.dispose();
        existingPanelEl.remove();
    }

    document.body.insertAdjacentHTML('beforeend', html);

    const editForm = document.getElementById("editEventForm");
    $.validator.unobtrusive.parse(editForm);

    const detailsModal = bootstrap.Modal.getInstance(document.getElementById('eventDetailsModal'));
    if (detailsModal) detailsModal.hide();

    const panel = new bootstrap.Offcanvas(document.getElementById('editEventPanel'));
    panel.show();
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


// ═══════════════ GROUP PARTICIPATION: OPEN MEMBER DETAILS MODAL ═══════════════
// ═══════════════ GROUP PARTICIPATION: OPEN MEMBER DETAILS MODAL ═══════════════
function openGroupParticipationModal(title, url) {

    document.getElementById("groupParticipationModalLabel").innerText = title;

    fetch(url, {
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        }
    })
        .then(response => response.text())
        .then(html => {

            // Put the returned partial view inside the modal
            document.getElementById("groupParticipationModalBody").innerHTML = html;


            // NOW initialize search, sorting, and pagination
            initializeMemberParticipation();


            // Open the modal
            const modal = bootstrap.Modal.getOrCreateInstance(
                document.getElementById("groupParticipationModal")
            );

            modal.show();
        })
        .catch(error => {
            console.error("Error loading group participation:", error);
        });
}


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