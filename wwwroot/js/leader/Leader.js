// document.addEventListener("click", async function (e) {

//     const button = e.target.closest(".open-modal");

//     if (!button) {
//         return;
//     }

//     e.preventDefault();

//     const url = button.dataset.url;
//     const title = button.dataset.title || "";

//     if (!url) {
//         return;
//     }

//     try {

//         const response = await fetch(url);

//         if (!response.ok) {
//             throw new Error("Unable to load modal.");
//         }

//         const html = await response.text();

//         const modalElement =
//             document.getElementById("appModal");

//         const modalTitle =
//             document.getElementById("appModalLabel");

//         const modalBody =
//             document.getElementById("appModalBody");

//         modalTitle.textContent = title;
//         modalBody.innerHTML = html;

//         bootstrap.Modal
//             .getOrCreateInstance(modalElement)
//             .show();

//     } catch (error) {

//         console.error(error);

//         showAlertDialog(
//             "Unable to load the form. Please try again.",
//             "error",
//             "Error"
//         );
//     }
// });