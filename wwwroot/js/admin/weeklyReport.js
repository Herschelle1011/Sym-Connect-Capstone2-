// ============================================================
// WEEKLY ORGANIZATIONAL SUMMARY
// ============================================================

document.addEventListener("DOMContentLoaded", function () {

    console.log("Weekly Report JS loaded");


    // ========================================================
    // CHECK DATA
    // ========================================================

    const reportData =
        window.weeklyReportData;


    if (!reportData) {

        console.error(
            "weeklyReportData was not found."
        );

        return;

    }


    console.log(
        "Weekly report data:",
        reportData
    );


    // ========================================================
    // ACTIVITY SUMMARY DOUGHNUT CHART
    // ========================================================

    const activityCanvas =
        document.getElementById(
            "activityPieChart"
        );


    if (activityCanvas) {

        new Chart(
            activityCanvas,
            {

                type: "doughnut",

                data: {

                    labels: [
                        "Completed",
                        "On-Going",
                        "Upcoming",
                        "Cancelled"
                    ],

                    datasets: [{

                        data: [

                            reportData
                                .activity
                                .completed,

                            reportData
                                .activity
                                .ongoing,

                            reportData
                                .activity
                                .upcoming,

                            reportData
                                .activity
                                .cancelled

                        ],

                        backgroundColor: [
                            "#2563eb",
                            "#16a34a",
                            "#f59e0b",
                            "#ef4444"
                        ],

                        borderWidth: 0

                    }]

                },


                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    cutout: "65%",

                    plugins: {

                        legend: {
                            display: false
                        }

                    }

                }

            }
        );

    }
    else {

        console.error(
            "activityPieChart canvas not found."
        );

    }


    // ========================================================
    // ATTENDANCE OVERVIEW LINE CHART
    // ========================================================

    const attendanceCanvas =
        document.getElementById(
            "attendanceLineChart"
        );


    if (attendanceCanvas) {

        new Chart(
            attendanceCanvas,
            {

                type: "line",

                data: {

                    labels:
                        reportData
                            .attendanceLabels,

                    datasets: [{

                        label:
                            "Attendance",

                        data:
                            reportData
                                .attendanceValues,

                        borderColor:
                            "#3860D6",

                        backgroundColor:
                            "rgba(56, 96, 214, 0.08)",

                        borderWidth: 2,

                        tension: 0.35,

                        fill: true,

                        pointBackgroundColor:
                            "#3860D6",

                        pointBorderColor:
                            "#3860D6",

                        pointRadius: 4,

                        pointHoverRadius: 5

                    }]

                },


                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    plugins: {

                        legend: {
                            display: false
                        }

                    },


                    scales: {

                        y: {

                            beginAtZero: true,

                            ticks: {

                                precision: 0

                            },

                            grid: {

                                color:
                                    "#f3f4f6"

                            }

                        },


                        x: {

                            grid: {

                                display:
                                    false

                            }

                        }

                    }

                }

            }
        );

    }
    else {

        console.error(
            "attendanceLineChart canvas not found."
        );

    }


    // ========================================================
    // EXPORT PDF
    // ========================================================

    const exportBtn =
        document.getElementById(
            "exportWeeklyReportBtn"
        );


    const reportArea =
        document.getElementById(
            "weeklyReportExportArea"
        );


    if (!exportBtn) {

        console.error(
            "Export button not found."
        );

        return;

    }


    if (!reportArea) {

        console.error(
            "weeklyReportExportArea not found."
        );

        return;

    }


    exportBtn.addEventListener(
        "click",
        async function () {

            console.log(
                "Export clicked"
            );


            // =================================================
            // CHECK PDF LIBRARIES
            // =================================================

            if (
                typeof window.html2canvas ===
                "undefined"
            ) {

                console.error(
                    "html2canvas not loaded."
                );

                alert(
                    "PDF export library could not be loaded."
                );

                return;

            }


            if (
                !window.jspdf ||
                !window.jspdf.jsPDF
            ) {

                console.error(
                    "jsPDF not loaded."
                );

                alert(
                    "PDF library could not be loaded."
                );

                return;

            }


            // =================================================
            // BUTTON LOADING
            // =================================================

            const originalHtml =
                exportBtn.innerHTML;


            exportBtn.disabled =
                true;


            exportBtn.innerHTML = `
                <span class="spinner-border spinner-border-sm me-1"></span>
                Exporting...
            `;


            try {

                // Give Chart.js time to completely render
                await new Promise(
                    function (resolve) {

                        setTimeout(
                            resolve,
                            500
                        );

                    }
                );


                // =================================================
                // CAPTURE REPORT
                // =================================================

                const capturedCanvas =
                    await window.html2canvas(
                        reportArea,
                        {

                            scale: 2,

                            useCORS: true,

                            backgroundColor:
                                "#ffffff",

                            logging: false

                        }
                    );


                console.log(
                    "Report captured:",
                    capturedCanvas.width,
                    capturedCanvas.height
                );


                // =================================================
                // CREATE PDF
                // =================================================

                const jsPDF =
                    window.jspdf.jsPDF;


                const pdf =
                    new jsPDF(
                        "landscape",
                        "mm",
                        "a4"
                    );


                const pageWidth =
                    pdf.internal
                        .pageSize
                        .getWidth();


                const pageHeight =
                    pdf.internal
                        .pageSize
                        .getHeight();


                const margin =
                    7;


                const maxWidth =
                    pageWidth -
                    (margin * 2);


                const maxHeight =
                    pageHeight -
                    (margin * 2);


                // =================================================
                // IMAGE SIZE
                // =================================================

                const imageRatio =
                    capturedCanvas.height /
                    capturedCanvas.width;


                let imageWidth =
                    maxWidth;


                let imageHeight =
                    imageWidth *
                    imageRatio;


                // If report is taller than A4,
                // shrink while keeping its ratio.
                if (
                    imageHeight >
                    maxHeight
                ) {

                    imageHeight =
                        maxHeight;


                    imageWidth =
                        imageHeight /
                        imageRatio;

                }


                const x =
                    (
                        pageWidth -
                        imageWidth
                    ) / 2;


                const imageData =
                    capturedCanvas
                        .toDataURL(
                            "image/jpeg",
                            0.95
                        );


                // =================================================
                // ADD IMAGE
                // =================================================

                pdf.addImage(
                    imageData,
                    "JPEG",
                    x,
                    margin,
                    imageWidth,
                    imageHeight
                );


                // =================================================
                // FILE NAME
                // =================================================

                const startDate =
                    document
                        .getElementById(
                            "startDate"
                        )
                        ?.value ||
                    "start";


                const endDate =
                    document
                        .getElementById(
                            "endDate"
                        )
                        ?.value ||
                    "end";


                pdf.save(
                    `weekly-organizational-summary-${startDate}-to-${endDate}.pdf`
                );


                console.log(
                    "PDF exported successfully."
                );

            }
            catch (error) {

                console.error(
                    "PDF export failed:",
                    error
                );


                alert(
                    "Unable to export the report. Check the browser console."
                );

            }
            finally {

                exportBtn.disabled =
                    false;


                exportBtn.innerHTML =
                    originalHtml;

            }

        }
    );

});