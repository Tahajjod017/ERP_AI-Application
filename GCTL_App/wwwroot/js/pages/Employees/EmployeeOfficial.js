$(document).ready(function () {

    //#region Choice min

    let employeeChoices;
    function initEmployeeChoices() {
        employeeChoices = new Choices('#EmployeePersonalId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Employee'
        });
    }
    document.addEventListener('DOMContentLoaded', initEmployeeChoices);
    initEmployeeChoices();


    //#endregion

});