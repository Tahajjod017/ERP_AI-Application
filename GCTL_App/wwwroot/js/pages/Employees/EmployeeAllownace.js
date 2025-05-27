$(document).ready(function () {

    //#region employeeChoices with onchange

    let employeeChoices;
    function initEmployeeChoices() {
        employeeChoices = new Choices('#EmployeePersonalId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Employee'
        });

        const employeeElement = document.getElementById('EmployeePersonalId');
        if (employeeElement) {
            employeeElement.addEventListener('change', function (e) {
                const selectedEmployeeId = e.detail.value || e.target.value;
                if (selectedEmployeeId && selectedEmployeeId !== '') {
                    loadEmployeeBenifitData(selectedEmployeeId);
                } else {
                    clearForm();
                }
            });
        }
    }
    document.addEventListener('DOMContentLoaded', initEmployeeChoices);
    initEmployeeChoices();

    //#endregion

    //#region Clear Form And Load Form

    function loadEmployeeBenifitData(selectedEmployeeId) {
        
    }

    function clearForm() {

    }

    //#endregion

    //#region Choice dropdowns

    //let conveyanceAllowancePercentageChoices;
    //function initConveyanceAllowancePercentageChoices() {
    //    conveyanceAllowancePercentageChoices = new Choices('#ConveyanceAllowancePercentage', {
    //        removeItemButton: true,
    //        shouldSort: false,
    //        placeholderValue: 'Select Year'
    //    });
    //}
    //document.addEventListener('DOMContentLoaded', initConveyanceAllowancePercentageChoices);
    //initConveyanceAllowancePercentageChoices();




    //#endregion

    //#region Universal choice

    let globalChoices = {}; 

    function InitChoiceDD(elementId, config = {}) {
        globalChoices[elementId] = new Choices(`#${elementId}`, {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select an option',
            ...config
        });
        return globalChoices[elementId];
    }

    
    InitChoiceDD('ConveyanceAllowancePercentage', { placeholderValue: 'Select %' });
    InitChoiceDD('MedicalAllowancePercentage', { placeholderValue: 'Select %' });
    InitChoiceDD('HouseRentAllowancePercentage', { placeholderValue: 'Select %' });

 

    //#endregion

    //#region Clear Coice

    //clearChoiceDD('ConveyanceAllowancePercentage');
    function clearChoiceDD(elementId) {
        const choicesInstance = globalChoices[elementId];

        if (choicesInstance) {
            choicesInstance.removeActiveItems();
            choicesInstance.setChoiceByValue('');
            $(`#${elementId}`).val('').trigger('change');
        } else {
            $(`#${elementId}`).val('').trigger('change');
        }
    }

    



    //#endregion

});