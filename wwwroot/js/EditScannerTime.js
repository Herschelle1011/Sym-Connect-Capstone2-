document.addEventListener("submit", async function (e) {

    if (e.target.id !== "editEventForm")
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
        document.getElementById("editIsAllDayToggle").checked;

    const startTimeInput = form.querySelector('[name="StartTime"]');
    const endTimeInput = form.querySelector('[name="EndTime"]');

    const startSpan = form.querySelector('[data-valmsg-for="StartTime"]');
    const endSpan = form.querySelector('[data-valmsg-for="EndTime"]');

    if (!isAllDay) {
        if (!startTimeInput.value) {
            startSpan.textContent = "Start Time is required.";
            hasError = true;
        } else {
            startSpan.textContent = "";
        }

        if (!endTimeInput.value) {
            endSpan.textContent = "End Time is required.";
            hasError = true;
        } else {
            endSpan.textContent = "";
        }
    } else {
        startSpan.textContent = "";
        endSpan.textContent = "";
    }

    const selectedFacilities = form.querySelectorAll(".facility-checkbox:checked");
    const facilitySpan = form.querySelector('[data-valmsg-for="SelectedFacilityIds"]');

    if (selectedFacilities.length === 0) {
        facilitySpan.textContent = "Select at least one facility.";
        hasError = true;
    } else {
        facilitySpan.textContent = "";
    }

    if (hasError) {
        return;
    }

    // ==============================
    // 3. EVERYTHING IS VALID
    // ==============================

    const startTime = startTimeInput.value;
    const endTime = endTimeInput.value;

    function addMinutes(timeStr, minutes) {
        if (!timeStr) return "";
        const [h, m] = timeStr.split(":").map(Number);
        const date = new Date(2000, 0, 1, h, m + minutes);
        return date.toTimeString().slice(0, 5);
    }

    function subtractMinutes(timeStr, minutes) {
        return addMinutes(timeStr, -minutes);
    }

    // ==============================
    // Existing saved values take priority over recomputed defaults.
    // The edit form partial must render these as data-* attributes
    // from the EditEventViewModel (see note below).
    // ==============================

    const savedTimeInOpen = form.dataset.timeInOpen;
    const savedTimeInClose = form.dataset.timeInClose;
    const savedTimeOutOpen = form.dataset.timeOutOpen;
    const savedTimeOutClose = form.dataset.timeOutClose;

    const fallbackTimeInOpen = subtractMinutes(startTime, 30);
    const fallbackTimeInClose = startTime;
    const fallbackTimeOutOpen = endTime;
    const fallbackTimeOutClose = addMinutes(endTime, 30);

    // Detect whether the user actually changed Start/End Time in this edit session.
    // If they did, the saved scanner offsets were computed for the OLD schedule
    // and are no longer trustworthy — force recompute from the NEW times instead.
    const originalStartTime = form.dataset.originalStartTime;
    const originalEndTime = form.dataset.originalEndTime;

    const startTimeChanged = startTime !== originalStartTime;
    const endTimeChanged = endTime !== originalEndTime;

    const timeInOpenValue = (savedTimeInOpen && !startTimeChanged) ? savedTimeInOpen : fallbackTimeInOpen;
    const timeInCloseValue = (savedTimeInClose && !startTimeChanged) ? savedTimeInClose : fallbackTimeInClose;
    const timeOutOpenValue = (savedTimeOutOpen && !endTimeChanged) ? savedTimeOutOpen : fallbackTimeOutOpen;
    const timeOutCloseValue = (savedTimeOutClose && !endTimeChanged) ? savedTimeOutClose : fallbackTimeOutClose;

    // ==============================
    // 4. LOAD SCANNER TIME MODAL
    // ==============================

    const html = await fetch('/Events/GetScannerEditTimeModal').then(r => r.text());

    document.getElementById('appModalBody').innerHTML = html;
    document.getElementById('appModalLabel').innerText = "Scanner Times";

    const allDayNotice = document.getElementById('allDayScannerNotice');
    const scannerFields = document.getElementById('scannerTimeFields');
    const defaultsHint = document.getElementById('scannerDefaultsHint');

    if (isAllDay) {
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

        document.getElementById('scannerTimeInOpenInput').value = timeInOpenValue;
        document.getElementById('scannerTimeInCloseInput').value = timeInCloseValue;
        document.getElementById('scannerTimeOutOpenInput').value = timeOutOpenValue;
        document.getElementById('scannerTimeOutCloseInput').value = timeOutCloseValue;
    }

    // "Reset to default" — explicitly recompute defaults, discarding saved values
    document.getElementById('resetScannerDefaultsBtn').onclick = function (evt) {
        evt.preventDefault();
        document.getElementById('scannerTimeInOpenInput').value = fallbackTimeInOpen;
        document.getElementById('scannerTimeInCloseInput').value = fallbackTimeInClose;
        document.getElementById('scannerTimeOutOpenInput').value = fallbackTimeOutOpen;
        document.getElementById('scannerTimeOutCloseInput').value = fallbackTimeOutClose;
    };

    // ==============================
    // 5. CLOSE EDIT PANEL
    // ==============================

    const offcanvasEl = document.getElementById('editEventPanel');
    const offcanvasInstance = bootstrap.Offcanvas.getInstance(offcanvasEl);

    if (offcanvasInstance) {
        offcanvasInstance.hide();
    }

    // ==============================
    // 6. OPEN SCANNER MODAL
    // ==============================

    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('appModal'));
    modal.show();



    document.getElementById('backToEventFormBtn').onclick = function () {
        modal.hide();
        const panel = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
        panel.show();
    };

    // ==============================
    // 7. CONFIRM SCANNER TIMES
    // ==============================

    document.getElementById('confirmScannerTimesBtn').onclick = async function () {

        let timeInOpen = "", timeInClose = "", timeOutOpen = "", timeOutClose = "";

        if (!isAllDay) {

            timeInOpen = document.getElementById('scannerTimeInOpenInput').value;
            timeInClose = document.getElementById('scannerTimeInCloseInput').value;
            timeOutOpen = document.getElementById('scannerTimeOutOpenInput').value;
            timeOutClose = document.getElementById('scannerTimeOutCloseInput').value;

            if (timeInOpen > startTime) {
                showAlertDialog(
                    "Time-In scanner cannot open after the event starts.",
                    "Invalid Scanner Time"
                );
                return;
            }
      

            if (timeInClose && timeInOpen > timeInClose) {
                showAlertDialog(
                    "Time-In scanner cannot close before it opens.",
                    "Invalid Scanner Time"
                );
                return;
            }

            if (timeOutOpen < endTime) {
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

            if (timeOutClose && timeOutOpen > timeOutClose) {
                showAlertDialog(
                    "Time-Out scanner cannot close before it opens.",
                    "Invalid Scanner Time"
                );
                return;
            }
        }

        const formData = new FormData(form);
        formData.append("TimeInScannerOpenTime", timeInOpen);
        formData.append("TimeInScannerClosesTime", timeInClose);
        formData.append("TimeOutScannerOpenTime", timeOutOpen);
        formData.append("TimeOutScannerClosesTime", timeOutClose);

        const response = await fetch(form.action, {
            method: "POST",
            body: formData
        });

        const contentType = response.headers.get("content-type");


        if (contentType && contentType.includes("application/json")) {

            const result = await response.json();

            if (result.success) {

                modal.hide();

                showAlertDialog(
                    result.message,
                    "Event Updated"
                );

                setTimeout(() => {
                    location.reload();
                }, 1200);

            } else {

                showAlertDialog(
                    result.message,
                    "Unable to Update Event"
                );
            }
        }

    }
 });

