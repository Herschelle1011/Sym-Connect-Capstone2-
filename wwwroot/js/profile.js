const input = document.getElementById('profilePicture');
const dropZone = document.getElementById('dropZone');
const fileInfoBar = document.getElementById('fileInfoBar');
const fileName = document.getElementById('fileName');
const fileSize = document.getElementById('fileSize');
const clearBtn = document.getElementById('clearFile');
const submitBtn = document.getElementById('submitBtn');
const preview = document.getElementById('avatarPreview');
const placeholder = document.getElementById('avatarPlaceholder');
const avatarRing = document.getElementById('avatarRing');
const validMsg = document.getElementById('validationMsg');
const MAX_MB = 5;
const ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];

function formatBytes(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
}

function showValidation(msg) {
    validMsg.textContent = msg;
    validMsg.classList.add('visible');
    submitBtn.disabled = true;
}

function clearValidation() {
    validMsg.textContent = '';
    validMsg.classList.remove('visible');
}

function handleFile(file) {
    clearValidation();
    if (!file) return;

    if (!ALLOWED.includes(file.type)) {
        showValidation('Unsupported format. Please use JPEG, PNG, or WebP.');
        return;
    }

    if (file.size > MAX_MB * 1024 * 1024) {
        showValidation(`File is too large (${formatBytes(file.size)}). Maximum allowed is ${MAX_MB} MB.`);
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        const img = new Image();
        img.onload = function () {
            if (img.width < 100 || img.height < 100) {
                showValidation(`Image too small (${img.width}×${img.height}px). Minimum is 100×100 px.`);
                return;
            }
            preview.src = e.target.result;
            preview.classList.add('visible');
            placeholder.style.display = 'none';
            avatarRing.classList.add('has-image');
            fileName.textContent = file.name;
            fileSize.textContent = `${formatBytes(file.size)} · ${img.width}×${img.height}px · ${file.type.split('/')[1].toUpperCase()}`;
            fileInfoBar.classList.add('visible');
            submitBtn.disabled = false;
        };
        img.src = e.target.result;
    };
    reader.readAsDataURL(file);
}

// ✅ GUARD: only wire up this feature's listeners if its elements exist on the page
if (input && dropZone) {
    input.addEventListener('change', () => {
        if (input.files && input.files[0]) handleFile(input.files[0]);
    });

    if (clearBtn) {
        clearBtn.addEventListener('click', () => {
            input.value = '';
            preview.src = '#';
            preview.classList.remove('visible');
            placeholder.style.display = 'flex';
            avatarRing.classList.remove('has-image');
            fileInfoBar.classList.remove('visible');
            submitBtn.disabled = true;
            clearValidation();
        });
    }

    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-over');
    });
    dropZone.addEventListener('dragleave', () => dropZone.classList.remove('drag-over'));
    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
        const file = e.dataTransfer.files[0];
        if (file) {
            const dt = new DataTransfer();
            dt.items.add(file);
            input.files = dt.files;
            handleFile(file);
        }
    });
}

// ==========================================
// CHANGE PROFILE PHOTO — this now actually runs
// ==========================================
document.addEventListener("click", function (event) {
    const changePhotoButton = event.target.closest("#changePhotoButton");
    if (!changePhotoButton) return;

    const input = document.getElementById("profilePictureInput");
    if (input) input.click();
});

// ==========================================
// PROFILE PHOTO PREVIEW
// ==========================================
document.addEventListener("change", function (event) {
    if (event.target.id !== "profilePictureInput") return;

    const input = event.target;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];

    const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
    if (!allowedTypes.includes(file.type)) {
        alert("Please select a JPG, PNG, or WEBP image.");
        input.value = "";
        return;
    }

    const preview = document.getElementById("profilePhotoPreview");
    const placeholder = document.getElementById("profilePhotoPlaceholder");
    if (!preview) return;

    const imageUrl = URL.createObjectURL(file);
    preview.src = imageUrl;
    preview.classList.remove("d-none");

    if (placeholder) {
        placeholder.classList.remove("d-flex");
        placeholder.classList.add("d-none");
    }
});


document.addEventListener("click", function (e) {

    const toggleButton =
        e.target.closest(".password-toggle");

    if (!toggleButton) {
        return;
    }

    const targetId =
        toggleButton.dataset.target;

    const passwordInput =
        document.getElementById(targetId);

    if (!passwordInput) {
        return;
    }

    const icon =
        toggleButton.querySelector("i");

    // PASSWORD IS CURRENTLY HIDDEN
    if (passwordInput.type === "password") {

        passwordInput.type = "text";

        icon.classList.remove("bi-eye");
        icon.classList.add("bi-eye-slash");

        toggleButton.setAttribute(
            "aria-label",
            "Hide password"
        );
    }

    // PASSWORD IS CURRENTLY VISIBLE
    else {

        passwordInput.type = "password";

        icon.classList.remove("bi-eye-slash");
        icon.classList.add("bi-eye");

        toggleButton.setAttribute(
            "aria-label",
            "Show password"
        );
    }
});