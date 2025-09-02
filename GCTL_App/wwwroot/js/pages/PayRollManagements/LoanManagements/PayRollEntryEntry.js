$(document).ready(function () {


    initializeDatepickerDMY("IssueDate");
    initializeMonthYearPicker(".StartDateMOnthYear");
    function calculateLoanSummary() {
        let loanAmount = parseFloat($("#LoanAmount").val()) || 0;
        let months = parseInt($("#LoanInstallmentPeriodID").val()) || 0;
        let startDate = $("#StartDate").val();

        if (loanAmount > 0 && months > 0 && startDate) {
            let perMonth = (loanAmount / months).toFixed(2);
            let date = new Date(startDate);
            if (!isNaN(date)) {
                date.setMonth(date.getMonth() + months - 1); // End date (inclusive)
                let endMonth = date.toLocaleString('default', { month: 'long' });
                let endYear = date.getFullYear();
                $("#loanSummary").text(
                    `Per Month Payment:${perMonth} tk/month. 
                    It will end in ${endMonth} ${endYear}.`
                );
            }
        } else {
            $("#loanSummary").text("");
        }
    }

    $(document).on("change keyup", "#LoanAmount, #LoanInstallmentPeriodID, #StartDate", calculateLoanSummary);

    $(document).on('click', '#SaveButton', function (e) {
        e.preventDefault();

        //if (!validateLoanForm()) {
        //    return; 
        //}
        var formData = $('form').serialize();

        $.ajax({
            url: '/PayRollLoanEntry/SaveAsync', // Your controller route
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                resetForm() 
                } else {
                    toastr.error("Failed: " + response.message);
                }
                if (response.errors) {
                    response.errors.forEach(err => toastr.error(err));
                } 
            },
            error: function (xhr, status, error) {
                toastr.error("An error occurred: " + error);
            }
        });
    });

    //
    // Separate validation method
    function validateLoanForm() {
        let isValid = true;
        // EmployeeID validation (must select at least one)
        let $employeeSelect = $('select[asp-for="EmployeeIDs"]');
        if ($employeeSelect.length === 0) {
            console.error("EmployeeIDs select element not found!");
            toastr.error("Form configuration error: Employee select not found.");
            return false;
        }

        let employeeIDs = $employeeSelect.val();
        console.log("Selected EmployeeIDs:", employeeIDs); // Debug output
        if (!employeeIDs || employeeIDs.length === 0) {
            toastr.error("Please select at least one Employee.");
            isValid = false;
            return isValid;
        }

        // LoanAmount validation
        let loanAmount = $('input[asp-for="LoanAmount"]').val();
        if (!loanAmount) {
            toastr.error("Loan Amount is required.");
            isValid = false;
            return isValid;
        }
        if (isNaN(loanAmount))
        {
            toastr.error("Loan Amount must be a valid number.");
            isValid = false;
            return isValid;
        }
        if (parseFloat(loanAmount) <= 0)
        {
            toastr.error("Loan Amount must be greater than 0.");
            isValid = false;
            return isValid;
        }

        // Installment Period validation
        let installmentPeriod = $('select[asp-for="LoanInstallmentPeriodID"]').val();
        if (!installmentPeriod) {
            toastr.error("Please select an Installment Period.");
            isValid = false;
            return isValid;
        }

        // Issue Date validation
        let issueDate = $('input[asp-for="IssueDate"]').val();
        if (!issueDate) {
            toastr.error("Issue Date is required.");
            isValid = false;
            return isValid;
        }

        // Start Date validation
        let startDate = $('input[asp-for="StartDate"]').val();
        if (!startDate) {
            toastr.error("Installment Start Date is required.");
            isValid = false;
            return isValid;
        }

        return isValid; // All validations passed
    }

    //
    $(document).on('click', '#ResetButton', function (e) {
        e.preventDefault();
        resetForm();
    });

    // Reset Function
    function resetForm() {
        var $form = $('#loanForm');
        $form[0].reset();
        choiceManager.resetChoice('LoanInstallmentPeriodID')
        choiceManager.resetChoice('OrganizationID')
        resetCoreuiMultiSelect('.form-multi-select');
        // Clear flatpickr datepickers
        $form.find('.datetimepicker').each(function () {
            if (this._flatpickr) {
                this._flatpickr.clear();
            } else {
                $(this).val('');
            }
        });
        
    }

    function resetCoreuiMultiSelect(selector) {
        document.querySelectorAll(selector).forEach(select => {
            try {
                const instance = coreui?.MultiSelect?.getInstance(select);
                if (instance && typeof instance.clear === 'function') {
                    instance.clear();
                } else {
                    // Fallback: manually deselect all options
                    Array.from(select.options).forEach(option => option.selected = false);
                    select.dispatchEvent(new Event('change'));
                }
            } catch (err) {
                console.error('Error resetting CoreUI MultiSelect:', err);
            }
        });
    }

    // #region OrganizationID on change
    $('#OrganizationID').on('change', function (e) {
        e.preventDefault();

        var organizationId = $(this).val();
        loadDepartmentsByOrganization(organizationId);
        getEmployeesByOrgBraDepId(organizationId, null);
    });
    // #endregion


    // #region DepartmentIDs on change
    document.getElementById('DepartmentIDs').addEventListener('changed.coreui.multi-select', function (event) {
        const orgId = $('#OrganizationID').val();

        const selected = event.value || []; // array of {text, value}
        const depIds = selected.map(x => parseInt(x.value));

        getEmployeesByOrgBraDepId(orgId, depIds);
    });
    // #endregion


    // #region loadDepartmentsByOrganization
    function loadDepartmentsByOrganization(organizationId) {
        $.ajax({
            url: '/AssignSpiralPattern/GetDepartmentByOrganization',
            type: 'GET',
            data: { id: organizationId },
            success: function (departments) {
                var select = $('#DepartmentIDs');
                select.empty();

                const grouped = {};

                departments.forEach(dep => {
                    const group = dep.groupName || 'No Group';
                    if (!grouped[group]) {
                        grouped[group] = [];
                    }
                    grouped[group].push(dep);
                });

                Object.keys(grouped).forEach(group => {
                    const optgroup = $('<optgroup>').attr('label', group);
                    grouped[group].forEach(dep => {
                        optgroup.append(
                            $('<option>').val(dep.id).text(dep.name)
                        );
                    });
                    select.append(optgroup);
                });

                // Get the CoreUI multiselect instance
                const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                if (multiSelectInstance) {
                    multiSelectInstance.update(); // Refresh the UI
                } else {
                    // Reinitialize if not already initialized (in case it's dynamically added)
                    new coreui.MultiSelect(select[0]);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading departments:', error);
            }
        });
    }
    // #endregion


    // #region GetEmployeesByOrgBraDepId
    function getEmployeesByOrgBraDepId(orgId, depIds = []) {
        $.ajax({
            url: '/AssignSpiralPattern/GetEmployeesByOrgBraDepId',
            type: 'GET',
            traditional: true,
            data: {
                orgId: orgId,
                depIds: depIds
            },
            success: function (employees) {
                const select = $('#EmployeeIDs');
                select.empty();

                const grouped = {};

                // Group employees by GroupName (DepartmentName)
                employees.forEach(emp => {
                    const group = emp.groupName || 'No Department';
                    if (!grouped[group]) {
                        grouped[group] = [];
                    }
                    grouped[group].push(emp);
                });

                // Build <optgroup> structure
                Object.keys(grouped).forEach(group => {
                    const optgroup = $('<optgroup>').attr('label', group);
                    grouped[group].forEach(emp => {
                        optgroup.append(
                            $('<option>').val(emp.id).text(emp.name)
                        );
                    });
                    select.append(optgroup);
                });

                const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                if (multiSelectInstance) {
                    multiSelectInstance.update(); // Refresh UI
                } else {
                    new coreui.MultiSelect(select[0]); // Init CoreUI multiselect
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading employees:', error);
            }
        });
    }
    // #endregion
})


