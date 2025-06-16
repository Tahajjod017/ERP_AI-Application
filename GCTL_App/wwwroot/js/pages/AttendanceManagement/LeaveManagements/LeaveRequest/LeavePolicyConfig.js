$(document).ready(function () {


    function resetForm() {
        $('#LeaveTypeName, #LeaveDays, #Code, #EffectiveFrom').val('');
        $('#IsPaidYes').prop('checked', true);
        $('#IsActiveYes').prop('checked', true);

        // Reset dropdowns
        $('#OrganizationID').val([]).trigger('change');
        $('#EffectiveFromMonthYear').val('Months').trigger('change');
        $('#EffectiveAfter').val('After Joining Date');

        // Clear validation styles and messages
        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();

    }

    $('#resetBtn').on('click', function () {
        resetForm();
    });

    // Validation

    $('#OrganizationID, #LeaveTypeName, #Code, #LeaveDays').on('input change', function () {
        let value = $(this).val();

        if (Array.isArray(value)) {
            if (value.length > 0) {
                $(this).removeClass('is-invalid');
                $(this).next('.text-danger').remove();
            }
        } else if (typeof value === 'string' && value.trim() !== '') {
            $(this).removeClass('is-invalid');
            $(this).next('.text-danger').remove();
        }
    });

    $('#OrganizationID').on('change', function () {
        const $select = $(this);
        const value = $select.val();

        // Target the CoreUI wrapper (adjust class based on actual CoreUI structure)
        const $visibleWrapper = $select.next('.c-multi-select, .coreui-multiselect');

        if (Array.isArray(value) && value.length > 0) {
            $visibleWrapper.removeClass('is-invalid');
            $visibleWrapper.next('.text-danger').remove();
        }
    });


    //
    function validateLeaveForm() {

        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();

        let isValid = true;
        if (!$('#OrganizationID').val() || $('#OrganizationID').val().length === 0) {
            const $select = $('#OrganizationID');
            const $visibleWrapper = $select.next();

            $visibleWrapper.addClass('is-invalid');
            if ($visibleWrapper.next('.text-danger').length === 0) {
                $visibleWrapper.after('<div class="text-danger">This field is required</div>');
            }

            isValid = false;
        }


        //
        if (!$('#LeaveTypeName').val().trim()) {
            $('#LeaveTypeName').addClass('is-invalid')
                .after('<div class="text-danger">This field is required</div>');
            isValid = false;
        }

        if (!$('#Code').val().trim()) {
            $('#Code').addClass('is-invalid')
                .after('<div class="text-danger">This field is required</div>');
            isValid = false;
        }

        if (!$('#LeaveDays').val()) {
            $('#LeaveDays').addClass('is-invalid')
                .after('<div class="text-danger">This field is required</div>');
            isValid = false;
        }

        return isValid;
    }



    //



    //Update Leave Data

    $('#AddLeavePolicyBtn').on('click', function (e) {
        e.preventDefault();
        if (!validateLeaveForm()) return;
        const leaveData = {
            LeaveTypeID: $('#LeaveTypeID').val(),
            LeaveTypeName: $('#LeaveTypeName').val(),
            //OrganizationID: $('#OrganizationID').val(),
            IsPaid: $('input[name="IsPaid"]:checked').val() === 'true',
            IsActive: $('input[name="IsActive"]:checked').val() === 'true',
           
        };

        $.ajax({
            url: '/LeaveSettings/LeavePolicyConfig', // Replace with your controller name
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(leaveData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    resetLeaveForm();
                    // Optionally reload data or close modal
                } else {
                    toastr.error(response.message);
                    if (response.errors) {
                        response.errors.forEach(error => toastr.warning(error));
                    }
                }
            },
            error: function () {
                toastr.error("An error occurred while sending data.");
            }
        });
    });
    //



});


