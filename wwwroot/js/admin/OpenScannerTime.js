document.addEventListener("submit", async function (e) {
    if (e.target.id !== "createEventForm") return;
    e.preventDefault();

    const form = e.target;

    const startTime = form.querySelector('[name="StartTime"]').value;
    const endTime = form.querySelector('[name="EndTime"]').value;
   



    function subtractMinutes(timeStr, minutes) {
        if (!timeStr) return "";
        const [h, m] = timeStr.split(":").map(Number);
        const date = new Date(2000, 0, 1, h, m - minutes);
        return date.toTimeString().slice(0, 5);
    }

    const defaultTimeIn = subtractMinutes(startTime, 30);
    const defaultTimeOut = subtractMinutes(endTime, 30);

    const html = await fetch('/Events/GetScannerTimeModal').then(r => r.text());
    document.getElementById('appModalBody').innerHTML = html;
    document.getElementById('appModalLabel').innerText = "Scanner Times";

    document.getElementById('scannerTimeInInput').value = defaultTimeIn;
    document.getElementById('scannerTimeOutInput').value = defaultTimeOut;

    // hide the offcanvas before showing the modal
    const offcanvasEl = document.getElementById('eventPanel');
    const offcanvasInstance = bootstrap.Offcanvas.getInstance(offcanvasEl);
    if (offcanvasInstance) offcanvasInstance.hide();

    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('appModal'));
    modal.show();

    document.getElementById('confirmScannerTimesBtn').onclick = async function () {
        const timeIn = document.getElementById('scannerTimeInInput').value;
        const timeOut = document.getElementById('scannerTimeOutInput').value;
        const isAllDay = document.getElementById('isAllDayToggle').checked;

        if (!isAllDay) {
            if (timeIn > startTime) {
                showToast("Time-In scanner cannot open after the event starts.", false);
                return;
            }
            if (timeOut > endTime) {
                showToast("Time-Out scanner cannot open after the event ends.", false);
                return;
            }
            if (timeOut < startTime) {
                showToast("Time-Out scanner cannot open before the event starts.", false);
                return;
            }
        }

        const formData = new FormData(form);
        formData.append("TimeInScannerOpenTime", document.getElementById('scannerTimeInInput').value);
        formData.append("TimeOutScannerOpenTime", document.getElementById('scannerTimeOutInput').value);

        const response = await fetch(form.action, {
            method: "POST",
            body: formData
        });

        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
            const result = await response.json();
            modal.hide();
            showToast(result.message, result.success);
            if (result.success) setTimeout(() => location.reload(), 900);
        } else {
            document.getElementById("appModalBody").innerHTML = await response.text();
        }
    };

    // Back button re-opens the existing offcanvas instead of fetching a blank form
    document.getElementById('backToEventFormBtn').onclick = function () {
        modal.hide();
        const panel = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
        panel.show();
    };
});