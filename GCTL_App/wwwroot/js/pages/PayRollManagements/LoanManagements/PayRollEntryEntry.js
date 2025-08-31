$(document).ready(function () {

    initializeDatepickerDMY("IssueDate,StartDate")

    $(document).on('click', '#SaveButton', function (e) {
        e.preventDefault();

        // Serialize form data
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
                    toastr.error("Failed: " + response.Message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error("An error occurred: " + error);
            }
        });
    });

    $(document).on('click', '#ResetButton', function (e) {
        e.preventDefault();
        resetForm();
    });

    // Reset Function
    function resetForm() {
        var $form = $('form');

        // Reset native form fields
        $form[0].reset();
        choiceManager.resetChoice('LoanInstallmentPeriodID')
        // Reset multi-selects (CoreUI)
        $form.find('.form-multi-select').each(function () {
            $(this).val([]).trigger('change');
        });

        // Clear flatpickr datepickers
        $form.find('.datetimepicker').each(function () {
            if (this._flatpickr) {
                this._flatpickr.clear();
            } else {
                $(this).val('');
            }
        });
    }


})


