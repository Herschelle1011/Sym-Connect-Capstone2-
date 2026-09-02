document.addEventListener('submit', async function (e) {

    // Only handle forms that have data-modal-body
    if (!e.target.hasAttribute('data-modal-body')) return;

    e.preventDefault();

    const form = e.target;

    // Prevent another submit while this one is still processing
    if (form.dataset.submitting === "true") return;

    form.dataset.submitting = "true";

    const formData = new FormData(form);

    // Read which modal body should be updated
    const modalBodySelector = form.getAttribute('data-modal-body');
    const modalBody = document.querySelector(modalBodySelector);

    // Show loading screen
    showLoading();

    try {

        const response = await fetch(form.action, {
            method: 'POST',
            body: formData
        });

        if (response.redirected) {
            window.location.href = response.url;
            return;
        }

        if (response.ok) {

            const html = await response.text();

            if (modalBody) {
                modalBody.innerHTML = html;
            }

        } else {

            console.error("Request failed:", response.status);

        }

    }
    catch (error) {

        console.error("Submit error:", error);

    }
    finally {

        // Allow submitting again
        form.dataset.submitting = "false";

        // Remove loading screen
        hideLoading();
    }
});