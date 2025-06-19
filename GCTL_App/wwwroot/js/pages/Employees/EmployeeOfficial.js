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



    let organizationChoices;
    function initOrganizationChoices() {
        organizationChoices = new Choices('#OrganizationID', {
            removeItemButton: false,
            shouldSort: false,
            placeholderValue: 'Select Organization'
        });
    }
    document.addEventListener('DOMContentLoaded', initOrganizationChoices);
    initOrganizationChoices();


    let branchChoices;
    function initBranchChoices() {
        branchChoices = new Choices('#OrganizationBranchID', {
            removeItemButton: true,
            shouldSort: true,
            placeholderValue: 'Select Branch'
        });
    }
    document.addEventListener('DOMContentLoaded', initBranchChoices);
    initBranchChoices();


    let employeeTypeChoices;
    function initEmployeeTypeChoices() {
        employeeTypeChoices = new Choices('#EmployeeTypeID', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Employee Type'
        });
    }
    document.addEventListener('DOMContentLoaded', initEmployeeTypeChoices);
    initEmployeeTypeChoices();


    let departmentChoices;
    function initDepartmentChoices() {
        departmentChoices = new Choices('#DepartmentID', {
            removeItemButton: false,
            shouldSort: true,
            placeholderValue: 'Select Department'
        });
    }
    document.addEventListener('DOMContentLoaded', initDepartmentChoices);
    initDepartmentChoices();


    let designationChoices;
    function initDesignationChoices() {
        designationChoices = new Choices('#DesignationID', {
            removeItemButton: true,
            shouldSort: true,
            placeholderValue: 'Select Designation'
        });
    }
    document.addEventListener('DOMContentLoaded', initDesignationChoices);
    initDesignationChoices();


    let employmentNatureChoices;
    function initEmploymentNatureChoices() {
        employmentNatureChoices = new Choices('#EmploymentNatureID', {
            removeItemButton: false,
            shouldSort: false,
            placeholderValue: 'Select Employment Nature'
        });
    }
    document.addEventListener('DOMContentLoaded', initEmploymentNatureChoices);
    initEmploymentNatureChoices();


    let seniorSupervisorChoices;
    function initSeniorSupervisorChoices() {
        seniorSupervisorChoices = new Choices('#SeniorSupervisorId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Senior Supervisor'
        });
    }
    document.addEventListener('DOMContentLoaded', initSeniorSupervisorChoices);
    initSeniorSupervisorChoices();


    let immediateSupervisorChoices;
    function initImmediateSupervisorChoices() {
        immediateSupervisorChoices = new Choices('#ImmediateSupervisorId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Immediate Supervisor'
        });
    }
    document.addEventListener('DOMContentLoaded', initImmediateSupervisorChoices);
    initImmediateSupervisorChoices();


    let headOfDepartmentChoices;
    function initHeadOfDepartmentChoices() {
        headOfDepartmentChoices = new Choices('#HeadOfDepartmentId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Head of Department'
        });
    }
    document.addEventListener('DOMContentLoaded', initHeadOfDepartmentChoices);
    initHeadOfDepartmentChoices();



    let timeUnitChoices;
    function initTimeUnitChoices() {
        timeUnitChoices = new Choices('#ProvisionPeriodTtimeTypeID', {
            removeItemButton: false,
            shouldSort: false,
            placeholderValue: 'Select Unit'
        });
    }
    document.addEventListener('DOMContentLoaded', initTimeUnitChoices);
    initTimeUnitChoices();


    let employmentStatusChoices;
    function initEmploymentStatusChoices() {
        employmentStatusChoices = new Choices('#EmploymentStatusId', {
            removeItemButton: false,
            shouldSort: false,
            placeholderValue: 'Select Employee Status'
        });
    }
    document.addEventListener('DOMContentLoaded', initEmploymentStatusChoices);
    initEmploymentStatusChoices();


    //#endregion


    //#region Validation
    function isValidEmail(email) {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }

    function showError(fieldName, message) {
        // Try multiple ways to find the input
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

        // Remove existing error first
        removeError(fieldName);

        // Add red border to input/select
        $input.addClass('border-danger').css('border-color', '#dc3545');

        // Add new error message with CSS to prevent layout shift
        //const $error = $('<span class="text-danger d-block mt-1 validation-error" data-field="' + fieldName + '" style="position: absolute; z-index: 1000; background: white; padding: 2px 4px; border-radius: 3px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); font-size: 12px; white-space: nowrap;">' + message + '</span>');
        const $error = $('<span class="text-danger d-block mt-1 validation-error" data-field="' + fieldName + '" style=" padding: 2px 4px; border-radius: 3px;  font-size: 12px; white-space: nowrap;">' + message + '</span>');

        // Find the best place to insert the error
        const $container = $input.closest('.form-floating, .flatpickr-input-container, .col-sm-12, .col-md-4');
        if ($container.length > 0) {
            $container.after($error);
        } else {
            $input.after($error);
        }
    }

    function removeError(fieldName) {
        // Remove red border from input/select
        const $input = $("input[name='" + fieldName + "'], select[name='" + fieldName + "'], #" + fieldName);
        $input.removeClass('border-danger').css('border-color', '');

        // Remove errors by field name
        $(".validation-error[data-field='" + fieldName + "']").remove();

        // Also try other methods as fallback
        $input.next(".validation-error").remove();
        $input.closest('.form-floating, .flatpickr-input-container').next(".validation-error").remove();
    }

    function clearErrors() {
        $(".validation-error").remove();
    }

    function attachInputValidationHandler() {
        // Handle input fields
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

        // Handle specific input fields by name
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
            }
        );
    }

    // Attach live validation for input fields
    attachInputValidationHandler();

    // Attach validation for select dropdowns
    $(document).on("change", "select", function () {
        const fieldName = $(this).attr("name");
        const value = $(this).val();

        if (value && value.trim() !== "") {
            removeError(fieldName);
        }
    });

    // Add form submission validation
    function validateOfficialForm() {
        clearErrors();
        let isValid = true;

        // Required select fields
        const requiredSelects = [
            { name: "EmployeePersonalId", label: "Employee" },
            //{ name: "OrganizationID", label: "Organization" },
            //{ name: "OrganizationBranchID", label: "Branch" },
            //{ name: "EmployeeTypeID", label: "Employee Type" },
            //{ name: "DepartmentID", label: "Department" },
            //{ name: "DesignationID", label: "Designation" },
            //{ name: "EmploymentNatureID", label: "Employment Nature" },
            //{ name: "EmploymentStatusId", label: "Employee Status" }
        ];

        // Required input fields
        const requiredInputs = [
            { name: "OfficePhone", label: "Office Phone" },
            { name: "OfficeEmail", label: "Office Email", type: "email" },
            { name: "AttendanceId", label: "Attendance ID" },
            { name: "JoiningDate", label: "Joining Date" },
            { name: "AppointmentLetterNo", label: "Appointment Letter No" },
            { name: "ConfirmationLetterNo", label: "Confirmation Letter No" }
        ];

        // Validate select fields
        requiredSelects.forEach(field => {
            const $select = $("select[name='" + field.name + "']");
            const value = $select.val();

            if (!value || value.trim() === "") {
                showError(field.name, field.label + " is required.");
                isValid = false;
            }
        });

        // Validate input fields
        requiredInputs.forEach(field => {
            const $input = $("input[name='" + field.name + "']");

            if ($input.length === 0) {
                console.error("Could not find input field: " + field.name);
                return;
            }

            const value = $input.val().trim();
            console.log("Validating field:", field.name, "Value:", value); // Debug log

            if (!value) {
                showError(field.name, field.label + " is required.");
                isValid = false;
            } else if (field.type === "email" && !isValidEmail(value)) {
                showError(field.name, "Invalid email format.");
                isValid = false;
            }
        });

        // Validate probation period logic
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

    //#endregion

    //#region Submit
   
        $('#employeeOfficialForm').on('submit', function (e) {
            e.preventDefault(); 

            if (!validateOfficialForm()) {
                // Scroll to first error
                const firstError = $('.validation-error').first();
                if (firstError.length) {
                    $('html, body').animate({
                        scrollTop: firstError.offset().top - 100
                    }, 500);
                }
                return;
            }

            var form = $(this);
            var formData = form.serialize(); // Serialize form data

            // Show loading state
            const $submitBtn = form.find('button[type="submit"]');
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
                    debugger
                    if (response.success) {
                        toastr.success(response.message)
                        window.location.href = '/EmployeeSalary/Index/'+ response.data; 
                    } else {
                        toastr.warning(response.message)
                    }
                     
                },
                error: function (xhr) {
                    console.error('Error:', xhr);
                    if (xhr.responseJSON && xhr.responseJSON.errors) {
                        // Handle validation errors from server
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
                    // Restore button state
                    $submitBtn.prop('disabled', false).text(originalText);
                }
            });
        });
   
    //#endregion

    //#region On change


    // Enhanced employee change handler with better dropdown population
    $("#EmployeePersonalId").change(function () {
        var selectedId = $(this).val();


        TabChange(selectedId) // this function is located in EmployeeTabChange.js
        LoadEmployeeOfficData(selectedId)

    });
    function LoadEmployeeOfficData(selectedId) {

        if (selectedId) {


            // Show loading indicator
            $(".form-control").prop('disabled', true);

            $.ajax({
                url: "/EmployeeOfficial/GetEmployeeDetails",
                type: "GET",
                data: { id: selectedId },
                success: function (response) {
                    console.log("Response received:", response);

                    // Populate dropdowns using Choices.js instances
                    populateChoicesDropdowns(response);

                    // Populate regular form fields
                    populateFormFields(response);

                    // Populate date fields
                    populateDateFields(response);

                    // Handle hidden fields
                    if (response.employeeOfficeInfoID) {
                        $("input[name='EmployeeOfficeInfoID']").val(response.employeeOfficeInfoID);
                    }

                    // Re-enable form controls
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
    };

    // Function to populate Choices.js dropdowns
    function populateChoicesDropdowns(response) {
        const dropdownMappings = [
            { instance: organizationChoices, value: response.organizationID },
            { instance: branchChoices, value: response.organizationBranchID },
            { instance: employeeTypeChoices, value: response.employeeTypeID },
            { instance: departmentChoices, value: response.departmentID },
            { instance: designationChoices, value: response.designationID },
            { instance: employmentNatureChoices, value: response.employmentNatureID },
            { instance: seniorSupervisorChoices, value: response.seniorSupervisorId },
            { instance: immediateSupervisorChoices, value: response.immediateSupervisorId },
            { instance: headOfDepartmentChoices, value: response.headOfDepartmentId },
            { instance: timeUnitChoices, value: response.provisionPeriodTtimeTypeID },
            { instance: employmentStatusChoices, value: response.employmentStatusId }
        ];

        dropdownMappings.forEach(mapping => {
            if (mapping.instance && mapping.value) {
                try {
                    // Method 1: Using Choices.js setChoiceByValue
                    if (mapping.instance.setChoiceByValue) {
                        mapping.instance.setChoiceByValue(mapping.value.toString());
                    }
                    // Method 2: Fallback - set the underlying select value and reinitialize
                    else {
                        const selectElement = mapping.instance.passedElement.element;
                        $(selectElement).val(mapping.value);
                        mapping.instance.destroy();
                        mapping.instance.init();
                    }
                } catch (error) {
                    console.error("Error setting dropdown value:", error);
                    // Fallback: Set using jQuery and trigger change
                    const selectId = mapping.instance.passedElement.element.id;
                    $(`#${selectId}`).val(mapping.value).trigger('change');
                }
            }
        });
    }

    // Alternative method for Choices.js dropdowns if the above doesn't work
    function populateChoicesDropdownsAlternative(response) {
        const dropdownData = {
            '#OrganizationID': response.organizationID,
            '#OrganizationBranchID': response.organizationBranchID,
            '#EmployeeTypeID': response.employeeTypeID,
            '#DepartmentID': response.departmentID,
            '#DesignationID': response.designationID,
            '#EmploymentNatureID': response.employmentNatureID,
            '#SeniorSupervisorId': response.seniorSupervisorId,
            '#ImmediateSupervisorId': response.immediateSupervisorId,
            '#HeadOfDepartmentId': response.headOfDepartmentId,
            '#ProvisionPeriodTtimeTypeID': response.provisionPeriodTtimeTypeID,
            '#EmploymentStatusId': response.employmentStatusId
        };

        Object.entries(dropdownData).forEach(([selector, value]) => {
            if (value) {
                const $select = $(selector);

                // First, set the value on the original select element
                $select.val(value);

                // Get the Choices instance
                const choicesInstance = getChoicesInstance(selector);

                if (choicesInstance) {
                    // Method 1: Use setChoiceByValue
                    try {
                        choicesInstance.setChoiceByValue(value.toString());
                    } catch (e) {
                        // Method 2: Refresh choices and set value
                        try {
                            choicesInstance.clearStore();
                            choicesInstance.setChoices([]);
                            $select.val(value).trigger('change');
                        } catch (e2) {
                            console.error('Failed to set choice:', selector, value, e2);
                        }
                    }
                }
            }
        });
    }

    // Helper function to get Choices instance by selector
    function getChoicesInstance(selector) {
        const elementId = selector.replace('#', '');

        const instanceMap = {
            'OrganizationID': organizationChoices,
            'OrganizationBranchID': branchChoices,
            'EmployeeTypeID': employeeTypeChoices,
            'DepartmentID': departmentChoices,
            'DesignationID': designationChoices,
            'EmploymentNatureID': employmentNatureChoices,
            'SeniorSupervisorId': seniorSupervisorChoices,
            'ImmediateSupervisorId': immediateSupervisorChoices,
            'HeadOfDepartmentId': headOfDepartmentChoices,
            'ProvisionPeriodTtimeTypeID': timeUnitChoices,
            'EmploymentStatusId': employmentStatusChoices
        };

        return instanceMap[elementId];
    }

    // Function to populate regular form fields
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

    // Function to populate date fields
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
                // Also set the name-based input if different
                $(`input[name='${responseKey}']`).val(formattedDate);
            }
        });
    }

    // Enhanced date formatting function
    function formatDateForFlatpickr(dateString) {
        if (!dateString) return '';

        try {
            let date;

            if (dateString instanceof Date) {
                date = dateString;
            } else if (typeof dateString === 'string') {
                // Handle C# DateTime JSON format like "/Date(1234567890000)/"
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

            // Format as YYYY-MM-DD for flatpickr
            return date.toISOString().split('T')[0];

        } catch (error) {
            console.error('Error formatting date:', dateString, error);
            return '';
        }
    }

    // Function to clear all form fields
    function clearAllFormFields() {
        // Clear text inputs
        $("input[name='PersonalEmail'], input[name='PersonalPhone'], input[name='OfficePhone']").val('');
        $("input[name='OfficeEmail'], input[name='AttendanceId']").val('');
        $("input[name='AppointmentLetterNo'], input[name='ConfirmationLetterNo'], input[name='ProvisionPeriod']").val('');

        // Clear date inputs
        $("input[name='AppointmentLetterIssueDate'], input[name='JoiningDate']").val('');
        $("input[name='ProvisionPeriodStartDate'], input[name='ConfirmationDate'], input[name='ContractEndDate']").val('');
        $("#floatingInputAppointmentDate, #floatingInputJoiningDate, #floatingInputProbationStartDate").val('');
        $("#floatingInputConfirmationDate, #floatingInputContractEndDate").val('');

        // Clear Choices.js dropdowns
        const choicesInstances = [
            organizationChoices, branchChoices, employeeTypeChoices, departmentChoices,
            designationChoices, employmentNatureChoices, seniorSupervisorChoices,
            immediateSupervisorChoices, headOfDepartmentChoices, timeUnitChoices, employmentStatusChoices
        ];

        choicesInstances.forEach(instance => {
            if (instance && instance.removeActiveItems) {
                instance.removeActiveItems();
            }
        });

        // Clear hidden fields
        $("input[name='EmployeeOfficeInfoID']").val('');
    }

    // Debug function to check dropdown states
    function debugDropdownStates() {
        console.log('Dropdown instances status:');
        console.log('organizationChoices:', organizationChoices);
        console.log('branchChoices:', branchChoices);
        console.log('employeeTypeChoices:', employeeTypeChoices);
        console.log('departmentChoices:', departmentChoices);
        console.log('designationChoices:', designationChoices);
        console.log('employmentNatureChoices:', employmentNatureChoices);
        console.log('seniorSupervisorChoices:', seniorSupervisorChoices);
        console.log('immediateSupervisorChoices:', immediateSupervisorChoices);
        console.log('headOfDepartmentChoices:', headOfDepartmentChoices);
        console.log('timeUnitChoices:', timeUnitChoices);
        console.log('employmentStatusChoices:', employmentStatusChoices);
    }

    // Alternative approach - Force reinitialize dropdowns after population
    function reinitializeDropdownsWithValues(response) {
        // Store values to set
        const valuesToSet = {
            'OrganizationID': response.organizationID,
            'OrganizationBranchID': response.organizationBranchID,
            'EmployeeTypeID': response.employeeTypeID,
            'DepartmentID': response.departmentID,
            'DesignationID': response.designationID,
            'EmploymentNatureID': response.employmentNatureID,
            'SeniorSupervisorId': response.seniorSupervisorId,
            'ImmediateSupervisorId': response.immediateSupervisorId,
            'HeadOfDepartmentId': response.headOfDepartmentId,
            'ProvisionPeriodTtimeTypeID': response.provisionPeriodTtimeTypeID,
            'EmploymentStatusId': response.employmentStatusId
        };

        // Set values on original select elements first
        Object.entries(valuesToSet).forEach(([elementId, value]) => {
            if (value) {
                $(`#${elementId}`).val(value);
            }
        });

        // Destroy and reinitialize Choices instances
        setTimeout(() => {
            if (organizationChoices) {
                organizationChoices.destroy();
                initOrganizationChoices();
            }
            if (branchChoices) {
                branchChoices.destroy();
                initBranchChoices();
            }
            if (employeeTypeChoices) {
                employeeTypeChoices.destroy();
                initEmployeeTypeChoices();
            }
            if (departmentChoices) {
                departmentChoices.destroy();
                initDepartmentChoices();
            }
            if (designationChoices) {
                designationChoices.destroy();
                initDesignationChoices();
            }
            if (employmentNatureChoices) {
                employmentNatureChoices.destroy();
                initEmploymentNatureChoices();
            }
            if (seniorSupervisorChoices) {
                seniorSupervisorChoices.destroy();
                initSeniorSupervisorChoices();
            }
            if (immediateSupervisorChoices) {
                immediateSupervisorChoices.destroy();
                initImmediateSupervisorChoices();
            }
            if (headOfDepartmentChoices) {
                headOfDepartmentChoices.destroy();
                initHeadOfDepartmentChoices();
            }
            if (timeUnitChoices) {
                timeUnitChoices.destroy();
                initTimeUnitChoices();
            }
            if (employmentStatusChoices) {
                employmentStatusChoices.destroy();
                initEmploymentStatusChoices();
            }
        }, 100);
    }


    //#endregion


    //#region On change New

    $("#OrganizationID").change(function () {
        var selectedId = $(this).val();
        GetBranches(selectedId); 
    });

    function GetBranches(selectedId) {
        $.ajax({
            url: '/EmployeeOfficial/GetBranches', 
            type: 'GET',
            data: { id: selectedId }, 
            success: function (response) {
                console.log('Branches fetched:', response);

                choiceManager.populateDropdown('OrganizationBranchID', response, { placeholder: 'Custom Select a Branch' })
            },
            error: function (xhr, status, error) {
                console.error('Error fetching branches:', error);
            }
        });
    }


    //#endregion

});