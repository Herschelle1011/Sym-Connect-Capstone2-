// ============================================================
// CREATE — Photo Upload
// ============================================================
var selectedFiles = [];
var dropArea = document.getElementById('photoDropArea');
var fileInput = document.getElementById('facilityPhotos');
var previewArea = document.getElementById('photoPreviewArea');
var errorBox = document.getElementById('photoError');
var countBox = document.getElementById('photoCount');

if (dropArea && fileInput) {

    dropArea.addEventListener('click', function () {
        fileInput.click();
    });

    fileInput.addEventListener('change', function () {
        var newFiles = Array.from(this.files);
        if (selectedFiles.length + newFiles.length > 5) {
            showError('You can only upload up to 5 photos total.');
            this.value = '';
            return;
        }
        newFiles.forEach(function (file) { selectedFiles.push(file); });
        this.value = '';
        clearError();
        renderPreviews();
        updateCount();
    });

    function renderPreviews() {
        previewArea.innerHTML = '';
        selectedFiles.forEach(function (file, index) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var wrapper = document.createElement('div');
                wrapper.style.cssText = 'position:relative; width:80px; height:80px;';
                var img = document.createElement('img');
                img.src = e.target.result;
                img.style.cssText = 'width:80px; height:80px; object-fit:cover; border-radius:8px; border:1px solid #e5e7eb;';
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.textContent = '✕';
                btn.style.cssText = `position:absolute; top:-6px; right:-6px;
                    width:20px; height:20px; border-radius:50%;
                    background:#ef4444; color:white; border:none;
                    font-size:0.65rem; cursor:pointer;
                    display:flex; align-items:center;
                    justify-content:center; line-height:1; padding:0;`;
                btn.addEventListener('click', function () {
                    selectedFiles.splice(index, 1);
                    renderPreviews(); updateCount(); clearError();
                });
                wrapper.appendChild(img);
                wrapper.appendChild(btn);
                previewArea.appendChild(wrapper);
            };
            reader.readAsDataURL(file);
        });
        syncFilesToInput();
    }

    function syncFilesToInput() {
        var dt = new DataTransfer();
        selectedFiles.forEach(function (file) { dt.items.add(file); });
        fileInput.files = dt.files;
    }

    function updateCount() {
        countBox.textContent = selectedFiles.length > 0
            ? selectedFiles.length + ' of 5 photo(s) selected' : '';
    }

    function showError(msg) { errorBox.textContent = msg; errorBox.style.display = 'block'; }
    function clearError() { errorBox.textContent = ''; errorBox.style.display = 'none'; }

    var createForm = document.getElementById('createFacilityForm');
    if (createForm) {
        createForm.addEventListener('submit', function (e) {
            if (selectedFiles.length > 5) { e.preventDefault(); showError('Maximum is 5 photos.'); }
        });
    }
}


// ============================================================
// EDIT — Called after modal content is injected into the DOM
// ============================================================
function removeExistingPhoto(photoId) {
    // 👈 Moved here from the partial
    var photoDiv = document.getElementById('photo-' + photoId);
    var hiddenInput = document.getElementById('remove-' + photoId);
    if (photoDiv) photoDiv.style.display = 'none';
    if (hiddenInput) hiddenInput.disabled = false;  // enables it so form submits it
}

function initEditPhotoUpload() {
    var editSelectedFiles = [];

    var dropArea = document.getElementById('editPhotoDropArea');
    var fileInput = document.getElementById('editFacilityPhotos');
    var previewArea = document.getElementById('editPhotoPreviewArea');
    var errorBox = document.getElementById('editPhotoError');
    var countBox = document.getElementById('editPhotoCount');
    var form = document.getElementById('editFacilityForm');

    // Guard — if modal isn't loaded yet, do nothing
    if (!dropArea || !fileInput || !form) return;

    dropArea.addEventListener('click', function () {
        fileInput.click();
    });

    fileInput.addEventListener('change', function () {
        var newFiles = Array.from(this.files);
        if (editSelectedFiles.length + newFiles.length > 5) {
            showEditError('You can only upload up to 5 photos total.');
            this.value = '';
            return;
        }
        newFiles.forEach(function (file) { editSelectedFiles.push(file); });
        this.value = '';
        clearEditError();
        renderEditPreviews();
        updateEditCount();
    });

    function renderEditPreviews() {
        previewArea.innerHTML = '';
        editSelectedFiles.forEach(function (file, index) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var wrapper = document.createElement('div');
                wrapper.style.cssText = 'position:relative; width:80px; height:80px;';
                var img = document.createElement('img');
                img.src = e.target.result;
                img.style.cssText = 'width:80px; height:80px; object-fit:cover; border-radius:8px; border:1px solid #e5e7eb;';
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.textContent = '✕';
                btn.style.cssText = `position:absolute; top:-6px; right:-6px;
                    width:20px; height:20px; border-radius:50%;
                    background:#ef4444; color:white; border:none;
                    font-size:0.65rem; cursor:pointer;
                    display:flex; align-items:center;
                    justify-content:center; line-height:1; padding:0;`;
                btn.addEventListener('click', function () {
                    editSelectedFiles.splice(index, 1);
                    renderEditPreviews(); updateEditCount(); clearEditError();
                });
                wrapper.appendChild(img);
                wrapper.appendChild(btn);
                previewArea.appendChild(wrapper);
            };
            reader.readAsDataURL(file);
        });
        syncEditFilesToInput();
    }

    function syncEditFilesToInput() {
        var dt = new DataTransfer();
        editSelectedFiles.forEach(function (file) { dt.items.add(file); });
        fileInput.files = dt.files;
    }

    function updateEditCount() {
        countBox.textContent = editSelectedFiles.length > 0
            ? editSelectedFiles.length + ' of 5 photo(s) selected' : '';
    }

    function showEditError(msg) { errorBox.textContent = msg; errorBox.style.display = 'block'; }
    function clearEditError() { errorBox.textContent = ''; errorBox.style.display = 'none'; }

    form.addEventListener('submit', function (e) {
        if (editSelectedFiles.length > 5) {
            e.preventDefault();
            showEditError('Maximum is 5 photos.');
        }
    });
}