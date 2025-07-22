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




function showDeleteModal(deleteCallback) {
    // Show the modal
    $('#confirmDeleteModal').modal('show');

    $('#confirmDeleteBtn').off('click').on('click', function () {
        deleteCallback();
        $('#confirmDeleteModal').modal('hide');
    });
};




function showLoadingIndicator() {
    if ($('#loadingIndicator').length === 0) {
        $('body').append(`
                    <div id="loadingIndicator" style="display:none; position: fixed; top: 35%; left: 35%; z-index: 9999;">
                        <img src="/images/spinner4-unscreen.gif" alt="Loading..." />
                    </div>
                `);
    }

    $("#loadingIndicator").show();
    // $('body').css('filter', 'blur(5px)');
}
function hideLoadingIndicator() {
    $("#loadingIndicator").hide();
}


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




document.addEventListener("DOMContentLoaded", function () {
    feather.replace();
});




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

//