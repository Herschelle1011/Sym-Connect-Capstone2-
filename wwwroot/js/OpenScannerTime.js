document.addEventListener("submit", async function (e) {

    if (e.target.id !== "createEventForm")
        return;

    e.preventDefault();

    const form = e.target;

    // ==============================
    // 1. NORMAL ASP.NET VALIDATION
    // ==============================

    if (!$(form).valid()) {
        return;
    }


    // ==============================
    // 2. CUSTOM VALIDATION
    // ==============================

    let hasError = false;

    const isAllDay =
        document.getElementById("isAllDayToggle").checked;

    const startTimeInput =
        form.querySelector('[name="StartTime"]');

    const endTimeInput =
        form.querySelector('[name="EndTime"]');


    // ------------------------------
    // TIME VALIDATION
    // ------------------------------

    const startSpan =
        form.querySelector('[data-valmsg-for="StartTime"]');

    const endSpan =
        form.querySelector('[data-valmsg-for="EndTime"]');


    if (!isAllDay) {

        if (!startTimeInput.value) {
            startSpan.textContent = "Start Time is required.";
            hasError = true;
        }
        else {
            startSpan.textContent = "";
        }


        if (!endTimeInput.value) {
            endSpan.textContent = "End Time is required.";
            hasError = true;
        }
        else {
            endSpan.textContent = "";
        }
    }
    else {
        // Clear old errors when All Day is checked
        startSpan.textContent = "";
        endSpan.textContent = "";
    }


    // ------------------------------
    // FACILITY VALIDATION
    // ------------------------------

    const selectedFacilities =
        form.querySelectorAll(".facility-checkbox:checked");

    const facilitySpan =
        form.querySelector(
            '[data-valmsg-for="SelectedFacilityIds"]'
        );


    if (selectedFacilities.length === 0) {

        facilitySpan.textContent =
            "Select at least one facility.";

        hasError = true;
    }
    else {
        facilitySpan.textContent = "";
    }


    // ==============================
    // 3. STOP IF CUSTOM ERRORS EXIST
    // ==============================

    if (hasError) {
        return;
    }


    // ==============================
    // 4. EVERYTHING IS VALID
    // ==============================

    const startTime = startTimeInput.value;
    const endTime = endTimeInput.value;


    function addMinutes(timeStr, minutes) {

        if (!timeStr)
            return "";

        const [h, m] =
            timeStr.split(":").map(Number);

        const date =
            new Date(2000, 0, 1, h, m + minutes);

        return date.toTimeString().slice(0, 5);
    }

    function subtractMinutes(timeStr, minutes) {
        return addMinutes(timeStr, -minutes);
    }


    // Opens: same as before
    const defaultTimeInOpen =
        subtractMinutes(startTime, 30);

    const defaultTimeOutOpen =
        endTime;

    // Closes: new defaults
    // Time-In closes once the event actually starts —
    // no point letting people "time in" after it's begun.
    const defaultTimeInClose =
        startTime;

    // Time-Out closes 30 min after the event ends —
    // grace window for people leaving late.
    const defaultTimeOutClose =
        addMinutes(endTime, 30);


    // ==============================
    // 5. LOAD SCANNER TIME MODAL
    // ==============================

    const scannerModalUrl =
        form.dataset.scannerModalUrl;

    if (!scannerModalUrl) {
        console.error("Scanner modal URL is missing.");

        showAlertDialog(
            "Unable to load scanner settings.",
            "Error"
        );

        return;
    }

    const scannerResponse =
        await fetch(scannerModalUrl);

    if (!scannerResponse.ok) {
        console.error(
            "Scanner modal request failed:",
            scannerResponse.status
        );

        showAlertDialog(
            "Unable to load scanner settings.",
            "Error"
        );

        return;
    }

    const html =
        await scannerResponse.text();


    document.getElementById('appModalBody').innerHTML =
        html;

    document.getElementById('appModalLabel').innerText =
        "Scanner Times";



    const allDayNotice = document.getElementById('allDayScannerNotice');
    const scannerFields = document.getElementById('scannerTimeFields');
    const defaultsHint = document.getElementById('scannerDefaultsHint');

    if (
        !allDayNotice ||
        !scannerFields ||
        !defaultsHint
    ) {
        console.error(
            "Required scanner elements are missing:",
            {
                allDayNotice,
                scannerFields,
                defaultsHint
            }
        );

        showAlertDialog(
            "Scanner time form could not be loaded correctly.",
            "Error"
        );

        return;
    }

    if (isAllDay) {
        // All-day: hide the time inputs, show the notice, skip defaults entirely
        allDayNotice.classList.remove('d-none');
        allDayNotice.classList.add('d-flex');
        scannerFields.classList.add('d-none');
        defaultsHint.classList.add('d-none');
    }
    else {
        allDayNotice.classList.add('d-none');
        allDayNotice.classList.remove('d-flex');
        scannerFields.classList.remove('d-none');
        defaultsHint.classList.remove('d-none');

        document.getElementById('scannerTimeInOpenInput').value = defaultTimeInOpen;
        document.getElementById('scannerTimeInCloseInput').value = defaultTimeInClose;
        document.getElementById('scannerTimeOutOpenInput').value = defaultTimeOutOpen;
        document.getElementById('scannerTimeOutCloseInput').value = defaultTimeOutClose;
    }



    // ==============================
    // 6. CLOSE CREATE EVENT PANEL
    // ==============================

    const offcanvasEl =
        document.getElementById('eventPanel');

    const offcanvasInstance =
        bootstrap.Offcanvas.getInstance(offcanvasEl);

    if (offcanvasInstance) {
        offcanvasInstance.hide();
    }


    // ==============================
    // 7. OPEN SCANNER MODAL
    // ==============================

    const modal =
        bootstrap.Modal.getOrCreateInstance(
            document.getElementById('appModal')
        );

    modal.show();


    // ==============================
    // 8. CONFIRM SCANNER TIMES
    // ==============================

    document.getElementById(
        'confirmScannerTimesBtn'
    ).onclick = async function () {

        const timeInOpen =
            document.getElementById('scannerTimeInOpenInput').value;

        const timeInClose =
            document.getElementById('scannerTimeInCloseInput').value;

        const timeOutOpen =
            document.getElementById('scannerTimeOutOpenInput').value;

        const timeOutClose =
            document.getElementById('scannerTimeOutCloseInput').value;


        if (!isAllDay) {

            if (timeInOpen >= timeInClose) {
                showAlertDialog(
                    "Time-In scanner closing time must be after its opening time.",
                    "Invalid Scanner Time"
                );
                return;
            }

            if (timeInClose > timeOutOpen) {
                showAlertDialog(
                    "Time-In scanner must close before the Time-Out scanner opens.",
                    "Invalid Scanner Time"
                );
                return;
            }

            if (timeOutOpen >= timeOutClose) {
                showAlertDialog(
                    "Time-Out scanner closing time must be after its opening time.",
                    "Invalid Scanner Time"
                );
                return;
            }

            if (timeOutOpen > endTime) {
                showAlertDialog(
                    "Time-Out scanner cannot open after the event ends.",
                    "Invalid Scanner Time"
                );
                return;
            }

            if (timeOutOpen < startTime) {
                showAlertDialog(
                    "Time-Out scanner cannot open before the event starts.",
                    "Invalid Scanner Time"
                );
                return;
            }
        }


        // Build final form
        const formData =
            new FormData(form);


        formData.append("TimeInScannerOpenTime", timeInOpen);
        formData.append("TimeInScannerClosesTime", timeInClose);
        formData.append("TimeOutScannerOpenTime", timeOutOpen);
        formData.append("TimeOutScannerClosesTime", timeOutClose);


        const response =
            await fetch(form.action, {
                method: "POST",
                body: formData
            });


        const contentType =
            response.headers.get("content-type");

            if (
                contentType &&
                contentType.includes("application/json")
            ) {
                const result = await response.json();

                if (result.success) {

                    modal.hide();

                    showAlertDialog(
                        result.message,
                        "Event Created"
                    );

                    setTimeout(() => {
                        location.reload();
                    }, 1200);

                } else {

                    showAlertDialog(
                        result.message,
                        "Unable to Create Event"
                    );
                }
            }
            else {
                document.getElementById(
                    "appModalBody"
                ).innerHTML = await response.text();
            }

        }; 


    // ==============================
    // 9. BACK BUTTON
    // ==============================

    document.getElementById(
        'backToEventFormBtn'
    ).onclick = function () {

        modal.hide();

        const panel =
            bootstrap.Offcanvas.getOrCreateInstance(
                offcanvasEl
            );

        panel.show();
    };

});






