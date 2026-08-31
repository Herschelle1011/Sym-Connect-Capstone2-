
document.addEventListener('DOMContentLoaded', () => {

    // ===== Modal control =====
    const overlay = document.getElementById('announcementModalOverlay');
    const modalBody = document.getElementById('announcementModalBody');
    const modalTitle = document.getElementById('announcementModalTitle');

    function openModal(title) {
        modalTitle.textContent = title;
        overlay.style.display = 'flex';
    }

    function closeModal() {
        overlay.style.display = 'none';
        modalBody.innerHTML = '';
    }

    document.getElementById('btnCloseAnnouncementModal').addEventListener('click', closeModal);

    // close on backdrop click, not on box click
    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) closeModal();
    });

    // ===== New Announcement =====
    document.getElementById('btnNewAnnouncement').addEventListener('click', async () => {
        const res = await fetch('/Announcements/Create', { method: 'GET' });
        modalBody.innerHTML = await res.text();
        openModal('New Announcement');
        bindConditionalFields();
        bindFormSubmit();
        bindCancelButton();
    });

    // ===== Edit Announcement (event delegation - table rows render server-side) =====
    document.getElementById('announcementTableBody').addEventListener('click', async (e) => {
        const editBtn = e.target.closest('.btnEditAnnouncement');
        const archiveBtn = e.target.closest('.btnArchiveAnnouncement');

        if (editBtn) {
            const id = editBtn.dataset.id;
            const res = await fetch(`/Announcements/Edit/${id}`);
            modalBody.innerHTML = await res.text();
            openModal('Edit Announcement');
            bindConditionalFields();
            bindFormSubmit();
            bindCancelButton();
        }

        if (archiveBtn) {
            const id = archiveBtn.dataset.id;
            if (!confirm('Archive this announcement? It will stop appearing everywhere.')) return;

            const res = await fetch(`/Announcements/Archive/${id}`, {
                method: 'POST',
                headers: { 'RequestVerificationToken': getAntiForgeryToken() }
            });
            const data = await res.json();
            if (data.success) {
                editBtn.closest('tr').remove();
            }
        }
    });

    function bindCancelButton() {
        const btn = document.getElementById('btnCancelAnnouncement');
        if (btn) btn.addEventListener('click', closeModal);
    }

    // ===== Conditional field show/hide (mirrors the screenshot behavior) =====
    function bindConditionalFields() {
        const audienceRadios = document.querySelectorAll('input[name="TargetAudience"]');
        const groupField = document.getElementById('groupField');
        const publishSelect = document.getElementById('publishOptionSelect');
        const scheduleField = document.getElementById('scheduleField');
        const expirationSelect = document.getElementById('expirationOptionSelect');
        const customExpirationField = document.getElementById('customExpirationField');

        function toggleGroupField() {
            const checked = document.querySelector('input[name="TargetAudience"]:checked');
            groupField.style.display = (checked && checked.value === 'SpecificGroup') ? 'block' : 'none';
        }

        function toggleScheduleField() {
            scheduleField.style.display = (publishSelect.value === 'Scheduled') ? 'block' : 'none';
        }

        function toggleCustomExpirationField() {
            customExpirationField.style.display = (expirationSelect.value === 'Custom') ? 'block' : 'none';
        }

        audienceRadios.forEach(r => r.addEventListener('change', toggleGroupField));
        publishSelect.addEventListener('change', toggleScheduleField);
        expirationSelect.addEventListener('change', toggleCustomExpirationField);

        // set initial visibility on load (e.g. editing an existing scheduled announcement)
        toggleGroupField();
        toggleScheduleField();
        toggleCustomExpirationField();
    }

    // ===== Client-side cross-field validation (mirrors AnnouncementViewModel.Validate) =====
    // This is a UX speed-up only. The server-side IValidatableObject check is the real gate -
    // this never replaces it, it just avoids a round trip for the common mistakes.
    function validateAnnouncementForm() {
        const errors = [];
        clearFieldErrors();

        const targetAudience = document.querySelector('input[name="TargetAudience"]:checked')?.value;
        const groupId = document.getElementById('GroupId')?.value;
        if (targetAudience === 'SpecificGroup' && !groupId) {
            errors.push({ field: 'GroupId', message: 'Please select a group.' });
        }

        const publishOption = document.getElementById('publishOptionSelect').value;
        const publishDateInput = document.getElementById('PublishDate');
        const publishDateVal = publishDateInput ? publishDateInput.value : null;

        if (publishOption === 'Scheduled') {
            if (!publishDateVal) {
                errors.push({ field: 'PublishDate', message: 'Please pick a schedule date and time.' });
            } else if (new Date(publishDateVal) <= new Date()) {
                errors.push({ field: 'PublishDate', message: 'Scheduled date must be in the future.' });
            }
        }

        const expirationOption = document.getElementById('expirationOptionSelect').value;
        const customExpInput = document.getElementById('CustomExpirationDate');
        const customExpVal = customExpInput ? customExpInput.value : null;

        if (expirationOption === 'Custom') {
            if (!customExpVal) {
                errors.push({ field: 'CustomExpirationDate', message: 'Please pick a custom expiration date.' });
            } else {
                const effectivePublish = (publishOption === 'Scheduled' && publishDateVal)
                    ? new Date(publishDateVal)
                    : new Date();
                if (new Date(customExpVal) <= effectivePublish) {
                    errors.push({ field: 'CustomExpirationDate', message: 'Expiration must be after the publish date.' });
                }
            }
        }

        return errors;
    }

    function clearFieldErrors() {
        document.querySelectorAll('.field-error').forEach(el => el.textContent = '');
    }

    function showFieldErrors(errors) {
        errors.forEach(err => {
            const span = document.querySelector(`span[data-valmsg-for="${err.field}"]`);
            if (span) span.textContent = err.message;
        });
    }

    // ===== Form submit (Create + Edit share the same handler; mode read from data-mode) =====
    function bindFormSubmit() {
        const form = document.getElementById('announcementForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const clientErrors = validateAnnouncementForm();
            if (clientErrors.length > 0) {
                showFieldErrors(clientErrors);
                return;
            }

            const formData = new FormData(form);
            const res = await fetch(form.action, {
                method: 'POST',
                body: formData
            });

            // Server-side validation failed -> response is the re-rendered partial with errors
            const contentType = res.headers.get('content-type') || '';
            if (contentType.includes('application/json')) {
                const data = await res.json();
                if (data.success) {
                    closeModal();
                    location.reload(); // simplest correct refresh of the table; swap for a targeted row update later if needed
                }
            } else {
                modalBody.innerHTML = await res.text();
                bindConditionalFields();
                bindFormSubmit();
                bindCancelButton();
            }
        });
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

}); // end DOMContentLoaded