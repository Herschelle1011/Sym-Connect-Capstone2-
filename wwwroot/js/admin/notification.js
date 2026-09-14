document.addEventListener("DOMContentLoaded", function () {

    const bell = document.getElementById("notifBell");
    const panel = document.getElementById("notifDropdownPanel");
    const countBadge = document.getElementById("notifCount");

    if (!bell || !panel || !countBadge) {
        return;
    }

    const dropdownUrl = bell.dataset.dropdownUrl;
    const countUrl = bell.dataset.countUrl;
    const markReadUrl = bell.dataset.markReadUrl;

    let hideTimer = null;
    let loaded = false;


    // LOAD NOTIFICATION DROPDOWN

    async function refreshNotificationDropdown() {

        if (!panel) return;

        try {
            const response = await fetch(dropdownUrl);

            if (response.redirected) {
                console.error(
                    "Notification request was redirected to:",
                    response.url
                );

                return;
            }

            if (!response.ok) {
                console.error(
                    "Failed to load notifications:",
                    response.status
                );

                return;
            }

            const contentType =
                response.headers.get("content-type");

            // We expect HTML because Dropdown returns a PartialView
            if (!contentType ||
                !contentType.includes("text/html")) {

                console.error(
                    "Unexpected notification response:",
                    contentType
                );

                return;
            }

            const html = await response.text();

            panel.innerHTML = html;

            loaded = true;
        }
        catch (error) {
            console.error(
                "Notification refresh error:",
                error
            );
        }
    }


    // ========================================
    // SHOW / HIDE DROPDOWN
    // ========================================

    function showPanel() {

        if (!panel) return;

        clearTimeout(hideTimer);

        panel.classList.add("show");

   
            refreshNotificationDropdown();
        
    }


    function scheduleHide() {

        if (!panel) return;

        hideTimer = setTimeout(function () {
            panel.classList.remove("show");
        }, 250);
    }


    const notificationWrapper =
        document.getElementById("notificationWrapper");

    if (notificationWrapper) {

        notificationWrapper.addEventListener(
            "mouseenter",
            function () {
                clearTimeout(hideTimer);

                panel.classList.add("show");

                refreshNotificationDropdown();
            }
        );

        notificationWrapper.addEventListener(
            "mouseleave",
            function () {

                hideTimer = setTimeout(function () {

                    panel.classList.remove("show");

                }, 200);
            }
        );
    }


    // ========================================
    // UNREAD COUNT
    // ========================================

    async function refreshUnreadCount() {

        if (!countBadge) return;

        try {
            const response =
                await fetch(countUrl);

            if (!response.ok) {
                console.error(
                    "Failed to get unread count."
                );
                return;
            }

            const data = await response.json();

            if (data.count > 0) {

                countBadge.textContent =
                    data.count > 99
                        ? "99+"
                        : data.count;

                countBadge.style.display =
                    "flex";
            }
            else {

                countBadge.textContent = "0";

                countBadge.style.display =
                    "none";
            }
        }
        
        catch (error) {
            console.error(
                "Unread count error:",
                error
            );
        }
    }


    refreshUnreadCount();


    // ========================================
    // SIGNALR
    // ========================================

    if (typeof signalR !== "undefined") {

        const notifConnection =
            new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect()
                .build();


        notifConnection.on(
            "ReceiveNotification",
            function (data) {

                console.log(
                    "New notification received:",
                    data
                );

                refreshUnreadCount();

                refreshNotificationDropdown();
            }
        );


        notifConnection.on(
            "PendingFilesChanged",
            function () {

                console.log(
                    "Pending files changed."
                );

                if (
                    typeof refreshPendingFilesCount
                    === "function"
                ) {
                    refreshPendingFilesCount();
                }
            }
        );


        notifConnection.start()
            .then(function () {
                console.log(
                    "Notification SignalR connected."
                );
            })
            .catch(function (error) {
                console.error(
                    "SignalR connection error:",
                    error
                );
            });
    }
    else {
        console.error(
            "SignalR library was not loaded."
        );
    }


    // ========================================
    // MARK ONE NOTIFICATION AS READ
    // ========================================

    async function markNotificationAsRead(
        notificationId
    ) {

        if (!notificationId) {
            console.error(
                "Notification ID is missing."
            );
            return false;
        }

        const tokenInput =
            document.querySelector(
                'input[name="__RequestVerificationToken"]'
            );

        if (!tokenInput) {
            console.error(
                "Anti-forgery token not found."
            );
            return false;
        }

        const formData = new FormData();

        formData.append(
            "notificationId",
            notificationId
        );

        formData.append(
            "__RequestVerificationToken",
            tokenInput.value
        );

        try {

            const response =
                await fetch(
                    markReadUrl,
                    {
                        method: "POST",
                        body: formData
                    }
                );

            if (!response.ok) {
                console.error(
                    "MarkAsRead failed:",
                    response.status
                );

                return false;
            }

            return true;
        }
        catch (error) {

            console.error(
                "MarkAsRead error:",
                error
            );

            return false;
        }
    }


    // ========================================
    // VIEW NOTIFICATION
    // ========================================

    document.addEventListener(
        "click",
        async function (e) {

            const viewBtn =
                e.target.closest(
                    ".notif-view-btn"
                );

            if (!viewBtn) {
                return;
            }

            e.preventDefault();

            const url = viewBtn.href;

            const notificationItem =
                viewBtn.closest(
                    ".notif-item, .notif-row"
                );

            if (!notificationItem) {
                window.location.href = url;
                return;
            }

            const notificationId =
                notificationItem.dataset.id;

            console.log(
                "Notification ID:",
                notificationId
            );

            const marked =
                await markNotificationAsRead(
                    notificationId
                );

            if (marked) {

                notificationItem
                    .classList
                    .remove("unread");

                await refreshUnreadCount();
            }

            window.location.href = url;
        }
    );

});