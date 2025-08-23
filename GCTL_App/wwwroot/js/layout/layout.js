
// #region Layout JS
var phoenixIsRTL = window.config.config.phoenixIsRTL;
if (phoenixIsRTL) {
    var linkDefault = document.getElementById('style-default');
    var userLinkDefault = document.getElementById('user-style-default');
    linkDefault.setAttribute('disabled', true);
    userLinkDefault.setAttribute('disabled', true);
    document.querySelector('html').setAttribute('dir', 'rtl');
} else {
    var linkRTL = document.getElementById('style-rtl');
    var userLinkRTL = document.getElementById('user-style-rtl');
    linkRTL.setAttribute('disabled', true);
    userLinkRTL.setAttribute('disabled', true);
}




var navbarTopStyle = window.config.config.phoenixNavbarTopStyle;
var navbarTop = document.querySelector('.navbar-top');
if (navbarTopStyle === 'darker') {
    navbarTop.setAttribute('data-navbar-appearance', 'darker');
}




var navbarVerticalStyle = window.config.config.phoenixNavbarVerticalStyle;
var navbarVertical = document.querySelector('.navbar-vertical');
if (navbarVertical && navbarVerticalStyle === 'darker') {
    navbarVertical.setAttribute('data-navbar-appearance', 'darker');
}

document.addEventListener("DOMContentLoaded", function () {
    feather.replace();
});
// #endregion


// #region Show Modal
function showDeleteModal(deleteCallback) {
    // Show the modal
    $('#confirmDeleteModal').modal('show');

    $('#confirmDeleteBtn').off('click').on('click', function () {
        deleteCallback();
        $('#confirmDeleteModal').modal('hide');
    });
};
// #endregion


//#region HideModal
function hideModal(id) {

    const modalEl = document.getElementById(id);
    if (!modalEl) return;

    let modalInstance = bootstrap.Modal.getInstance(modalEl);
    if (!modalInstance) {
        modalInstance = new bootstrap.Modal(modalEl);
    }

    modalInstance.hide();
}
//#endregion


// #region showLoadingIndicator
function showLoadingIndicator() {
    if ($('#loadingIndicator').length === 0) {
        $('body').append(`
                    <div id="loadingIndicator" style="display:none; position: fixed; top: 35%; left: 35%; z-index: 9999;">
                        <img src="/media/indicator/spinner.gif" alt="Loading..." />
                    </div>
                `);
    }

    $("#loadingIndicator").show();
    // $('body').css('filter', 'blur(5px)');
}
function hideLoadingIndicator() {
    $("#loadingIndicator").hide();
}
// #endregion


//#region base Loading indicator 
function showLoadingBaseIndicator(message) {
    // Optional: If you want to show a message, you can add a <h5> or similar in your partial view and control it here.
    if (message) {
        var messageElement = document.getElementById('loadingMessage');
        if (messageElement) {
            messageElement.textContent = message;
            messageElement.style.display = 'block';
        }
    }

    // Show the loader
    var loaderWrapper = document.querySelector('.custom-loader-wrapper');
    if (loaderWrapper) {
        loaderWrapper.style.display = 'flex'; // flex to center properly
    }
}

function hideLoadingBaseIndicator() {
   
    var loaderWrapper = document.querySelector('.custom-loader-wrapper');
    if (loaderWrapper) {
        loaderWrapper.style.display = 'none';
    }
}
//#endregion


// #region For sort icon
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("th.sort").forEach(function (th) {
        const icon = document.createElement("span");
        icon.className = "sort-icon ms-2";
        icon.innerHTML = `<i class="fas fa-sort small text-muted"></i>`;
        th.appendChild(icon);
    });
});
// #endregion


// #region For flatpicker datepicker
//document.addEventListener("DOMContentLoaded", function () {
//    flatpickr(".datetimepicker", {
//        //onDayCreate: function (dObj, dStr, fp, dayElem) {
//        //    const dayNumber = parseInt(dayElem.textContent);
//        //    const year = fp.currentYear;
//        //    const month = fp.currentMonth;

//        //    // Check if the day is part of the currently displayed month
//        //    if (
//        //        !isNaN(dayNumber) &&
//        //        !dayElem.classList.contains("prevMonthDay") &&
//        //        !dayElem.classList.contains("nextMonthDay")
//        //    ) {
//        //        const date = new Date(year, month, dayNumber);
//        //        const dayOfWeek = date.getDay(); // 5 = Friday, 6 = Saturday

//        //        if (dayOfWeek === 5 || dayOfWeek === 6) {
//        //            dayElem.style.backgroundColor = "#FFA500"; // #FFA500 = Orange, gray = #e8eaec
//        //            dayElem.style.color = "#ffffff";
//        //            dayElem.style.borderRadius = "50%";
//        //        }
//        //    }
//        //}
//    });
//});
// #endregion


