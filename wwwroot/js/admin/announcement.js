document.addEventListener('DOMContentLoaded', () => {

    // =========================================================
    // NEW ANNOUNCEMENT
    // =========================================================
    const newAnnouncementBtn =
        document.getElementById('btnNewAnnouncement');

    if (newAnnouncementBtn) {
        newAnnouncementBtn.addEventListener('click', async () => {
            await loadAnnouncementModal(
                'New Announcement',
                '/Announcements/Create'
            );
        });
    }


    // =========================================================
    // EDIT + ARCHIVE
    // =========================================================
    const tableBody =
        document.getElementById('announcementTableBody');

    if (tableBody) {
        tableBody.addEventListener('click', async (e) => {
            const editBtn =
                e.target.closest('.btnEditAnnouncement');
            const archiveBtn =
                e.target.closest('.btnArchiveAnnouncement');

            // EDIT
            if (editBtn) {
                const id = editBtn.dataset.id;
                await loadAnnouncementModal(
                    'Edit Announcement',
                    `/Announcements/Edit/${id}`
                );
                return;
            }

            // ARCHIVE
            if (archiveBtn) {
                const id = archiveBtn.dataset.id;
                await loadAnnouncementModal(
                    'Archive Announcement',
                    `/Announcements/Archive/${id}`
                );
                return;
            }
        });
    }


    // =========================================================
    // LOAD PARTIAL INTO GLOBAL _Modal.cshtml
    // =========================================================
    async function loadAnnouncementModal(title, url) {
        try {
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(
                    `Unable to load modal: ${response.status}`
                );
            }

            const html = await response.text();

            // Global _Modal.cshtml elements
            const modalElement =
                document.getElementById('appModal');
            const modalBody =
                document.getElementById('appModalBody');
            const modalTitle =
                document.getElementById('appModalLabel');

            modalTitle.textContent = title;
            modalBody.innerHTML = html;

            setAppModalSize(title === 'Archive Announcement' ? null : 'md');


            // Bootstrap Modal
            const modal =
                bootstrap.Modal.getOrCreateInstance(modalElement);
            modal.show();

            // Initialize fields depending on loaded form
            bindConditionalFields();
            bindAnnouncementFormSubmit();

            // Reinitialize ASP.NET validation
            if (window.jQuery && $.validator) {
                $.validator.unobtrusive.parse(
                    '#appModalBody'
                );
            }
        }
        catch (error) {
            console.error(
                'Announcement modal error:',
                error
            );
        }
    }


    // =========================================================
    // CONDITIONAL FIELDS
    // =========================================================
    function bindConditionalFields() {
        const audienceRadios =
            document.querySelectorAll(
                'input[name="TargetAudience"]'
            );
        const groupField =
            document.getElementById('groupField');
        const publishSelect =
            document.getElementById('publishOptionSelect');
        const scheduleField =
            document.getElementById('scheduleField');
        const expirationSelect =
            document.getElementById(
                'expirationOptionSelect'
            );
        const customExpirationField =
            document.getElementById(
                'customExpirationField'
            );

        // This function can also run for Archive modal,
        // where these fields do not exist.
        if (
            !groupField ||
            !publishSelect ||
            !scheduleField ||
            !expirationSelect ||
            !customExpirationField
        ) {
            return;
        }

        function toggleGroupField() {
            const checked =
                document.querySelector(
                    'input[name="TargetAudience"]:checked'
                );
            groupField.style.display =
                checked &&
                    checked.value === 'SpecificGroup'
                    ? 'block'
                    : 'none';
        }

        function toggleScheduleField() {
            scheduleField.style.display =
                publishSelect.value === 'Scheduled'
                    ? 'block'
                    : 'none';
        }

        function toggleCustomExpirationField() {
            customExpirationField.style.display =
                expirationSelect.value === 'Custom'
                    ? 'block'
                    : 'none';
        }

        audienceRadios.forEach(radio => {
            radio.addEventListener(
                'change',
                toggleGroupField
            );
        });

        publishSelect.addEventListener(
            'change',
            toggleScheduleField
        );

        expirationSelect.addEventListener(
            'change',
            toggleCustomExpirationField
        );

        // Initial state
        toggleGroupField();
        toggleScheduleField();
        toggleCustomExpirationField();
    }


    // =========================================================
    // CLIENT VALIDATION
    // =========================================================
    function validateAnnouncementForm() {
        const errors = [];
        clearFieldErrors();

        const targetAudience =
            document.querySelector(
                'input[name="TargetAudience"]:checked'
            )?.value;

        const groupId =
            document.getElementById('GroupId')?.value;

        if (
            targetAudience === 'SpecificGroup' &&
            !groupId
        ) {
            errors.push({
                field: 'GroupId',
                message: 'Please select a group.'
            });
        }

        const publishSelect =
            document.getElementById(
                'publishOptionSelect'
            );

        // Archive modal does not have these fields
        if (!publishSelect) {
            return errors;
        }

        const publishOption =
            publishSelect.value;

        const publishDateInput =
            document.getElementById('PublishDate');

        const publishDateVal =
            publishDateInput
                ? publishDateInput.value
                : null;

        if (publishOption === 'Scheduled') {
            if (!publishDateVal) {
                errors.push({
                    field: 'PublishDate',
                    message:
                        'Please pick a schedule date and time.'
                });
            }
            else if (
                new Date(publishDateVal) <= new Date()
            ) {
                errors.push({
                    field: 'PublishDate',
                    message:
                        'Scheduled date must be in the future.'
                });
            }
        }

        const expirationSelect =
            document.getElementById(
                'expirationOptionSelect'
            );

        if (!expirationSelect) {
            return errors;
        }

        const expirationOption =
            expirationSelect.value;

        const customExpInput =
            document.getElementById(
                'CustomExpirationDate'
            );

        const customExpVal =
            customExpInput
                ? customExpInput.value
                : null;

        if (expirationOption === 'Custom') {
            if (!customExpVal) {
                errors.push({
                    field: 'CustomExpirationDate',
                    message:
                        'Please pick a custom expiration date.'
                });
            }
            else {
                const effectivePublish =
                    publishOption === 'Scheduled' &&
                        publishDateVal
                        ? new Date(publishDateVal)
                        : new Date();

                if (
                    new Date(customExpVal)
                    <= effectivePublish
                ) {
                    errors.push({
                        field: 'CustomExpirationDate',
                        message:
                            'Expiration must be after the publish date.'
                    });
                }
            }
        }

        return errors;
    }


    function clearFieldErrors() {
        document
            .querySelectorAll('.field-error')
            .forEach(element => {
                element.textContent = '';
            });
    }


    function showFieldErrors(errors) {
        errors.forEach(error => {
            const span =
                document.querySelector(
                    `span[data-valmsg-for="${error.field}"]`
                );
            if (span) {
                span.textContent = error.message;
            }
        });
    }


    // =========================================================
    // CREATE + EDIT FORM
    // =========================================================
    function bindAnnouncementFormSubmit() {
        const form =
            document.getElementById(
                'announcementForm'
            );

        if (!form) {
            return;
        }

        form.addEventListener(
            'submit',
            async function (e) {
                e.preventDefault();

                const errors =
                    validateAnnouncementForm();

                if (errors.length > 0) {
                    showFieldErrors(errors);
                    return;
                }

                const submitBtn =
                    form.querySelector(
                        'button[type="submit"]'
                    );

                if (submitBtn) {
                    submitBtn.disabled = true;
                }

                try {
                    const response =
                        await fetch(
                            form.action,
                            {
                                method: 'POST',
                                body: new FormData(form)
                            }
                        );

                    const contentType =
                        response.headers.get(
                            'content-type'
                        ) || '';

                    // CREATE / EDIT SUCCESS
                    if (
                        contentType.includes(
                            'application/json'
                        )
                    ) {
                        const result =
                            await response.json();

                        if (result.success) {
                            closeAnnouncementModal();
                            showAlertDialog(
                                result.message ||
                                'Announcement saved successfully.',
                                'success',
                                'Success'
                            );

                            setTimeout(() => {
                                location.reload();
                            }, 1000);
                        }
                        else {
                            showAlertDialog(
                                result.message ||
                                'Unable to save announcement.',
                                'error',
                                'Failed'
                            );

                            if (submitBtn) {
                                submitBtn.disabled = false;
                            }
                        }
                    }

                    // VALIDATION FAILED
                    else {
                        const html =
                            await response.text();

                        document.getElementById(
                            'appModalBody'
                        ).innerHTML = html;

                        bindConditionalFields();
                        bindAnnouncementFormSubmit();

                        if (
                            window.jQuery &&
                            $.validator
                        ) {
                            $.validator
                                .unobtrusive
                                .parse(
                                    '#appModalBody'
                                );
                        }
                    }
                }
                catch (error) {
                    console.error(error);
                    showAlertDialog(
                        'Something went wrong.',
                        'error',
                        'Error'
                    );

                    if (submitBtn) {
                        submitBtn.disabled = false;
                    }
                }
            }
        );
    }


    // =========================================================
    // ARCHIVE FORM
    // Uses event delegation because archiveForm
    // is loaded dynamically
    // =========================================================
    document.addEventListener(
        'submit',
        async function (e) {
            if (e.target.id !== 'archiveForm') {
                return;
            }

            e.preventDefault();

            const form = e.target;

            const submitBtn =
                form.querySelector(
                    'button[type="submit"]'
                );

            if (submitBtn) {
                submitBtn.disabled = true;
            }

            try {
                const response =
                    await fetch(
                        form.action,
                        {
                            method: 'POST',
                            body: new FormData(form)
                        }
                    );

                const result =
                    await response.json();

                if (result.success) {
                    closeAnnouncementModal();

                    showAlertDialog(
                        result.message ||
                        'Announcement archived successfully.',
                        'success',
                        'Success'
                    );

                    setTimeout(() => {
                        location.reload();
                    }, 1000);
                }
                else {
                    showAlertDialog(
                        result.message ||
                        'Unable to archive announcement.',
                        'error',
                        'Failed'
                    );

                    if (submitBtn) {
                        submitBtn.disabled = false;
                    }
                }
            }
            catch (error) {
                console.error(
                    'Archive error:',
                    error
                );

                showAlertDialog(
                    'Something went wrong while archiving.',
                    'error',
                    'Error'
                );

                if (submitBtn) {
                    submitBtn.disabled = false;
                }
            }
        }
    );


    // =========================================================
    // CLOSE GLOBAL APP MODAL
    // =========================================================
    function closeAnnouncementModal() {
        const modalElement =
            document.getElementById('appModal');

        const modal =
            bootstrap.Modal.getInstance(
                modalElement
            );

        if (modal) {
            modal.hide();
        }
    }

});


