$(document).ready(function () {
    //#region employeeChoices with onchange

    let employeeChoices;
    function initEmployeeChoices() {
        employeeChoices = new Choices('#EmployeePersonalId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Employee'
        });

        // Add event listener for employee selection change
        const employeeElement = document.getElementById('EmployeePersonalId');
        if (employeeElement) {
            employeeElement.addEventListener('change', function (e) {
                const selectedEmployeeId = e.detail.value || e.target.value;
                if (selectedEmployeeId && selectedEmployeeId !== '') {
                    loadEmployeeSalaryData(selectedEmployeeId);
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

    //#region Choice Min

    let gradeChoices;
    function initGradeChoices() {
        gradeChoices = new Choices('#GradeID', {
            removeItemButton: false,
            shouldSort: false,
            placeholderValue: 'Select Grade'
        });
    }
    document.addEventListener('DOMContentLoaded', initGradeChoices);
    initGradeChoices();

    let currencyChoices;
    function initCurrencyChoices() {
        currencyChoices = new Choices('#CurrencyID', {
            removeItemButton: false,
            shouldSort: false,
            placeholderValue: 'Select Currency'
        });
    }
    document.addEventListener('DOMContentLoaded', initCurrencyChoices);
    initCurrencyChoices();

    let paymentPeriodChoices;
    function initPaymentPeriodChoices() {
        paymentPeriodChoices = new Choices('#PaymenPeriodTypeID', {
            removeItemButton: false,
            shouldSort: false,
            placeholderValue: 'Select Payment Period'
        });
    }
    document.addEventListener('DOMContentLoaded', initPaymentPeriodChoices);
    initPaymentPeriodChoices();

    //#endregion

    //#region Payment Mode Choice

    let paymentModeChoices;

    function initPaymentModeChoices() {
        const element = document.getElementById('PaymentModeId');

        if (!element) {
            console.error('PaymentModeId element not found');
            return;
        }

        if (paymentModeChoices) {
            paymentModeChoices.destroy();
        }

        try {
            paymentModeChoices = new Choices(element, {
                removeItemButton: true,
                maxItemCount: 2,
                shouldSort: false,
                placeholderValue: 'Select Payment Mode',
                searchEnabled: true,
                itemSelectText: '',
                duplicateItemsAllowed: false,
                paste: false
            });

            // Add event listeners for Choices.js events
            element.addEventListener('addItem', function (event) {
                console.log('Item added:', event.detail);
                setTimeout(handlePaymentModeChange, 100);
            });

            element.addEventListener('removeItem', function (event) {
                console.log('Item removed:', event.detail);
                setTimeout(handlePaymentModeChange, 100);
            });

            element.addEventListener('change', function (event) {
                console.log('Change event fired');
                setTimeout(handlePaymentModeChange, 100);
            });
        } catch (error) {
            console.error('Error initializing Choices.js:', error);
            element.addEventListener('change', handlePaymentModeChangeNative);
        }
    }

    function handlePaymentModeChange() {
        if (!paymentModeChoices) {
            handlePaymentModeChangeNative();
            return;
        }

        try {
            const selectedItems = paymentModeChoices.getValue(true);
            console.log('Selected items:', selectedItems);

            const validSelectedItems = selectedItems.filter(item => item && item !== '');
            const selectedCount = validSelectedItems.length;

            console.log('Valid selected count:', selectedCount);

            const multyPaymentDiv = document.getElementById('multyPayment');
            const primarySelect = document.getElementById('primaryPaymentMode');
            const secondarySelect = document.getElementById('secondaryPaymentMode');
            const percentageInput = document.getElementById('primaryPaymentPercentage');

            if (selectedCount === 2) {
                if (multyPaymentDiv) multyPaymentDiv.style.display = 'block';

                const [primary, secondary] = validSelectedItems;

                if (primarySelect) primarySelect.value = primary;
                if (secondarySelect) secondarySelect.value = secondary;

                if (percentageInput && !percentageInput.value) {
                    percentageInput.value = '50';
                }

                console.log('Multi-payment enabled with:', primary, secondary);
            } else {
                if (multyPaymentDiv) multyPaymentDiv.style.display = 'none';
                if (primarySelect) primarySelect.value = '';
                if (secondarySelect) secondarySelect.value = '';
                if (percentageInput) percentageInput.value = '';

                console.log('Multi-payment disabled');
            }
        } catch (error) {
            console.error('Error in handlePaymentModeChange:', error);
            handlePaymentModeChangeNative();
        }
    }

    function handlePaymentModeChangeNative() {
        const element = document.getElementById('PaymentModeId');
        if (!element) return;

        const selectedOptions = Array.from(element.selectedOptions).filter(opt => opt.value !== '');
        const selectedCount = selectedOptions.length;

        console.log('Native handling - selected count:', selectedCount);

        const multyPaymentDiv = document.getElementById('multyPayment');
        const primarySelect = document.getElementById('primaryPaymentMode');
        const secondarySelect = document.getElementById('secondaryPaymentMode');
        const percentageInput = document.getElementById('primaryPaymentPercentage');

        if (selectedCount === 2) {
            if (multyPaymentDiv) multyPaymentDiv.style.display = 'block';

            const [primary, secondary] = selectedOptions.map(opt => opt.value);

            if (primarySelect) primarySelect.value = primary;
            if (secondarySelect) secondarySelect.value = secondary;

            if (percentageInput && !percentageInput.value) {
                percentageInput.value = '50';
            }
        } else {
            if (multyPaymentDiv) multyPaymentDiv.style.display = 'none';
            if (primarySelect) primarySelect.value = '';
            if (secondarySelect) secondarySelect.value = '';
            if (percentageInput) percentageInput.value = '';
        }
    }

    function safeInitialize() {
        try {
            initPaymentModeChoices();
        } catch (error) {
            console.error('Failed to initialize Choices.js, using native select:', error);
            const element = document.getElementById('PaymentModeId');
            if (element) {
                element.addEventListener('change', handlePaymentModeChangeNative);
            }
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', safeInitialize);
    } else {
        safeInitialize();
    }

    setTimeout(safeInitialize, 100);

    document.addEventListener('DOMContentLoaded', function () {
        const percentageInput = document.getElementById('primaryPaymentPercentage');
        if (percentageInput) {
            percentageInput.addEventListener('input', function (e) {
                let value = parseInt(e.target.value);
                if (isNaN(value) || value < 1) {
                    e.target.value = '1';
                } else if (value > 99) {
                    e.target.value = '99';
                }
            });
        }
    });

    window.debugPaymentMode = function () {
        console.log('PaymentModeChoices:', paymentModeChoices);
        console.log('Element:', document.getElementById('PaymentModeId'));
        console.log('MultiPayment div:', document.getElementById('multyPayment'));
        if (paymentModeChoices) {
            console.log('Selected values:', paymentModeChoices.getValue(true));
        }
    };

    //#endregion

    //#region Form Submission

    //$('form').on('submit', function (e) {
    //    // Allow default form submission to asp-action="Index" asp-controller="EmployeeSalary"
    //    showLoadingIndicator();
    //    $(this).attr('action', '/EmployeeSalary/Index');
    //    $(this).attr('method', 'post');
        
    //});


    $('form').on('submit', function (e) {
        e.preventDefault(); // Prevent default form submission

        showLoadingIndicator(); // Show loading indicator if needed

        $.ajax({
            url: '/EmployeeSalary/Index',
            type: 'POST',
            data: $(this).serialize(), // Serialize form data
            success: function (response) {
                hideLoadingIndicator()
                console.log('Form submitted successfully', response);
                if (response.success) {
                    toastr.success(response.message)
                    window.location.href = '/EmployeeBenifit/Index/' + response.data;
                } else {
                    toastr.warning(response.message)
                }
            },
            error: function (xhr, status, error) {
                // Handle error (e.g., show error message)
                console.error('Error submitting form', error);
            }
        });
    });

    //#endregion

    //#region Employee Data Loading

    function loadEmployeeSalaryData(employeeId) {
        showLoadingIndicator();

        $.ajax({
            url: '/EmployeeSalary/GetEmployeeSalaryData',
            type: 'GET',
            data: { employeeId: employeeId },
            success: function (response) {
                if (response.success) {
                    populateForm(response.data);
                } else {
                    console.error('Error loading employee data:', response.message);
                    showNotification('Error loading employee data: ' + (response.message || 'Unknown error'), 'error');
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error:', error);
                showNotification('Failed to load employee data. Please try again.', 'error');
            },
            complete: function () {
                hideLoadingIndicator();
            }
        });
    }

    function populateForm(data) {
        try {
            $('#PersonalPhone').val(data.personalPhone || '');
            $('#PersonalEmail').val(data.personalEmail || '');
            $('#BankName').val(data.bankName || '');
            $('#BranchName').val(data.branchName || '');
            $('#Address').val(data.address || '');
            $('#AccountName').val(data.accountName || '');
            $('#AccountNo').val(data.accountNo || '');
            $('#ATMCardNo').val(data.atmCardNo || '');
            $('#RoutingNo').val(data.routingNo || '');
            $('#SWIFTCode').val(data.swiftCode || '');
            $('#IFSCCode').val(data.ifscCode || '');
            $('#bKashAccountNo').val(data.bKashAccountNo || '');
            $('#RoketAccountNo').val(data.roketAccountNo || '');
            $('#NagodAccountNo').val(data.nagodAccountNo || '');
            $('#EmployeeGID').val(data.employeeGID || '');
            $('#Salary').val(data.salary || '');

            if (data.gradeID && gradeChoices) {
                gradeChoices.setChoiceByValue(data.gradeID.toString());
            }
            if (data.currencyID && currencyChoices) {
                currencyChoices.setChoiceByValue(data.currencyID.toString());
            }
            if (data.paymenPeriodTypeID && paymentPeriodChoices) {
                paymentPeriodChoices.setChoiceByValue(data.paymenPeriodTypeID.toString());
            }

            if (data.paymentModeIds && data.paymentModeIds.length > 0 && paymentModeChoices) {
                paymentModeChoices.removeActiveItems();
                data.paymentModeIds.forEach(id => {
                    paymentModeChoices.setChoiceByValue(id.toString());
                });
            }

            if (data.primaryPaymentModeId) {
                $('#primaryPaymentMode').val(data.primaryPaymentModeId);
            }
            if (data.primaryPaymentPercent) {
                $('#primaryPaymentPercentage').val(data.primaryPaymentPercent);
            }
            if (data.secondaryPaymentModeId) {
                $('#secondaryPaymentMode').val(data.secondaryPaymentModeId);
            }

            $('input[name="EmployeeSalarySettingsID"]').val(data.employeeSalarySettingsID || '');

            showNotification('Employee data loaded successfully', 'success');
        } catch (error) {
            console.error('Error populating form:', error);
            showNotification('Error populating form data', 'error');
        }
    }

    function clearForm() {
        $('input[type="text"], input[type="email"], input[type="number"]').not('#EmployeePersonalId').val('');
        if (gradeChoices) gradeChoices.setChoiceByValue('');
        if (currencyChoices) currencyChoices.setChoiceByValue('');
        if (paymentPeriodChoices) paymentPeriodChoices.setChoiceByValue('');
        if (paymentModeChoices) paymentModeChoices.removeActiveItems();

        const multyPaymentDiv = document.getElementById('multyPayment');
        if (multyPaymentDiv) multyPaymentDiv.style.display = 'none';
    }

    //#endregion

    //#region Utility Functions

    function showLoadingIndicator() {
        const submitBtn = $('button[type="submit"]');
        submitBtn.prop('disabled', true);
        submitBtn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Saving...');
    }

    function hideLoadingIndicator() {
        const submitBtn = $('button[type="submit"]');
        submitBtn.prop('disabled', false);
        submitBtn.html('Save');
    }

    function showNotification(message, type = 'info') {
        const alertClass = type === 'success' ? 'alert-success' :
            type === 'error' ? 'alert-danger' :
                type === 'warning' ? 'alert-warning' : 'alert-info';

        const notification = $(`
            <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999; max-width: 400px;" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `);

        $('body').append(notification);

        setTimeout(() => {
            notification.alert('close');
        }, 5000);
    }

    window.debugPaymentMode = function () {
        console.log('PaymentModeChoices:', paymentModeChoices);
        console.log('Element:', document.getElementById('PaymentModeId'));
        console.log('MultiPayment div:', document.getElementById('multyPayment'));
        if (paymentModeChoices) {
            console.log('Selected values:', paymentModeChoices.getValue(true));
        }
    };

    //#endregion
});