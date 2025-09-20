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

//v1.2
const showCustomToaster = (() => {
    const duration = 3000;
    const toaster = document.getElementById("customToaster");
    if (!toaster) return;

    function show(message, type = "success") {
        let iconURL = "";

        switch (type) {
            case "success":
                iconURL = "/img/toasterIcon/tickmark.png";
                break;
            case "error":
                iconURL = "/img/toasterIcon/cross.png";
                break;
            case "loading":
                iconURL = "/img/toasterIcon/loading.gif";
                break;
            default:
                iconURL = "";
        }

        toaster.innerHTML = `<img src="${iconURL}" class="toast-icon" alt="${type} icon"><span>${message}</span>`;
        toaster.classList.remove("hide-toast");
        toaster.classList.add("show-toast");

        setTimeout(() => {
            toaster.classList.remove("show-toast");
            toaster.classList.add("hide-toast");
        }, duration);
    }

    // return object with helper methods
    return {
        success: (msg) => show(msg, "success"),
        error: (msg) => show(msg, "error"),
        loading: (msg) => show(msg, "loading"),
        custom: (msg, type) => show(msg, type) // for custom types
    };
})();
