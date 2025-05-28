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

 

   



    //#region Submit Form
    $('form').on('submit', function (e) {
        e.preventDefault();

        var form = $(this);
        var formData = new FormData(form[0]);

        $.ajax({
            url: form.attr('action'),
            type: form.attr('method'),
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Employee allowance saved successfully.');
                    window.location.href = '/EmployeeAdditional/Index/' + response.data;
                } else {
                    toastr.error('Error saving employee allowance');
                    console.error(response.message);
                }
            },
            error: function (xhr) {
                if (xhr.status === 400 && xhr.responseJSON) {
                    var errors = xhr.responseJSON;
                    for (var field in errors) {
                        if (errors.hasOwnProperty(field)) {
                            toastr.error(errors[field][0]);
                        }
                    }
                } else {
                    toastr.error('Failed to save employee allowance. Please try again.');
                }
            }
        });
    });
    //#endregion

    //#region Clear Form And Load Form
    function loadEmployeeBenifitData(selectedEmployeeId) {
        $.ajax({
            url: '/EmployeeAllowance/GetEmployeeAllowance',
            type: 'GET',
            data: { employeeId: selectedEmployeeId },
            success: function (data) {
                if (data) {
                    populateForm(data);
                }
            },
            error: function () {
                toastr.error('Failed to load employee allowance data.');
            }
        });
    }

    function populateForm(data) {

        debugger
        $('#EmployeeBaseAllowanceID').val(data.employeeBaseAllowanceID || 0);
        $('#PersonalEmail').val(data.personalEmail || '');
        $('#PersonalPhone').val(data.personalPhone || '');

        $('#allowanceEnabled').prop('checked', data.isEmployeeAllowanceEnabled || false);


        // Mobile/Internet Allowance
        $('#MobileInternetAllowance').val(data.mobileInternetAllowance || '');
        $('#mobileInternetAllowanceEnabled').prop('checked', data.isMobileInternetAllowanceEnabled || false);

        // Shift Allowance
        $('#ShiftAllowance').val(data.shiftAllowance || '');
        $('#shiftAllowanceEnabled').prop('checked', data.isShiftAllowanceEnabled || false);

        // House Rent Allowance
        if (data.houseRentAllowancePercentage) {
            globalChoices['HouseRentAllowancePercentage'].setChoiceByValue(data.houseRentAllowancePercentage.toString());
        }
        $('#houseRentAllowanceEnabled').prop('checked', data.isHouseRentAllowancePercentageEnabled || false);

        // Medical Allowance
        if (data.medicalAllowancePercentage) {
            globalChoices['MedicalAllowancePercentage'].setChoiceByValue(data.medicalAllowancePercentage.toString());
        }
        $('#medicalAllowanceEnabled').prop('checked', data.isMedicalAllowancePercentageEnabled || false);

        // Conveyance Allowance
        if (data.conveyanceAllowancePercentage) {
            globalChoices['ConveyanceAllowancePercentage'].setChoiceByValue(data.conveyanceAllowancePercentage.toString());
        }
        $('#conveyanceAllowanceEnabled').prop('checked', data.isConveyanceAllowancePercentageEnabled || false);
    }

    function clearForm() {
        $('form')[0].reset();
        $('#EmployeeBaseAllowanceID').val(0);

        // Clear all choice dropdowns
        clearChoiceDD('HouseRentAllowancePercentage');
        clearChoiceDD('MedicalAllowancePercentage');
        clearChoiceDD('ConveyanceAllowancePercentage');

        // Uncheck all switches
        $('input[type="checkbox"]').prop('checked', false);
    }
    //#endregion


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