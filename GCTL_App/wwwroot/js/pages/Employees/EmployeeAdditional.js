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
                     loadEmployeeAdditionalData(selectedEmployeeId);
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




    //#region Load Data

    function loadEmployeeAdditionalData(selectedEmployeeId) {
        // Simulate an AJAX call to fetch employee additional data
        $.ajax({
            url: '/EmployeeAdditional/GetEmployeeData', // Adjust the URL to your API endpoint
            type: 'GET',
            data: { id: selectedEmployeeId },
            success: function (data) {

                debugger

                choiceManager.setChoiceValue('EmployeePersonalId', data.employeePersonalId);
                $('#PersonalEmail').val(data.personalEmail);
                $('#PersonalPhone').val(data.personalPhone);


                // Populate the form fields with the returned data
                $('#PasportName').val(data.pasportName);
                $('#PasportNo').val(data.pasportNo);
                $('#PasportPlaceOfIssue').val(data.pasportPlaceOfIssue);
                $('#PasportIssueDate').val(data.pasportIssueDate);
                $('#PasportExpireDate').val(data.pasportExpireDate);
                $('#DrivingLicenceNo').val(data.drivingLicenceNo);
                // $('#LicenceTypeID').val(data.licenceTypeID);
                choiceManager.setChoiceValue('LicenceTypeID', data.licenceTypeID);

                $('#DrivingLicenceIssueDate').val(data.drivingLicenceIssueDate);
                $('#DrivingLicenceExpireDate').val(data.drivingLicenceExpireDate);
                $('#SymbolOfVehicleClass').val(data.symbolOfVehicleClass);
                $('#DrivingLicencePlaceOfIssue').val(data.drivingLicencePlaceOfIssue);
                $('#WorkPermaitNumber').val(data.workPermitNumber);
                $('#WorkPermitType').val(data.workPermitType);
                $('#WorkPermitEffectiveDate').val(data.workPermitEffectiveDate);
                $('#WorkPermitExpireDate').val(data.workPermitExpireDate);
                $('#VisaExpireDate').val(data.visaExpireDate);


               

                // If using a date picker, you may need to trigger a change event
                $('.datetimepicker').flatpickr().setDate(data.pasportIssueDate);
                $('.datetimepicker').flatpickr().setDate(data.pasportExpireDate);
                $('.datetimepicker').flatpickr().setDate(data.drivingLicenceIssueDate);
                $('.datetimepicker').flatpickr().setDate(data.drivingLicenceExpireDate);
                $('.datetimepicker').flatpickr().setDate(data.workPermitEffectiveDate);
                $('.datetimepicker').flatpickr().setDate(data.workPermitExpireDate);
                $('.datetimepicker').flatpickr().setDate(data.visaExpireDate);
            },
            error: function (error) {
                console.error("Error fetching employee data:", error);
                clearForm(); // Clear the form if there's an error
            }
        });
    }

    function clearForm() {
        // Clear all input fields
        $('#PasportName').val('');
        $('#PasportNo').val('');
        $('#PasportPlaceOfIssue').val('');
        $('#PasportIssueDate').val('');
        $('#PasportExpireDate').val('');
        $('#DrivingLicenceNo').val('');
        $('#LicenceTypeID').val('');
        $('#DrivingLicenceIssueDate').val('');
        $('#DrivingLicenceExpireDate').val('');
        $('#SymbolOfVehicleClass').val('');
        $('#DrivingLicencePlaceOfIssue').val('');
        $('#WorkPermaitNumber').val('');
        $('#WorkPermitType').val('');
        $('#WorkPermitEffectiveDate').val('');
        $('#WorkPermitExpireDate').val('');
        $('#VisaExpireDate').val('');

        // Reset date pickers if necessary
        $('.datetimepicker').flatpickr().clear();
    }

    //#endregion

  



    //#region Submit Form

    $('form').on('submit', function (e) {
        e.preventDefault();

        const fields = [ "EmployeePersonalId", "PasportName", "PasportNo", "DrivingLicenceNo", "SymbolOfVehicleClass" ];

        if (!validateFields(fields)) {
            return; 
        }


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
                    toastr.success(response.message || 'Employee Additional saved successfully.');
                    window.location.href = '/EmployeeEducation/Index/' + response.data;
                } else {
                    toastr.warning(response.message);
                   
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



});