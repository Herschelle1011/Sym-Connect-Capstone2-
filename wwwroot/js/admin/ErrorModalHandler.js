document.addEventListener('submit', async function (e) {

    // 🎯 Only handle forms that have the data-modal-body attribute
    // This is your "opt-in" — forms without it behave normally
    if (!e.target.hasAttribute('data-modal-body')) return;

    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);

    // Read the attribute to know which modal body to update
    const modalBodySelector = form.getAttribute('data-modal-body');
    const modalBody = document.querySelector(modalBodySelector);

    const response = await fetch(form.action, {
        method: 'POST',
        body: formData
    });

    if (response.ok) {
        const html = await response.text();

        if (response.redirected) {
            // ✅ Success — go to the redirect URL
            window.location.href = response.url;
        } else {
            // ❌ Validation error — update THIS form's modal body
            modalBody.innerHTML = html;
        }
    }
});