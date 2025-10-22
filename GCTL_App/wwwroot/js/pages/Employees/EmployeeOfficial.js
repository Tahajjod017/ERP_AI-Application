$(document).ready(function () {
    // Wait for choiceManager to initialize
    function initializePaginatedDropdowns() {
        console.debug('Initializing paginated dropdowns');
        const dropdownConfigs = [
            {
                selectId: 'EmployeePersonalId',
                endpoint: '/EmployeePersonal/SearchEmployeeDD',
                placeholderText: 'Select Employee...',
                minSearchLength: 3,
                pageSize: 50
            },
            // keep the two supervisor configs you already have …
            {
                selectId: 'SeniorSupervisorId',
                endpoint: '/EmployeeOfficial/GetEmployeeSupDDbyComp',
                placeholderText: 'Select Senior Supervisor...',
                minSearchLength: 3,
                pageSize: 50,
                dependencies: ['OrganizationID', 'EmployeePersonalId']
            },
            {
                selectId: 'ImmediateSupervisorId',
                endpoint: '/EmployeeOfficial/GetEmployeeSupDDbyComp',
                placeholderText: 'Select Immediate Supervisor...',
                minSearchLength: 3,
                pageSize: 50,
                dependencies: ['OrganizationID', 'EmployeePersonalId']
            },
            {
                selectId: 'HeadOfDepartmentId',
                endpoint: '/EmployeeOfficial/GetEmployeeHOD',
                placeholderText: 'Select Head of Department...',
                minSearchLength: 3,
                pageSize: 50,
                dependencies: ['DepartmentID']
            }
        ];

        const choicesInstances = {};
        dropdownConfigs.forEach(config => {
            console.debug(`Initializing Choices for ${config.selectId}`);
            choicesInstances[config.selectId] = initPaginatedChoices(config);
            if (!choicesInstances[config.selectId]) {
                console.error(`Failed to initialize Choices for ${config.selectId}`);
            }
        });
        return choicesInstances;
    }

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
    
    $("#OrganizationID").change(function () {
        const selectedId = $(this).val();
        const empID = choiceManager.getChoiceValue('EmployeePersonalId') || '';

        ['SeniorSupervisorId', 'ImmediateSupervisorId'/*, 'EmployeePersonalId'*/].forEach(fieldId => {
            const paginatedInstance = choicesInstances[fieldId];
            if (paginatedInstance && paginatedInstance.refresh) {
                paginatedInstance.refresh();               // <-- already correct
            }
        });

        GetBranches(selectedId);
    });

    $("#DepartmentID").change(function () {
        const selectedId = $(this).val();
        const paginatedInstance = choicesInstances['HeadOfDepartmentId'];

        if (paginatedInstance && paginatedInstance.refresh) {
            // ✅ Use the refresh method
            paginatedInstance.refresh();
        }
    });

    $("#EmployeePersonalId").change(function () {
        const selectedId = $(this).val();
        TabChange(selectedId);

        const orgId = choiceManager.getChoiceValue('OrganizationID') || '';

        ['SeniorSupervisorId', 'ImmediateSupervisorId'].forEach(fieldId => {
            const paginatedInstance = choicesInstances[fieldId];
            if (paginatedInstance && paginatedInstance.refresh) {
                // ✅ Refresh with new dependencies
                paginatedInstance.refresh();
            }
        });

        LoadEmployeeOfficData(selectedId);
    });



    const lastInt = getLastIntFromUrl();
    if (lastInt) {
        // 1. fetch the employee (same endpoint you already use)
        fetch(`/EmployeePersonal/GetEmployeeById?id=${lastInt}`)
            .then(r => r.json())
            .then(employee => {
                const inst = choicesInstances['EmployeePersonalId'];
                if (!inst) throw new Error('EmployeePersonalId instance not ready');

                // 2. inject the single item (paginated Choices understands static items)
                inst.choices.setChoices([{
                    value: employee.id,
                    label: employee.name,
                    selected: true
                }], 'value', 'label', false);

                // 3. select it
                inst.choices.setChoiceByValue(employee.id.toString());

                // 4. fire native change so the rest of the page reacts
                $('#EmployeePersonalId').val(employee.id).trigger('change');

                // 5. load the rest of the form
                LoadEmployeeOfficData(lastInt);
            })
            .catch(err => console.error('Pre-select employee failed:', err));
    }

  

    // Load a specific employee by ID into the dropdown
    async function loadEmployeeById(empId) {
        try {
            console.debug(`Loading employee with ID: ${empId}`);
            const res = await fetch(`/EmployeePersonal/GetEmployeeById?id=${empId}`);
            if (!res.ok) {
                throw new Error(`HTTP error! status: ${res.status}`);
            }
            const employee = await res.json();
            if (employee && choicesInstances['EmployeePersonalId']) {
                console.debug(`Populating EmployeePersonalId with:`, employee);
                choiceManager.populator.populateStatic('EmployeePersonalId', [{
                    value: employee.id,
                    label: employee.name,
                    selected: true
                }], {
                    labelKey: 'label',
                    valueKey: 'value',
                    placeholder: choiceManager.getPlaceholderFromHtml('EmployeePersonalId') || 'Select Employee...'
                });
            }
        } catch (error) {
            console.error('Error loading employee by ID:', error);
        }
    }


    function getLastIntFromUrl() {
        const parts = window.location.pathname.split('/').filter(Boolean).reverse();
        return parts.find(part => !isNaN(part) && Number.isInteger(Number(part)));
    }

    // Other existing functions
    function GetBranches(selectedId) {
        $.ajax({
            url: '/EmployeeOfficial/GetBranches',
            type: 'GET',
            data: { id: selectedId },
            success: function (response) {
                console.log('Branches fetched:', response);
                choiceManager.populateDropdown('OrganizationBranchID', response, {
                    labelKey: 'OrganizationBranchName',
                    valueKey: 'OrganizationBranchID'
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
                    console.log("Response received:", response);
                    populateChoicesDropdowns(response);
                    populateFormFields(response);
                    populateDateFields(response);
                    if (response.employeeOfficeInfoID) {
                        $("input[name='EmployeeOfficeInfoID']").val(response.employeeOfficeInfoID);
                    }
                    $(".form-control").prop('disabled', false);
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

    function populateChoicesDropdowns(response) {
        const mapping = [
            { id: 'OrganizationID', value: response.organizationID },
            { id: 'EmployeeTypeID', value: response.employeeTypeID },
            { id: 'DepartmentID', value: response.departmentID },
            { id: 'DesignationID', value: response.designationID },
            { id: 'EmploymentNatureID', value: response.employmentNatureID },
            { id: 'ProvisionPeriodTtimeTypeID', value: response.provisionPeriodTtimeTypeID },
            { id: 'EmploymentStatusId', value: response.employmentStatusId },

            // ---- paginated ones ----
          //  { id: 'EmployeePersonalId', value: response.employeePersonalId },
            { id: 'SeniorSupervisorId', value: response.seniorSupervisorId },
            { id: 'ImmediateSupervisorId', value: response.immediateSupervisorId },
            { id: 'HeadOfDepartmentId', value: response.headOfDepartmentId },

            { id: 'OrganizationBranchID', value: response.organizationBranchID }
        ];

        mapping.forEach(m => {
            if (!m.value) return;

            const inst = choicesInstances[m.id];
            if (inst && inst.choices) {
                // inject + select
                inst.choices.setChoices([{
                    value: m.value.toString(),
                    label: ''   // label will be filled by the server when the user opens the dropdown
                }], 'value', 'label', false);
                inst.choices.setChoiceByValue(m.value.toString());
                $(`#${m.id}`).val(m.value).trigger('change');
            } else {
                // fallback for non-paginated dropdowns (OrganizationID, etc.)
                choiceManager.setChoiceValue(m.id, m.value.toString());
            }
        });
    }

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
    }

    //#endregion

    // Initialize paginated dropdowns after choiceManager
    const choicesInstances = initializePaginatedDropdowns();
});





