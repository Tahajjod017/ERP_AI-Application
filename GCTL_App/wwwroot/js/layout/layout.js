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

//#region Show Loading on page Load

//var loadingTimer;
//var loadingShown = false; // flag to ensure only one modal show
//var loadingTimeout; // for auto-hide

//function triggerLoadingModal(message) {
//    if (!loadingShown) {
//        loadingTimer = setTimeout(function () {
//            showLoadingBaseIndicator(message);
//            loadingShown = true;

//            // Auto-hide modal after 10 seconds
//            loadingTimeout = setTimeout(function () {
//                hideLoadingBaseIndicator(); // make sure this exists
//                loadingShown = false;
//            }, 100);
//        }, 2000); // delay before showing modal
//    }
//}

//window.addEventListener('beforeunload', function () {
//    triggerLoadingModal('Loading, please wait...');
//});

//document.addEventListener('DOMContentLoaded', function () {
//    // Intercept anchor clicks (real page navigations only)
//    document.querySelectorAll('a[href]').forEach(function (anchor) {
//        anchor.addEventListener('click', function (e) {
//            var href = anchor.getAttribute('href');

//            // Skip links that are empty, hashes, or same-page
//            if (
//                !href ||
//                href.trim() === '' ||
//                href.trim() === '#' ||
//                href.startsWith('#') ||
//                e.ctrlKey || e.shiftKey || e.altKey || e.metaKey
//            ) return;

//            triggerLoadingModal('Loading, please wait...');
//        });
//    });

//    // Intercept form submissions
//    document.querySelectorAll('form').forEach(function (form) {
//        form.addEventListener('submit', function () {
//            triggerLoadingModal('Processing, please wait...');
//        });
//    });
//});

//#endregion




document.addEventListener("DOMContentLoaded", function () {
    feather.replace();
});




// For sort icon
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("th.sort").forEach(function (th) {
        const icon = document.createElement("span");
        icon.className = "sort-icon ms-2";
        icon.innerHTML = `<i class="fas fa-sort small text-muted"></i>`;
        th.appendChild(icon);
    });
});



//