////customer toaster

//function showCustomToaster(message, type = "success") {
//    const duration = 3000;
//    const toaster = document.getElementById("customToaster");
//    if (!toaster) return;

//    let iconURL = "";

//    switch (type) {
//        case "success":
//            iconURL = "/img/toasterIcon/tickmark.png";
//            break;
//        case "error":
//            iconURL = "/img/toasterIcon/cross.png";
//            break;
//        case "loading":
//            iconURL = "/img/toasterIcon/cross.png";
//            break;
//        default:
//            iconURL = "";
//    }

//    toaster.innerHTML = `<img src="${iconURL}" class="toast-icon" alt="${type} icon"><span>${message}</span>`;

//    toaster.classList.remove("hide-toast");
//    toaster.classList.add("show-toast");

//    setTimeout(() => {
//        toaster.classList.remove("show-toast");
//        toaster.classList.add("hide-toast");
//    }, duration);
//}

////v1.2
//const showCustomToaster = (() => {
//    const duration = 3000;
//    const toaster = document.getElementById("customToaster");
//    if (!toaster) return;

//    function show(message, type = "success") {
//        let iconURL = "";

//        switch (type) {
//            case "success":
//                iconURL = "/img/toasterIcon/tickmark.png";
//                break;
//            case "error":
//                iconURL = "/img/toasterIcon/cross.png";
//                break;
//            case "loading":
//                iconURL = "/img/toasterIcon/loading.gif";
//                break;
//            default:
//                iconURL = "";
//        }

//        toaster.innerHTML = `<img src="${iconURL}" class="toast-icon" alt="${type} icon"><span>${message}</span>`;
//        toaster.classList.remove("hide-toast");
//        toaster.classList.add("show-toast");

//        setTimeout(() => {
//            toaster.classList.remove("show-toast");
//            toaster.classList.add("hide-toast");
//        }, duration);
//    }

//    // return object with helper methods
//    return {
//        success: (msg) => show(msg, "success"),
//        error: (msg) => show(msg, "error"),
//        loading: (msg) => show(msg, "loading"),
//        custom: (msg, type) => show(msg, type) // for custom types
//    };
//})();

//v1.3
const customToaster = (() => {
    const durationDefault = 3000;
    const toaster = document.getElementById("customToaster");
    const modal = document.getElementById("customConfirmModal");
    const modalIcon = document.getElementById("modalIcon");
    const confirmMessage = document.getElementById("confirmMessage");
    const btnYes = document.getElementById("confirmYes");
    const btnNo = document.getElementById("confirmNo");

    function show(message, type = "success", duration = durationDefault) {
        if (!toaster) return;
        let iconHTML = "";
        switch (type) {
            case "success":
                iconHTML = `<img src="https://cdn-icons-png.flaticon.com/512/190/190411.png" class="toast-icon" alt="success">`;
                break;
            case "error":
                iconHTML = `<img src="https://cdn-icons-png.flaticon.com/512/1828/1828665.png" class="toast-icon" alt="error">`;
                break;
            case "loading":
                iconHTML = `<div class="spinner"></div>`;
                break;
            case "info":
                iconHTML = `<img src="https://cdn-icons-png.flaticon.com/512/1828/1828884.png" class="toast-icon" alt="info">`;
                break;
            case "warning":
                iconHTML = `<img src="https://cdn-icons-png.flaticon.com/512/189/189792.png" class="toast-icon spin" alt="warning">`;
                break;
            case "alert":
                iconHTML = `<img src="https://cdn-icons-png.flaticon.com/512/1827/1827349.png" class="toast-icon" alt="alert">`;
                break;
        }
        toaster.innerHTML = `${iconHTML}<span>${message}</span>`;
        toaster.classList.remove("hide-toast");
        toaster.classList.add("show-toast");
        setTimeout(() => { toaster.classList.remove("show-toast"); toaster.classList.add("hide-toast"); }, duration);
    }

    async function confirm(message) {
        return new Promise(resolve => {
            confirmMessage.textContent = message;
            modalIcon.classList.remove("spin");
            modalIcon.src = "https://cdn-icons-png.flaticon.com/512/189/189792.png";
            modalIcon.classList.add("spin");
            modal.classList.add("active");

            const cleanup = () => {
                modal.classList.remove("active");
                btnYes.removeEventListener("click", onYes);
                btnNo.removeEventListener("click", onNo);
            }

            const onYes = () => { cleanup(); resolve(true); }
            const onNo = () => { cleanup(); resolve(false); }

            btnYes.addEventListener("click", onYes);
            btnNo.addEventListener("click", onNo);
        });
    }

    return {
        success: (msg) => show(msg, "success"),
        error: (msg) => show(msg, "error"),
        loading: (msg, dur) => show(msg, "loading", dur),
        info: (msg, dur) => show(msg, "info", dur),
        warning: (msg, dur) => show(msg, "warning", dur),
        alert: (msg, dur) => show(msg, "alert", dur),
        confirm
    }
})();

///* ====== Demo Buttons ====== */
//document.getElementById("btnSuccess").addEventListener("click", () => customToaster.success("Operation completed successfully!"));
//document.getElementById("btnError").addEventListener("click", () => customToaster.error("Something went wrong!"));
//document.getElementById("btnLoading").addEventListener("click", () => customToaster.loading("Loading data...", 5000));
//document.getElementById("btnInfo").addEventListener("click", () => customToaster.info("This is an info message."));
//document.getElementById("btnWarning").addEventListener("click", () => customToaster.warning("This is a warning!"));
//document.getElementById("btnAlert").addEventListener("click", () => customToaster.alert("This is an alert!"));
//document.getElementById("btnConfirm").addEventListener("click", async () => {
//    const confirmed = await customToaster.confirm("Do you want to continue?");
//    if (confirmed) customToaster.success("Confirmed! Next process...");
//    else customToaster.error("Cancelled!");
//});
