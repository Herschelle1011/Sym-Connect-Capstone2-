

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
// document.addEventListener('submit', async function (e) {
//     if (e.target.id !== 'archiveFolderForm') return;

//     e.preventDefault();
//     const form = e.target;

//     const submitBtn = form.querySelector('button[type="submit"]');
//     if (submitBtn) submitBtn.disabled = true;

//     try {
//         const response = await fetch(form.action, {
//             method: 'POST',
//             body: new FormData(form)
//         });

//         const result = await response.json();

//         if (result.success) {
//             const modalElement = document.getElementById('appModal');
//             const modal = bootstrap.Modal.getInstance(modalElement);
//             if (modal) modal.hide();

//             showAlertDialog(
//                 result.message || 'Folder archived successfully.',
//                 'success',
//                 'Success'
//             );

//             setTimeout(() => location.reload(), 1000);
//         } else {
//             showAlertDialog(
//                 result.message || 'Unable to archive folder.',
//                 'error',
//                 'Failed'
//             );
//             if (submitBtn) submitBtn.disabled = false;
//         }
//     } catch (error) {
//         console.error('Archive error:', error);
//         showAlertDialog(
//             'Something went wrong while archiving.',
//             'error',
//             'Error'
//         );
//         if (submitBtn) submitBtn.disabled = false;
//     }
// });




// ═══════════════ MOVE FILE MODAL: search + select ═══════════════
document.addEventListener('input', function (e) {
    if (e.target.id !== 'folderSearchInput') return;

    const term = e.target.value.trim().toLowerCase();
    document.querySelectorAll('#folderList .folder-row').forEach(row => {
        const name = row.querySelector('.folder-name').textContent.toLowerCase();
        row.style.display = name.includes(term) ? '' : 'none';
    });
});

document.addEventListener('click', function (e) {
    const row = e.target.closest('#folderList .folder-row');
    if (!row) return;

    document.querySelectorAll('#folderList .folder-row').forEach(r => {
        r.classList.remove('folder-row-selected');
        r.querySelector('.folder-icon').classList.remove('folder-icon-active');
    });
    row.classList.add('folder-row-selected');
    row.querySelector('.folder-icon').classList.add('folder-icon-active');
});

// ═══════════════ MOVE FILE MODAL: confirm move ═══════════════
document.addEventListener('click', async function (e) {
    const btn = e.target.closest('#moveFileConfirmBtn');
    if (!btn) return;

    const selectedRadio = document.querySelector('input[name="destinationFolder"]:checked');
    if (!selectedRadio) {
        showAlertDialog('Please select a folder.', 'error');
        return;
    }

    const fileId = btn.dataset.fileId;
    const folderId = selectedRadio.value;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    btn.disabled = true;

    try {
        const response = await fetch('/MediaAndDocuments/MoveFile', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ fileId, folderId, __RequestVerificationToken: token })
        });

        const result = await response.json();

        if (result.success) {
            bootstrap.Modal.getInstance(document.getElementById('appModal'))?.hide();
            showAlertDialog(result.message || 'File moved successfully.', 'success');
            setTimeout(() => location.reload(), 1000);
        } else {
            showAlertDialog(result.message || 'Unable to move file.', 'error');
            btn.disabled = false;
        }
    } catch (err) {
        console.error(err);
        showAlertDialog('Something went wrong. Please try again.', 'error');
        btn.disabled = false;
    }
});

async function refreshPendingFilesCount() {

    const countElement =
        document.getElementById("pendingFilesCount");

    if (!countElement) {
        return;
    }

    try {

        const response =
            await fetch("/MediaAndDocuments/PendingFilesCount");

        if (!response.ok) {
            console.error("Unable to get pending file count.");
            return;
        }

        const data = await response.json();

        countElement.textContent = data.count;

        if (data.count > 0) {
            countElement.classList.remove("d-none");
        }
        else {
            countElement.classList.add("d-none");
        }

    }
    catch (error) {

        console.error(
            "Pending file count error:",
            error
        );
    }
}



// =====================================================
// SEARCH FILES
// =====================================================
document.addEventListener("input", function (e) {

    // Only run this code for the file search box
    if (e.target.id !== "fileSearchInput") {
        return;
    }

    const searchText =
        e.target.value
            .toLowerCase()
            .trim();

    const rows =
        document.querySelectorAll(
            "#fileTableBody tr[data-id]"
        );

    rows.forEach(function (row) {

        // Get all visible text from the row
        const rowText =
            row.textContent
                .toLowerCase();

        // Check if the row contains the search text
        const matches =
            rowText.includes(searchText);

        // Show or hide the row
        row.style.display =
            matches ? "" : "none";
    });
});