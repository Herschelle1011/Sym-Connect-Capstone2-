
function initCreateEventPanel() {
    const isAllDayToggle = document.getElementById("isAllDayToggle");
    const startTimeGroup = document.getElementById("startTimeGroup");
    const endTimeGroup = document.getElementById("endTimeGroup");

    if (!isAllDayToggle) return; // panel not loaded yet, skip safely

    function toggleTimeInputs() {
        const hide = isAllDayToggle.checked;
        startTimeGroup.style.display = hide ? "none" : "block";
        endTimeGroup.style.display = hide ? "none" : "block";
    }

    isAllDayToggle.addEventListener("change", toggleTimeInputs);
    toggleTimeInputs();
}