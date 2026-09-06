document.addEventListener("DOMContentLoaded", function () {

    const bell = document.getElementById("notifBell");
    const panel = document.getElementById("notifDropdownPanel");
    const countBadge =
        document.getElementById("notifCount");

    let hideTimer = null;
    let loaded = false;


    // LOAD NOTIFICATION DROPDOWN

    async function refreshNotificationDropdown() {

        if (!panel) return;


        try {
            const response = await fetch("/Notifications/Dropdown");


            if (!response.ok) {
                console.error(
                    "Failed to load notifications:",
                    response.status
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

        if (!loaded) {
            refreshNotificationDropdown();
        }
    }


    function scheduleHide() {

        if (!panel) return;

        hideTimer = setTimeout(function () {
            panel.classList.remove("show");
        }, 250);
    }


    if (bell && panel) {

        bell.addEventListener(
            "mouseenter",
            showPanel
        );

        bell.addEventListener(
            "mouseleave",
            scheduleHide
        );

        panel.addEventListener(
            "mouseenter",
            function () {
                clearTimeout(hideTimer);
            }
        );

        panel.addEventListener(
            "mouseleave",
            scheduleHide
        );
    }


    // ========================================
    // UNREAD COUNT
    // ========================================

    async function refreshUnreadCount() {

        if (!countBadge) return;

        try {
            const response =
                await fetch("/Notifications/UnreadCount");

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
                    "/Notifications/MarkAsRead",
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