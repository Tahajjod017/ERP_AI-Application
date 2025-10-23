$(document).ready(function () {
    
   // const choicesInstances = initializePaginatedDropdowns();
    let branchLoad = true;
    
    let choicesInstances = {};

    try {
        if (typeof initializePaginatedDropdowns === 'function') {
            choicesInstances = initializePaginatedDropdowns();
        }
    } catch (error) {
        console.warn('initializePaginatedDropdowns not available, using empty object');
        choicesInstances = {};
    }
   

   
    //#region employeeChoices with onchange




    var initData = $('#initEmp').val();

    if (initData && initData !== '') {
        setTimeout(function () {
            loadAndSelectEmployee(initData);
        }, 300);
    }

    paginationService.init('EmployeePersonalId', {
        apiUrl: '/EmployeePersonal/SearchEmployeesAll',
        pageSize: 50,
        minSearchLength: 2,
        loadInitial: true,
        placeholder: 'Select Employee',
        searchPlaceholder: 'Type to search...'
    });




    $("#OrganizationID").change(function () {
        const selectedId = $(this).val();
        const empID = choiceManager.getChoiceValue('EmployeePersonalId') || '';

        // Refresh supervisor Select2 dropdowns
        const supervisorIds = ['SeniorSupervisorId', 'ImmediateSupervisorId', 'HeadOfDepartmentId'];

        supervisorIds.forEach(fieldId => {
            const $select = $('#' + fieldId);
            if ($select.length) {
                if ($select.hasClass('select2-hidden-accessible')) {
                    // This is a Select2 dropdown - clear it
                    $select.val(null).trigger('change');
                } else {
                    // This might be a choices.js dropdown - refresh it
                    const paginatedInstance = choicesInstances[fieldId];
                    if (paginatedInstance && paginatedInstance.refresh) {
                        paginatedInstance.refresh();
                    }
                }
            }
        });

        // Refresh EmployeePersonalId (choices.js)
        const employeeInstance = choicesInstances['EmployeePersonalId'];
        if (employeeInstance && employeeInstance.refresh) {
            employeeInstance.refresh();
        }

        if (branchLoad) {
            GetBranches(selectedId);
        }

        
    });

    $("#DepartmentID").change(function () {
        const selectedId = $(this).val();

        // Handle Head of Department refresh
        const $headOfDept = $('#HeadOfDepartmentId');
        if ($headOfDept.length) {
            if ($headOfDept.hasClass('select2-hidden-accessible')) {
                // Select2 dropdown - clear it
                $headOfDept.val(null).trigger('change');
            } else {
                // Choices.js dropdown - refresh it
                const paginatedInstance = choicesInstances['HeadOfDepartmentId'];
                if (paginatedInstance && paginatedInstance.refresh) {
                    paginatedInstance.refresh();
                }
            }
        }
    });




    //#region Initialize Select2 for Supervisor Dropdowns
    function initializeSupervisorSelect2() {
        const commonConfig = {
            width: '100%',
            ajax: {
                url: '/EmployeeOfficial/GetSupervisors',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    
                    //const organizationId = $('#OrganizationID').val();
                    //const departmentId = $('#DepartmentID').val();

                    const organizationId = choiceManager.getChoiceValue('OrganizationID');
                    const departmentId = choiceManager.getChoiceValue('DepartmentID');

                    return {
                        search: params.term || '',
                        page: params.page || 1,
                        organizationId: organizationId,
                        departmentId: departmentId,
                        roleType: $(this).attr('id') // This will be the select element ID
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: data.results,
                        pagination: {
                            more: data.pagination.more
                        }
                    };
                },
                cache: true
            }
        };

        // Senior Supervisor
        $('#SeniorSupervisorId').select2({
            ...commonConfig,
            allowClear: true,

            placeholder: 'Select Senior Supervisor'
        });


        //const initializeSelect = () => {
        //    $('#SeniorSupervisorId').select2({
        //        width: '100%',
        //        allowClear: true,
        //        placeholder: 'Select an option',
        //        language: { noResults: () => 'No results found' },
        //        escapeMarkup: markup => markup
        //    });
        //};

        // Immediate Supervisor
        $('#ImmediateSupervisorId').select2({
            ...commonConfig,
            allowClear: true,

            placeholder: 'Select Immediate Supervisor'
        });

        // Head of Department
        $('#HeadOfDepartmentId').select2({
            ...commonConfig,
            allowClear: true,

            placeholder: 'Select Head of Department'
        });

        // Refresh supervisor dropdowns when organization or department changes
        $('#OrganizationID, #DepartmentID').on('change', function () {
            refreshSupervisorDropdowns();
        });
    }

    function refreshSupervisorDropdowns() {
        const organizationId = $('#OrganizationID').val();
        const departmentId = $('#DepartmentID').val();

        if (!organizationId) {
            // Clear all supervisor dropdowns if no organization selected
            $('#SeniorSupervisorId, #ImmediateSupervisorId, #HeadOfDepartmentId')
                .val(null).trigger('change');
            return;
        }

        // Refresh each dropdown
        $('#SeniorSupervisorId').val(null).trigger('change');
        $('#ImmediateSupervisorId').val(null).trigger('change');
        $('#HeadOfDepartmentId').val(null).trigger('change');
    }

    // Initialize Select2 dropdowns
    initializeSupervisorSelect2();
    //#endregion


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
       
        if (selectedEmployeeId) {
            LoadEmployeeOfficData(selectedEmployeeId);
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
    showDev(lastInt, 'Last int:');

    if (lastInt) {
        LoadEmployeeOfficData(lastInt);
       


        setTimeout(function () {
            loadAndSelectEmployee(lastInt);
        }, 300);



        //TabChange(lastInt);
    }

    //#endregion



  

  



    //#region Form Validation and Submission
    function isValidEmail(email) {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }
    function showError(fieldName, message) {
        let $input = $("input[name='" + fieldName + "']");
        if ($input.length === 0) {
            $input = $("select[name='" + fieldName + "']");
        }
        if ($input.length === 0) {
            $input = $("#" + fieldName);
        }
        if ($input.length === 0) {
            console.error("Could not find input for field: " + fieldName);
            return;
        }
        removeError(fieldName);
        $input.addClass('border-danger').css('border-color', '#dc3545');
        const $error = $('<span class="text-danger d-block mt-1 validation-error" data-field="' + fieldName + '" style="padding: 2px 4px; border-radius: 3px; font-size: 12px; white-space: nowrap;">' + message + '</span>');
        const $container = $input.closest('.form-floating, .flatpickr-input-container, .col-sm-12, .col-md-4');
        if ($container.length > 0) {
            $container.after($error);
        } else {
            $input.after($error);
        }
    }
    function removeError(fieldName) {
        const $input = $("input[name='" + fieldName + "'], select[name='" + fieldName + "'], #" + fieldName);
        $input.removeClass('border-danger').css('border-color', '');
        $(".validation-error[data-field='" + fieldName + "']").remove();
        $input.next(".validation-error").remove();
        $input.closest('.form-floating, .flatpickr-input-container').next(".validation-error").remove();
    }
    function clearErrors() {
        $(".validation-error").remove();
    }
    function attachInputValidationHandler() {
        $(document).on("input blur", "input[asp-for]", function () {
            const fieldName = $(this).attr("name");
            const value = $(this).val().trim();
            if (value !== "") {
                if (fieldName === "OfficeEmail" && !isValidEmail(value)) {
                    showError(fieldName, "Invalid email format.");
                    return;
                }
                removeError(fieldName);
            }
        });
        $(document).on("input blur",
            "input[name='OfficePhone'], input[name='OfficeEmail'], input[name='AttendanceId'], input[name='ConfirmationLetterNo'], input[name='AppointmentLetterNo'], input[name='JoiningDate']",
            function () {
                const fieldName = $(this).attr("name");
                const value = $(this).val().trim();
                if (value !== "") {
                    if (fieldName === "OfficeEmail" && !isValidEmail(value)) {
                        showError(fieldName, "Invalid email format.");
                        return;
                    }
                    removeError(fieldName);
                }
            });
    }
    attachInputValidationHandler();

    $(document).on("change", "select", function () {
        const fieldName = $(this).attr("name");
        const value = $(this).val();
        if (value && value.trim() !== "") {
            removeError(fieldName);
        }
    });
    function validateOfficialForm() {
        clearErrors();
        let isValid = true;
        const requiredSelects = [
            { name: "EmployeePersonalId", label: "Employee" },
            { name: "OrganizationID", label: "Organization" },
            { name: "EmploymentStatusId", label: "Status" }
        ];
        const requiredInputs = [
            { name: "OfficeEmail", label: "Office Email", type: "email" }
        ];
        requiredSelects.forEach(field => {
            const $select = $("select[name='" + field.name + "']");
            const value = $select.val();
            if (!value || value.trim() === "") {
                showError(field.name, field.label + " is required.");
                toastr.warning(field.name, field.label + " is required.");
                isValid = false;
            }
        });
        requiredInputs.forEach(field => {
            const $input = $("input[name='" + field.name + "']");
            if ($input.length === 0) {
                console.error("Could not find input field: " + field.name);
                return;
            }
            const value = $input.val().trim();
            console.log("Validating field:", field.name, "Value:", value);
            if (!value) {
                showError(field.name, field.label + " is required.");
                toastr.warning(field.name, field.label + " is required.");
                isValid = false;
            } else if (field.type === "email" && !isValidEmail(value)) {
                showError(field.name, "Invalid email format.");
                isValid = false;
            }
        });
        const probationPeriod = $("input[name='ProvisionPeriod']").val().trim();
        const probationStartDate = $("input[name='ProvisionPeriodStartDate']").val().trim();
        const timeUnit = $("select[name='ProvisionPeriodTtimeTypeID']").val();
        if (probationPeriod && (!probationStartDate || !timeUnit)) {
            if (!probationStartDate) {
                showError("ProvisionPeriodStartDate", "Probation start date is required when probation period is specified.");
                isValid = false;
            }
            if (!timeUnit) {
                showError("ProvisionPeriodTtimeTypeID", "Time unit is required when probation period is specified.");
                isValid = false;
            }
        }
        return isValid;
    }

    // Form submission
    $('#employeeOfficialForm').on('submit', function (e) {
        e.preventDefault();
        if (!validateOfficialForm()) {
            const firstError = $('.validation-error').first();
            if (firstError.length) {
                $('html, body').animate({
                    scrollTop: firstError.offset().top - 100
                }, 500);
            }
            return;
        }
        const form = $(this);
        const formData = form.serialize();
        const $submitBtn = $('#btnSubmit');
        const originalText = $submitBtn.text();
        $submitBtn.prop('disabled', true).text('Saving...');
        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: formData,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                console.log(response);
                if (response.success) {
                    toastr.success(response.message);
                    window.location.href = '/EmployeeOfficial/Index/' + response.data;
                } else {
                    toastr.warning(response.message);
                }
            },
            error: function (xhr) {
                console.error('Error:', xhr);
                if (xhr.responseJSON && xhr.responseJSON.errors) {
                    Object.keys(xhr.responseJSON.errors).forEach(key => {
                        const errors = xhr.responseJSON.errors[key];
                        if (errors.length > 0) {
                            showError(key, errors[0]);
                        }
                    });
                } else {
                    alert('Something went wrong! Please try again.');
                }
            },
            complete: function () {
                $submitBtn.prop('disabled', false).text(originalText);
            }
        });
    });


    //#endregion
   
    function GetBranches(selectedId) {
        $.ajax({
            url: '/EmployeeOfficial/GetBranches',
            type: 'GET',
            data: { id: selectedId },
            success: function (response) {
                showDev(response, '/GetBranches')
                choiceManager.populateDropdown('OrganizationBranchID', response, {
                    labelKey: 'name',
                    valueKey: 'id'
                });
            },
            error: function (xhr, status, error) {
                console.error('Error fetching branches:', error);
            }
        });
    }


    //#region Load Employee Official Data
    function LoadEmployeeOfficData(selectedId) {
        if (selectedId) {
            $(".form-control").prop('disabled', true);
            $.ajax({
                url: "/EmployeeOfficial/GetEmployeeDetails",
                type: "GET",
                data: { id: selectedId },
                success: function (response) {
                    showDev(response, 'office response')

                    branchLoad = false;

                    populateChoicesDropdowns(response);
                    populateFormFields(response);
                    populateDateFields(response);
                    if (response.employeeOfficeInfoID) {
                        $("input[name='EmployeeOfficeInfoID']").val(response.employeeOfficeInfoID);
                    }
                    $(".form-control").prop('disabled', false);

                    branchLoad = true;

                    console.log("Employee data loaded successfully");
                },
                error: function (xhr, status, error) {
                    $(".form-control").prop('disabled', false);
                    console.error("Error loading employee data:", error);
                    console.error("Response:", xhr.responseText);
                    alert("Error loading employee data. Please try again.");
                }
            });
        } else {
            clearAllFormFields();
        }
    }

    //#region Modified populateChoicesDropdowns function for Select2
    function populateChoicesDropdowns(response) {
        const mapping = [
            { id: 'OrganizationID', value: response.organizationID },
            { id: 'EmployeeTypeID', value: response.employeeTypeID },
            { id: 'DepartmentID', value: response.departmentID },
            { id: 'DesignationID', value: response.designationID },
            { id: 'EmploymentNatureID', value: response.employmentNatureID },
            { id: 'ProvisionPeriodTtimeTypeID', value: response.provisionPeriodTtimeTypeID },
            { id: 'EmploymentStatusId', value: response.employmentStatusId },
            { id: 'OrganizationBranchID', value: response.organizationBranchID }
        ];

        mapping.forEach(item => {
            choiceManager.setChoiceValue(item.id, item.value);
        });

        
        // Set supervisor values after a delay to ensure dropdowns are initialized
        setTimeout(function () {
           
            //if (response.organizationID) {
            //    $('#OrganizationID').val(response.organizationID).trigger('change');
            //}

            //if (response.departmentID) {
            //    $('#DepartmentID').val(response.departmentID).trigger('change');
            //}

            
            setTimeout(function () {
               


                if (response.seniorSupervisorId) { //&& response.seniorSupervisorName) {

                    var seniorSupervisorName = GetEmpNameById(response.seniorSupervisorId);

                    setSelectedSupervisor('SeniorSupervisorId', response.seniorSupervisorId, seniorSupervisorName);
                }
                if (response.immediateSupervisorId) {  // && response.immediateSupervisorName) {

                    var immediateSupervisorName = GetEmpNameById(response.immediateSupervisorId);

                    setSelectedSupervisor('ImmediateSupervisorId', response.immediateSupervisorId, immediateSupervisorName);
                }
                if (response.headOfDepartmentId) { // && response.headOfDepartmentName) {

                    var headOfDepartmentName = GetEmpNameById(response.headOfDepartmentId);

                    setSelectedSupervisor('HeadOfDepartmentId', response.headOfDepartmentId, headOfDepartmentName);
                }


            }, 1000);
        }, 500);
    }

    // Function to set selected supervisor in Select2
    function setSelectedSupervisor(selectId, supervisorId, supervisorName) {
        if (supervisorId && supervisorName) {
            var $select = $('#' + selectId);

            // Create and append the option
            if ($select.find('option[value="' + supervisorId + '"]').length === 0) {
                var option = new Option(supervisorName, supervisorId, true, true);
                $select.append(option);
            }

            // Set the value and trigger change
            $select.val(supervisorId).trigger('change');
        }
    }
    //#endregion



    //#region get emp name by id synchronously

    function GetEmpNameById(empId) {
        var empName = '';
        $.ajax({
            url: '/EmployeePersonal/GetEmployeeNameById',
            type: 'GET',
            data: { id: empId },
            async: false, // Synchronous request
            success: function (response) {
                empName = response.name;
            },
            error: function (xhr, status, error) {
                console.error('Error fetching employee name:', error);
            }
        });
        return empName;
    }
    //#endregion


    function populateFormFields(response) {
        const fieldMappings = {
            'PersonalEmail': response.personalEmail,
            'PersonalPhone': response.personalPhone,
            'OfficePhone': response.officePhone,
            'OfficeEmail': response.officeEmail,
            'AttendanceId': response.attendanceId,
            'AppointmentLetterNo': response.appointmentLetterNo,
            'ConfirmationLetterNo': response.confirmationLetterNo,
            'ProvisionPeriod': response.provisionPeriod
        };
        Object.entries(fieldMappings).forEach(([fieldName, value]) => {
            $(`input[name='${fieldName}']`).val(value || '');
        });
    }

    function populateDateFields(response) {
        const dateMappings = {
            'appointmentLetterIssueDate': '#floatingInputAppointmentDate',
            'joiningDate': '#floatingInputJoiningDate',
            'provisionPeriodStartDate': '#floatingInputProbationStartDate',
            'confirmationDate': '#floatingInputConfirmationDate',
            'contractEndDate': '#floatingInputContractEndDate'
        };
        Object.entries(dateMappings).forEach(([responseKey, selector]) => {
            const dateValue = response[responseKey];
            if (dateValue) {
                const formattedDate = formatDateForFlatpickr(dateValue);
                $(selector).val(formattedDate);
                $(`input[name='${responseKey}']`).val(formattedDate);
            }
        });
    }

    function formatDateForFlatpickr(dateString) {
        if (!dateString) return '';
        try {
            let date;
            if (dateString instanceof Date) {
                date = dateString;
            } else if (typeof dateString === 'string') {
                if (dateString.includes('/Date(')) {
                    const timestamp = parseInt(dateString.match(/\d+/)[0]);
                    date = new Date(timestamp);
                } else {
                    date = new Date(dateString);
                }
            } else if (typeof dateString === 'number') {
                date = new Date(dateString);
            }
            if (isNaN(date.getTime())) {
                console.warn('Invalid date:', dateString);
                return '';
            }
            return date.toISOString().split('T')[0];
        } catch (error) {
            console.error('Error formatting date:', dateString, error);
            return '';
        }
    }

    //#endregion


    //#region Clear Form

    function clearAllFormFields() {
        $("input[name='PersonalEmail'], input[name='PersonalPhone'], input[name='OfficePhone']").val('');
        $("input[name='OfficeEmail'], input[name='AttendanceId']").val('');
        $("input[name='AppointmentLetterNo'], input[name='ConfirmationLetterNo'], input[name='ProvisionPeriod']").val('');
        $("input[name='AppointmentLetterIssueDate'], input[name='JoiningDate']").val('');
        $("input[name='ProvisionPeriodStartDate'], input[name='ConfirmationDate'], input[name='ContractEndDate']").val('');
        $("#floatingInputAppointmentDate, #floatingInputJoiningDate, #floatingInputProbationStartDate").val('');
        $("#floatingInputConfirmationDate, #floatingInputContractEndDate").val('');
        choiceManager.resetAllChoices();
        $("input[name='EmployeeOfficeInfoID']").val('');


        $('#SeniorSupervisorId').val(null).trigger('change');
        $('#ImmediateSupervisorId').val(null).trigger('change');
        $('#HeadOfDepartmentId').val(null).trigger('change');

    }

    //#endregion

    // Initialize paginated dropdowns after choiceManager
   // const choicesInstances = initializePaginatedDropdowns();
});





