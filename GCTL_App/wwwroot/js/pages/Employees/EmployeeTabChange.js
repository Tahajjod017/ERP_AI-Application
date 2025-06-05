//#region Jump Tab






function TabChange(selectedEmployeeId) {
    const $navLinks = $('#myTab .nav-link');
    const employeeId = selectedEmployeeId

    if (employeeId && !isNaN(employeeId) && Number.isInteger(Number(employeeId)) && Number(employeeId) > 0) {
        $navLinks.each(function () {
            const baseUrl = $(this).attr('href').split('/index')[0];
            $(this).attr('href', `${baseUrl}/index/${employeeId}`);
            console.log('Updated href:', $(this).attr('href')); // Debug: Log updated href
        });
    } else {

        $navLinks.each(function () {
            const baseUrl = $(this).attr('href').split('/index')[0];
            $(this).attr('href', `${baseUrl}/index`);
            console.log('Reset href:', $(this).attr('href')); // Debug: Log reset href
        });
    }
}

//#endregion