// #region Validation
function validateField(fieldId, response) {
    const $field = $('#' + fieldId);
    const $errorSpan = $('#' + fieldId + 'Error');
    const hasError = response.field === fieldId;
    const hasValue = $field.val();

    const flatpickrInstance = $field[0]?._flatpickr;
    const $flatpickerInner = flatpickrInstance?.altInput ? $(flatpickrInstance.altInput) : $field;

    const isChoices = $field.closest('.choices').length > 0;
    const $choicesInner = isChoices ? $field.closest('.choices').find('.choices__inner') : null;

    // Reset previous styles
    $flatpickerInner.removeClass('is-invalid is-valid');
    $errorSpan.hide().text('');
    if ($choicesInner) {
        $choicesInner.removeClass('border-danger border-success');
    }

    // Apply validation feedback
    if (hasError) {
        $flatpickerInner.addClass('is-invalid');
        $errorSpan.text(response.message).show();
        if ($choicesInner) $choicesInner.addClass('border-danger');
    } else if (hasValue) {
        $flatpickerInner.addClass('is-valid');
        if ($choicesInner) $choicesInner.addClass('border-success');
    }
}
// #endregion


// #region resetValidation
function resetValidation(fields) {
    fields.forEach(function (fieldId) {
        const $field = $('#' + fieldId);
        const $errorSpan = $('#' + fieldId + 'Error');

        // Remove validation classes from the native input
        $field.removeClass('is-valid is-invalid');
        $errorSpan.hide().text('');

        // Clear the value (works for regular inputs/selects)
        $field.val('');

        // If Choices.js is used, also clean its validation state
        const $choicesInner = $field.closest('.choices').find('.choices__inner');
        if ($choicesInner.length) {
            $choicesInner.removeClass('border-danger border-success is-invalid is-valid');
        }
    });
}
// #endregion


//#region Dev messgae

const dev = true;

function showDev(message, headerText = 'console') {
    if (!dev) return;

    let container = document.getElementById("custom-toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "custom-toast-container";
        document.body.appendChild(container);

        Object.assign(container.style, {
            position: "fixed",
            bottom: "20px",
            left: "20px",
            display: "flex",
            flexDirection: "column",
            gap: "10px",
            zIndex: "9999",
        });
    }

    const toast = document.createElement("div");

   
    Object.assign(toast.style, {
        background: "#333",
        color: "#fff",
        borderRadius: "5px",
        fontSize: "14px",
        boxShadow: "0 2px 6px rgba(0,0,0,0.3)",
        opacity: "0",
        transform: "translateY(20px)",
        transition: "opacity 0.3s ease, transform 0.3s ease",
        pointerEvents: "auto",
        //maxWidth: "300px",
        //minWidth: "300px",
        //overflow: "hidden",
        width: "300px",       // Initial fixed width
        maxWidth: "800px",    // Allow resizing up to this limit
        resize: "both", //    "horizontal"
        overflow: "auto",    
        
        maxHeight: "450px",
        display: "flex",
        flexDirection: "column",
    });

    // Header with Copy button
    const header = document.createElement("div");
    Object.assign(header.style, {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        background: "#444",
        padding: "3px 3px",
        borderTopLeftRadius: "5px",
        borderTopRightRadius: "5px",
    });

    // Console-style text
    const consoleText = document.createElement("span");
    consoleText.textContent = headerText;
    Object.assign(consoleText.style, {
        fontSize: "14px",
        whiteSpace: "nowrap",
    });



    const copyBtn = document.createElement("button");
    copyBtn.textContent = "Copy";
    Object.assign(copyBtn.style, {
        background: "#666",
        color: "#fff",
        border: "none",
        padding: "2px 6px",
        fontSize: "12px",
        borderRadius: "3px",
        cursor: "pointer",
    });

    header.appendChild(consoleText);
    header.appendChild(copyBtn);
    toast.appendChild(header);

    // Message body
    const body = document.createElement("div");
    Object.assign(body.style, {
        padding: "10px 15px",
        overflowY: "auto",
        fontFamily: "monospace",
        whiteSpace: "pre-wrap",
        wordBreak: "break-word",
    });

    let rawText;
    if (typeof message === "object") {
        rawText = JSON.stringify(message, null, 2);
        body.textContent = rawText;
    } else {
        rawText = message;
        body.textContent = message;
    }

    copyBtn.addEventListener("click", () => {
        navigator.clipboard.writeText(rawText).then(() => {
            copyBtn.textContent = "Copied!";
            setTimeout(() => (copyBtn.textContent = "Copy"), 1000);
        });
    });

    toast.appendChild(body);
    container.appendChild(toast);

    requestAnimationFrame(() => {
        toast.style.opacity = "1";
        toast.style.transform = "translateY(0)";
    });

    let hideTimeout;
    let isHovered = false;

    const scheduleRemoval = () => {
        hideTimeout = setTimeout(() => {
            if (!isHovered) {
                toast.style.opacity = "0";
                toast.style.transform = "translateY(20px)";
                setTimeout(() => toast.remove(), 300);
            }
        }, 3000);
    };

    toast.addEventListener("mouseenter", () => {
        isHovered = true;
        clearTimeout(hideTimeout);
    });

    toast.addEventListener("mouseleave", () => {
        isHovered = false;
        scheduleRemoval();
    });

    scheduleRemoval();
}


//#endregion


//