// =============================================================
// NEW: CARD LAYOUT INTERACTIONS
// (All/Pinned tabs, audience filter, search, dropdown menu,
// show more/less). Kept in a separate listener so none of the
// code above needed to be touched.
// =============================================================
document.addEventListener('DOMContentLoaded', () => {

    const cardList = document.getElementById('announcementTableBody');
    if (!cardList) {
        return;
    }

    const cards = () => Array.from(cardList.querySelectorAll('.ann-card'));

    let activeTab = 'all';
    let audienceValue = '';
    let searchValue = '';

    function applyFilters() {
        cards().forEach(card => {
            const isPinned = card.dataset.pinned === 'true';
            const audience = card.dataset.audience || '';
            const text = card.textContent.toLowerCase();

            const matchesTab =
                activeTab === 'all' || (activeTab === 'pinned' && isPinned);

            const matchesAudience =
                !audienceValue || audience === audienceValue;

            const matchesSearch =
                !searchValue || text.includes(searchValue);

            card.style.display =
                (matchesTab && matchesAudience && matchesSearch) ? '' : 'none';
        });
    }

    // ---------------------------------------------------------
    // ALL / PINNED TABS
    // ---------------------------------------------------------
    document.querySelectorAll('.ann-tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.ann-tab').forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            activeTab = tab.dataset.tab;
            applyFilters();
        });
    });

    // ---------------------------------------------------------
    // AUDIENCE FILTER
    // ---------------------------------------------------------
    const audienceFilter = document.getElementById('audienceFilter');
    if (audienceFilter) {
        audienceFilter.addEventListener('change', () => {
            audienceValue = audienceFilter.value;
            applyFilters();
        });
    }

    // ---------------------------------------------------------
    // SEARCH
    // ---------------------------------------------------------
    const searchInput = document.getElementById('announcementSearch');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            searchValue = searchInput.value.trim().toLowerCase();
            applyFilters();
        });
    }

    // ---------------------------------------------------------
    // THREE-DOT DROPDOWN (Edit / Delete)
    // ---------------------------------------------------------
    cardList.addEventListener('click', (e) => {
        const moreBtn = e.target.closest('.btn-more');

        // Close every open menu first
        document.querySelectorAll('.ann-dropdown-menu').forEach(menu => {
            if (!moreBtn || menu !== moreBtn.closest('.ann-card-menu-wrapper')?.querySelector('.ann-dropdown-menu')) {
                menu.hidden = true;
            }
        });

        if (!moreBtn) {
            return;
        }

        const menu = moreBtn.closest('.ann-card-menu-wrapper').querySelector('.ann-dropdown-menu');
        menu.hidden = !menu.hidden;
    });

    // Close dropdown when clicking anywhere outside a card menu
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.ann-card-menu-wrapper')) {
            document.querySelectorAll('.ann-dropdown-menu').forEach(menu => {
                menu.hidden = true;
            });
        }
    });

    // ---------------------------------------------------------
    // SHOW MORE / SHOW LESS
    // Only shown when the description actually overflows its
    // 3-line clamp — measured directly rather than guessed from
    // a character count, so it stays accurate at any font size.
    // ---------------------------------------------------------
    document.querySelectorAll('.ann-card-desc').forEach(desc => {
        const btn = cardList.querySelector(`.btn-show-more[data-target="${desc.id}"]`);
        if (!btn) {
            return;
        }

        if (desc.scrollHeight > desc.clientHeight + 1) {
            btn.hidden = false;
        }

        btn.addEventListener('click', () => {
            const expanded = desc.classList.toggle('expanded');
            btn.textContent = expanded ? 'Show less' : 'Show more';
        });
    });
});



