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
                    TabChange(selectedEmployeeId) // this function is located in EmployeeTabChange.js

                } else {
                    clearForm();
                }
            });
        }
    }
    document.addEventListener('DOMContentLoaded', initEmployeeChoices);
    initEmployeeChoices();

    //#endregion


    //#region Pupulate Data

    // Static data for form population
    const formData = {
        company: ["1"], // Selecting CSL
        isEmployeeAllowanceEnabled: true,
        mobileAllowance: "1500 tk Monthly",
        isMobileInternetAllowanceEnabled: true,
        mobileAllowanceEffectiveFrom: "28/06/2025",
        internetAllowance: "1000 tk Monthly",
        isInternetAllowanceEnabled: true,
        internetAllowanceEffectiveFrom: "01/12/2025",
        isHouseRentAllowanceEnabled: true,
        houseRentAllowancePercentage: "20",
        isMedicalAllowanceEnabled: true,
        medicalAllowancePercentage: "15",
        isConveyanceAllowanceEnabled: true,
        conveyanceAllowancePercentage: "10"
    };

    // Function to populate the form
    function populateFormpp() {
        // Company selection
        $('#multiple-select-tag').val(formData.company).trigger('change');

        // Employee Allowance toggle
        $('#allowanceEnabled').prop('checked', formData.isEmployeeAllowanceEnabled);

        // Mobile Allowance
        $('#MobileAllowance').val(formData.mobileAllowance);
        $('#mobileAllowanceEnabled').prop('checked', formData.isMobileInternetAllowanceEnabled);
        $('#mobileAllowanceEffectiveFrom').val(formData.mobileAllowanceEffectiveFrom);

        // Internet Allowance
        $('#InternetAllowance').val(formData.internetAllowance);
        $('#internetAllowanceEnabled').prop('checked', formData.isInternetAllowanceEnabled);
        $('#internetAllowanceEffectiveFrom').val(formData.internetAllowanceEffectiveFrom);

        // House Rent Allowance
        $('#houseRentAllowanceEnabled').prop('checked', formData.isHouseRentAllowanceEnabled);
        $('#HouseRentAllowancePercentage').val(formData.houseRentAllowancePercentage);

        // Medical Allowance
        $('#medicalAllowanceEnabled').prop('checked', formData.isMedicalAllowanceEnabled);
        $('#MedicalAllowancePercentage').val(formData.medicalAllowancePercentage);

        // Conveyance Allowance
        $('#conveyanceAllowanceEnabled').prop('checked', formData.isConveyanceAllowanceEnabled);
        $('#ConveyanceAllowancePercentage').val(formData.conveyanceAllowancePercentage);

        // Reinitialize any select plugins if needed (e.g., CoreUI select)
        if ($.fn.CoreUISelect) {
            $('#multiple-select-tag').CoreUISelect('update');
        }

        // Trigger flatpickr update if needed
        if ($('.datetimepicker').length) {
            $('.datetimepicker').each(function () {
                if (this._flatpickr) {
                    this._flatpickr.setDate($(this).val(), true);
                }
            });
        }
    }

    //#endregion
 

   



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
                   // populateFormpp();
                }
            },
            error: function () {
                toastr.error('Failed to load employee allowance data.');
            }
        });
    }

    function populateForm(data) {
        console.log('populate form', data);
        debugger
        // Basic fields
        $('#EmployeeBaseAllowanceID').val(data.employeeBaseAllowanceID || 0);
        $('#PersonalEmail').val(data.personalEmail || '');
        $('#PersonalPhone').val(data.personalPhone || '');

        // Main allowance toggle
        $('#allowanceEnabled').prop('checked', data.isEmployeeAllowanceEnabled || false);

        // Mobile Allowance
        $('#MobileAllowance').val(data.mobileAllowance || '');
        $('#mobileAllowanceEnabled').prop('checked', data.isMobileAllowanceEnabled || false);

        // Internet Allowance  
        $('#InternetAllowance').val(data.internetAllowance || '');
        $('#internetAllowanceEnabled').prop('checked', data.isInternetAllowanceEnabled || false);

        // Effective dates - handle null values properly
        if (data.mobileAllowanceEffectiveFrom) {
          //  $('#mobileAllowanceEffectiveFrom').val(moment(data.mobileAllowanceEffectiveFrom).format('DD/MM/YYYY'));
            $('#mobileAllowanceEffectiveFrom').val(data.mobileAllowanceEffectiveFrom);
           // $('#mobileAllowanceEffectiveFrom').val(formData.mobileAllowanceEffectiveFrom);

        } else {
            $('#mobileAllowanceEffectiveFrom').val('');
        }

        if (data.internetAllowanceEffectiveFrom) {
           // $('#internetAllowanceEffectiveFrom').val(moment(data.internetAllowanceEffectiveFrom).format('DD/MM/YYYY'));
            $('#internetAllowanceEffectiveFrom').val(data.internetAllowanceEffectiveFrom);
           // $('#internetAllowanceEffectiveFrom').val(formData.internetAllowanceEffectiveFrom);


        } else {
            $('#internetAllowanceEffectiveFrom').val('');
        }

        // Shift Allowance
        $('#ShiftAllowance').val(data.shiftAllowance || '');
        $('#shiftAllowanceEnabled').prop('checked', data.isShiftAllowanceEnabled || false);

        // House Rent Allowance
        if (data.houseRentAllowancePercentage && globalChoices['HouseRentAllowancePercentage']) {
            globalChoices['HouseRentAllowancePercentage'].setChoiceByValue(data.houseRentAllowancePercentage.toString());
        }
        $('#houseRentAllowanceEnabled').prop('checked', data.isHouseRentAllowancePercentageEnabled || false);

        // Medical Allowance
        if (data.medicalAllowancePercentage && globalChoices['MedicalAllowancePercentage']) {
            globalChoices['MedicalAllowancePercentage'].setChoiceByValue(data.medicalAllowancePercentage.toString());
        }
        $('#medicalAllowanceEnabled').prop('checked', data.isMedicalAllowancePercentageEnabled || false);

        // Conveyance Allowance
        if (data.conveyanceAllowancePercentage && globalChoices['ConveyanceAllowancePercentage']) {
            globalChoices['ConveyanceAllowancePercentage'].setChoiceByValue(data.conveyanceAllowancePercentage.toString());
        }
        $('#conveyanceAllowanceEnabled').prop('checked', data.isConveyanceAllowancePercentageEnabled || false);

        if ($('.datetimepicker').length) {
            $('.datetimepicker').each(function () {
                if (this._flatpickr) {
                    this._flatpickr.setDate($(this).val(), true);
                }
            });
        }
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