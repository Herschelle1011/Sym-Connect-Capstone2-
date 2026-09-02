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