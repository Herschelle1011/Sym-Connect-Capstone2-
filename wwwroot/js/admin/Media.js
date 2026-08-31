document.addEventListener('DOMContentLoaded', () => {

    // ===== Shared modal shell (present on both Index and FolderDetails) =====
    const overlay = document.getElementById('mediaModalOverlay');
    const modalBody = document.getElementById('mediaModalBody');
    const modalTitle = document.getElementById('mediaModalTitle');
    const closeBtn = document.getElementById('btnCloseMediaModal');

    if (!overlay || !modalBody || !modalTitle) return; // page doesn't use the modal shell at all

    function openModal(title) {
        modalTitle.textContent = title;
        overlay.style.display = 'flex';
    }

    function closeModal() {
        overlay.style.display = 'none';
        modalBody.innerHTML = '';
    }

    if (closeBtn) closeBtn.addEventListener('click', closeModal);
    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) closeModal();
    });

    // ===== New Folder (Index page only) =====
    const btnNewFolder = document.getElementById('btnNewFolder');
    if (btnNewFolder) {
        btnNewFolder.addEventListener('click', async () => {
            const res = await fetch('/MediaAndDocuments/CreateFolder', { method: 'GET' });
            modalBody.innerHTML = await res.text();
            openModal('New Folder');
            bindCreateFolderForm();
        });
    }

    function bindCreateFolderForm() {
        const cancelBtn = document.getElementById('btnCancelCreateFolder');
        if (cancelBtn) cancelBtn.addEventListener('click', closeModal);

        const form = document.getElementById('createFolderForm');
        if (!form) return;

        const submitBtn = form.querySelector('button[type="submit"]');

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (submitBtn) submitBtn.disabled = true;

            try {
                const formData = new FormData(form);
                const res = await fetch(form.action, { method: 'POST', body: formData });

                // CreateFolder always returns JSON - success or failure, checked in the controller
                const data = await res.json();

                if (data.success) {
                    closeModal();
                    showAlertDialog(data.message, 'success');
                    setTimeout(() => location.reload(), 900);
                } else {
                    // keep the modal open so they can fix the name and retry
                    showAlertDialog(data.message, 'error');
                    if (submitBtn) submitBtn.disabled = false;
                }
            } catch (err) {
                console.error(err);
                showAlertDialog('Something went wrong. Please try again.', 'error');
                if (submitBtn) submitBtn.disabled = false;
            }
        });
    }

    // ===== Upload Files (FolderDetails page only) =====
    const btnUploadFiles = document.getElementById('btnUploadFiles');
    if (btnUploadFiles) {
        btnUploadFiles.addEventListener('click', async () => {
            const folderId = btnUploadFiles.dataset.folderId; // "" when Unfiled (folderId is null)
            const query = folderId ? `?folderId=${folderId}` : '';

            const res = await fetch(`/MediaAndDocuments/UploadFiles${query}`, { method: 'GET' });
            modalBody.innerHTML = await res.text();
            openModal('Upload Files');
            bindUploadFilesForm();
        });
    }

    function bindUploadFilesForm() {
        const cancelBtn = document.getElementById('btnCancelUploadFiles');
        if (cancelBtn) cancelBtn.addEventListener('click', closeModal);

        const form = document.getElementById('uploadFilesForm');
        if (!form) return;

        const submitBtn = form.querySelector('button[type="submit"]');

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (submitBtn) submitBtn.disabled = true;

            try {
                const formData = new FormData(form); // multipart/form-data - browser sets this automatically because of the file input
                const res = await fetch(form.action, { method: 'POST', body: formData });

                const data = await res.json(); // UploadFiles action always returns JSON, never a partial

                if (data.success) {
                    closeModal();
                    showAlertDialog(data.message, 'success');
                    setTimeout(() => location.reload(), 900);
                } else {
                    // keep the modal open so they can pick different files and retry
                    showAlertDialog(data.message, 'error');
                    if (submitBtn) submitBtn.disabled = false;
                }
            } catch (err) {
                console.error(err);
                showAlertDialog('Something went wrong. Please try again.', 'error');
                if (submitBtn) submitBtn.disabled = false;
            }
        });
    }

    // ===== Delete File (FolderDetails page - event delegation since rows are server-rendered) =====
    // Left on showToast, as before - only Create Folder / Upload Files were asked to switch to showAlertDialog.
    document.addEventListener('click', async function (e) {
        const deleteBtn = e.target.closest('.btnDeleteFile');
        if (!deleteBtn) return;

        if (!confirm('Delete this file? This cannot be undone.')) return;

        const mediaFileId = deleteBtn.dataset.id;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        deleteBtn.disabled = true;

        try {
            const response = await fetch('/MediaAndDocuments/DeleteFile', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({
                    mediaFileId,
                    __RequestVerificationToken: token
                })
            });

            const result = await response.json();

            if (result.success) {
                deleteBtn.closest('tr').remove();
                if (typeof showToast === 'function') showToast(result.message, true);
            } else {
                if (typeof showToast === 'function') showToast(result.message, false);
                deleteBtn.disabled = false;
            }
        } catch (err) {
            console.error(err);
            if (typeof showToast === 'function') showToast('Something went wrong. Please try again.', false);
            deleteBtn.disabled = false;
        }
    });

});