document.addEventListener("DOMContentLoaded", function () {

    console.log("Member points event details JS loaded");


    // =========================================
    // ELEMENTS
    // =========================================

    const activityItems =
        Array.from(document.querySelectorAll(".mp-item"));

    const activityCards =
        document.querySelectorAll(".point-activity-card");

    const filterButtons =
        document.querySelectorAll(".mp-filter");

    const searchInput =
        document.getElementById("mpSearch");

    const emptyMessage =
        document.getElementById("mpEmpty");

    const modalContainer =
        document.getElementById("pointDetailsModalContainer");


    let currentFilter = "all";


    // =========================================
    // FILTER + SEARCH
    // =========================================

    function applyFilters() {

        if (!searchInput) {
            return;
        }


        const searchTerm =
            searchInput.value
                .trim()
                .toLowerCase();


        let shown = 0;


        activityItems.forEach(function (item) {

            const itemStatus =
                item.dataset.status || "";

            const itemTitle =
                item.dataset.title || "";


            const matchesStatus =
                currentFilter === "all" ||
                itemStatus === currentFilter;


            const matchesSearch =
                searchTerm === "" ||
                itemTitle.includes(searchTerm);


            const shouldShow =
                matchesStatus &&
                matchesSearch;


            item.classList.toggle(
                "d-none",
                !shouldShow
            );


            if (shouldShow) {
                shown++;
            }

        });


        if (emptyMessage) {

            emptyMessage.classList.toggle(
                "d-none",
                shown > 0
            );

        }

    }


    // =========================================
    // FILTER BUTTON CLICK
    // =========================================

    filterButtons.forEach(function (button) {

        button.addEventListener(
            "click",
            function () {

                filterButtons.forEach(
                    function (btn) {

                        btn.classList.remove(
                            "is-active"
                        );

                    }
                );


                button.classList.add(
                    "is-active"
                );


                currentFilter =
                    button.dataset.filter || "all";


                applyFilters();

            }
        );

    });


    // =========================================
    // SEARCH
    // =========================================

    if (searchInput) {

        searchInput.addEventListener(
            "input",
            applyFilters
        );

    }


    // =========================================
    // OPEN POINT DETAILS
    // =========================================

    async function openPointDetails(card) {

        const attendanceId =
            card.dataset.attendanceId;


        console.log(
            "Clicked Attendance ID:",
            attendanceId
        );


        if (!attendanceId) {

            console.error(
                "Attendance ID was not found on the card."
            );

            return;
        }


        if (!modalContainer) {

            console.error(
                "pointDetailsModalContainer was not found."
            );

            return;
        }


        try {

            // =====================================
            // CALL MEMBER POINTS CONTROLLER
            // =====================================

            const response = await fetch(
                `/Member/Points/Details?attendanceId=${encodeURIComponent(attendanceId)}`,
                {
                    method: "GET",
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                }
            );


            console.log(
                "Details response:",
                response.status
            );


            if (!response.ok) {

                console.error(
                    "Unable to load point details."
                );

                return;
            }


            // Controller returns the HTML from:
            // _PointDetailsModal.cshtml

            const modalHtml =
                await response.text();


            // =====================================
            // PUT MODAL INTO PAGE
            // =====================================

            modalContainer.innerHTML =
                modalHtml;


            const modalElement =
                document.getElementById(
                    "pointDetailsModal"
                );


            if (!modalElement) {

                console.error(
                    "pointDetailsModal was not found in the returned HTML."
                );

                return;
            }


            // =====================================
            // OPEN BOOTSTRAP MODAL
            // =====================================

            const modal =
                bootstrap.Modal.getOrCreateInstance(
                    modalElement
                );


            modal.show();


            // =====================================
            // CLEAN UP AFTER CLOSING
            // =====================================

            modalElement.addEventListener(
                "hidden.bs.modal",
                function () {

                    modalContainer.innerHTML = "";

                },
                {
                    once: true
                }
            );

        }
        catch (error) {

            console.error(
                "Error loading point details:",
                error
            );

        }

    }


    // =========================================
    // CARD CLICK
    // =========================================

    activityCards.forEach(function (card) {

        card.addEventListener(
            "click",
            function () {

                openPointDetails(card);

            }
        );


        // Also allow Enter key
        card.addEventListener(
            "keydown",
            function (event) {

                if (event.key === "Enter") {

                    event.preventDefault();

                    openPointDetails(card);

                }

            }
        );

    });

});