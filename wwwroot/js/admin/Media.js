

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
                const data = await res.json();

                if (data.success) {
                    closeModal();
                    showAlertDialog(data.message, 'success');
                    setTimeout(() => location.reload(), 900);
                } else {
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
            const folderId = btnUploadFiles.dataset.folderId;
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
                const formData = new FormData(form);
                const res = await fetch(form.action, { method: 'POST', body: formData });
                const data = await res.json();

                if (data.success) {
                    closeModal();
                    showAlertDialog(data.message, 'success');
                    setTimeout(() => location.reload(), 900);
                } else {
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

// ========================================
// RENAME FOLDER
// ========================================
document.addEventListener("submit", async function (e) {

    if (e.target.id !== "renameFolderForm") {
        return;
    }

    e.preventDefault();



    const form = e.target;

    const submitBtn =
        form.querySelector('button[type="submit"]');

    if (submitBtn) {
        submitBtn.disabled = true;
    }

    showLoading();


    try {

        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form)
        });

        const result = await response.json();

        console.log("Rename result:", result);

        if (result.success) {

            // Close Rename modal
            const modalElement =
                document.getElementById("appModal");

            const modal =
                bootstrap.Modal.getInstance(modalElement);

            if (modal) {
                modal.hide();
            }

            // Show alert modal
            showAlertDialog(
                result.message,
                "success",
                "Folder Renamed"
            );

            setTimeout(() => {
                location.reload();
            }, 1200);

        } else {

            showAlertDialog(
                result.message,
                "error",
                "Rename Failed"
            );

            if (submitBtn) {
                submitBtn.disabled = false;
                hideLoading();

            }
        }

    } catch (error) {

        console.error("Rename error:", error);

        showAlertDialog(
            "Something went wrong while renaming the folder.",
            "error",
            "Rename Failed"
        );

        if (submitBtn) {
            submitBtn.disabled = false;
        }
    }
});


// =========================================================
// ARCHIVE FOLDER
// =========================================================
document.addEventListener('submit', async function (e) {
    if (e.target.id !== 'archiveFolderForm') return;

    e.preventDefault();
    const form = e.target;

    const submitBtn = form.querySelector('button[type="submit"]');
    if (submitBtn) submitBtn.disabled = true;

    try {
        const response = await fetch(form.action, {
            method: 'POST',
            body: new FormData(form)
        });

        const result = await response.json();

        if (result.success) {
            const modalElement = document.getElementById('appModal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) modal.hide();

            showAlertDialog(
                result.message || 'Folder archived successfully.',
                'success',
                'Success'
            );

            setTimeout(() => location.reload(), 1000);
        } else {
            showAlertDialog(
                result.message || 'Unable to archive folder.',
                'error',
                'Failed'
            );
            if (submitBtn) submitBtn.disabled = false;
        }
    } catch (error) {
        console.error('Archive error:', error);
        showAlertDialog(
            'Something went wrong while archiving.',
            'error',
            'Error'
        );
        if (submitBtn) submitBtn.disabled = false;
    }
});