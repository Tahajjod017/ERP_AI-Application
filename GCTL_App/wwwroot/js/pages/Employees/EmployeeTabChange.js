//#region Jump Tab

$(document).ready(function () {

    const lastInt = getLastIntFromUrl();
    if (lastInt) {
        
        TabChange(lastInt);
    }
});


function getLastIntFromUrl() {
    const parts = window.location.pathname.split('/').filter(Boolean).reverse();
    return parts.find(part => !isNaN(part) && Number.isInteger(Number(part)));
}

function setEmpIdOnStart(selectedEmployeeId) {

    debugger
    // **Check pre-selected value on page load**
    const initialSelectedEmployeeId = employeeElement.value;
    if (initialSelectedEmployeeId && initialSelectedEmployeeId !== '') {
        //loadEmployeeAdditionalData(initialSelectedEmployeeId);
        TabChange(initialSelectedEmployeeId);
    }
}



function TabChange(selectedEmployeeId) {
    const $mainNavLinks = $('#myTab .nav-link');
    const $subNavLinks = $('#payrollSubTab .nav-link');
    const employeeId = selectedEmployeeId;

    if (employeeId && !isNaN(employeeId) && Number.isInteger(Number(employeeId)) && Number(employeeId) > 0) {
       
        $mainNavLinks.each(function () {
            const baseUrl = $(this).attr('href').split('/index')[0];
            $(this).attr('href', `${baseUrl}/index/${employeeId}`);
            console.log('Main Tab Updated href:', $(this).attr('href')); // Debug
        });

       
        $subNavLinks.each(function () {
            const baseUrl = $(this).attr('href').split('/index')[0];
            $(this).attr('href', `${baseUrl}/index/${employeeId}`);
            console.log('Sub Tab Updated href:', $(this).attr('href')); // Debug
        });

    } else {
     
        $mainNavLinks.each(function () {
            const baseUrl = $(this).attr('href').split('/index')[0];
            $(this).attr('href', `${baseUrl}/index`);
            console.log('Main Tab Reset href:', $(this).attr('href')); // Debug
        });

       
        $subNavLinks.each(function () {
            const baseUrl = $(this).attr('href').split('/index')[0];
            $(this).attr('href', `${baseUrl}/index`);
            console.log('Sub Tab Reset href:', $(this).attr('href')); // Debug
        });
    }
}




//function TabChange(selectedEmployeeId) {
//    const $navLinks = $('#myTab .nav-link');
//    const employeeId = selectedEmployeeId

//    if (employeeId && !isNaN(employeeId) && Number.isInteger(Number(employeeId)) && Number(employeeId) > 0) {
//        $navLinks.each(function () {
//            const baseUrl = $(this).attr('href').split('/index')[0];
//            $(this).attr('href', `${baseUrl}/index/${employeeId}`);
//            console.log('Updated href:', $(this).attr('href')); // Debug: Log updated href
//        });
//    } else {

//        $navLinks.each(function () {
//            const baseUrl = $(this).attr('href').split('/index')[0];
//            $(this).attr('href', `${baseUrl}/index`);
//            console.log('Reset href:', $(this).attr('href')); // Debug: Log reset href
//        });
//    }
//}

//#endregion

