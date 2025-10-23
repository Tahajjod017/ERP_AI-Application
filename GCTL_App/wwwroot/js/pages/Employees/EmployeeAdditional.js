

$(document).ready(function () {

    //#region employeeChoices with onchange




    var initData = $('#initEmp').val();

    if (initData && initData !== '') {
        setTimeout(function () {
            loadAndSelectEmployee(initData);
        }, 300);
    }

    paginationService.init('EmployeePersonalId', {
        apiUrl: '/EmployeePersonal/SearchEmployees',
        pageSize: 50,
        minSearchLength: 2,
        loadInitial: true,  
        placeholder: 'Select Employee',
        searchPlaceholder: 'Type to search...'
    });

   



    async function loadAndSelectEmployee(employeeId) {
        try {
            // Fetch the specific employee data from server
            const response = await fetch(`/EmployeePersonal/GetEmployeeByIdCC?id=${employeeId}`);
            const employee = await response.json();

            if (employee && employee.value) {
                // Get the Choices instance
                const instance = paginationService.activeInstances['EmployeePersonalId'];

                if (instance && instance.choices) {
                    // Add this specific employee to choices first
                    instance.choices.setChoices([{
                        value: employee.value,
                        label: employee.label,
                        selected: true
                    }], 'value', 'label', false);

                    // Set the value
                    instance.choices.setChoiceByValue(employee.value);

                    // Update the underlying select
                    $('#EmployeePersonalId').val(employee.value).trigger('change');

                    console.log('Employee preselected:', employee.label);
                }
            }
        } catch (error) {
            console.error('Error loading preselected employee:', error);
        }
    }




    $('#EmployeePersonalId').on('change', function (e) {
        const selectedEmployeeId = e.target.value;
        showDev(selectedEmployeeId, 'Selected Employee ID:');
        debugger;
        if (selectedEmployeeId) {
            loadEmployeeAdditionalData(selectedEmployeeId);
            TabChange(selectedEmployeeId);
        } else {
            clearForm();
        }
    });

    //#endregion


    //#region Get Last Int from URL

    function getLastIntFromUrl() {
        const parts = window.location.pathname.split('/').filter(Boolean).reverse();
        return parts.find(part => !isNaN(part) && Number.isInteger(Number(part)));
    }

    const lastInt = getLastIntFromUrl();
    showDev(lastInt ,'Last int:' );

    if (lastInt) {
        loadEmployeeAdditionalData(lastInt);
       // paginationService.setValue('EmployeePersonalId', lastInt);


       
        setTimeout(function () {
            loadAndSelectEmployee(lastInt);
        }, 300);
       


        //TabChange(lastInt);
    }

    //#endregion

    //#region Load Data

    function loadEmployeeAdditionalData(selectedEmployeeId) {
        // Simulate an AJAX call to fetch employee additional data
        $.ajax({
            url: '/EmployeeAdditional/GetEmployeeData', // Adjust the URL to your API endpoint
            type: 'GET',
            data: { id: selectedEmployeeId },
            success: function (data) {

                showDev(data, 'Employee Additional Data:');

               // choiceManager.setChoiceValue('EmployeePersonalId', data.employeePersonalId);
                $('#PersonalEmail').val(data.personalEmail);
                $('#PersonalPhone').val(data.personalPhone);


                // Populate the form fields with the returned data
                $('#PasportName').val(data.pasportName);
                $('#PasportNo').val(data.pasportNo);
                $('#PasportPlaceOfIssue').val(data.pasportPlaceOfIssue);
                $('#DrivingLicenceNo').val(data.drivingLicenceNo);
                // $('#LicenceTypeID').val(data.licenceTypeID);
                choiceManager.setChoiceValue('LicenceTypeID', data.licenceTypeID);

                $('#SymbolOfVehicleClass').val(data.symbolOfVehicleClass);
                $('#DrivingLicencePlaceOfIssue').val(data.drivingLicencePlaceOfIssue);
                $('#WorkPermaitNumber').val(data.workPermaitNumber);
                $('#WorkPermitType').val(data.workPermitType);


                $('#PasportIssueDate').val(extractDateOnly(data.pasportIssueDate));
                $('#PasportExpireDate').val(extractDateOnly(data.pasportExpireDate));
                $('#DrivingLicenceIssueDate').val(extractDateOnly(data.drivingLicenceIssueDate));
                $('#DrivingLicenceExpireDate').val(extractDateOnly(data.drivingLicenceExpireDate));
                $('#WorkPermitEffectiveDate').val(extractDateOnly(data.workPermitEffectiveDate));
                $('#WorkPermitExpireDate').val(extractDateOnly(data.workPermitExpireDate));
                $('#VisaExpireDate').val(extractDateOnly(data.visaExpireDate));


               

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

    function extractDateOnly(isoString) {
        return isoString ? isoString.split('T')[0] : '';
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

       
       // $('.datetimepicker').flatpickr().clear();

        paginationService.reset('EmployeePersonalId');
        $('#PersonalEmail').val('');
        $('#PersonalPhone').val('');
    }

    //#endregion




    //#region Submit Form



    // $('form').on('submit', function (e) {

    $('#additionalForm').on('submit', function (e) {
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


    //#region Clear Form

  

    $('#btnClear').on('click', function (e) {
        e.preventDefault();
       // alert('55')
       choiceManager.clearChoice('LicenceTypeID');
       

    });

    //#endregion



});