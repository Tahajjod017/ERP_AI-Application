$(document).ready(function () {

    $('#addNewLeaveForm').on('submit', function (e) {
        e.preventDefault();
        if (!validateLeaveForm()) return;
        var formData = {
            OrganizationID: $('#OrganizationID').val(),
            LeaveTypeName: $('#LeaveTypeName').val(),
            //IsApid: $('#IsApid').val() === 'true',
            IsApid: $('input[name="IsApid"]:checked').val() === 'true',
            LeaveDays: $('#LeaveDays').val(),
            Code: $('#Code').val()
        };

        $.ajax({
            url: '/LeaveSettings/AddNewLeave', 
            type: 'POST',
            data: formData,
            success: function (response) {
                console.log("Response:", response);
                if (response.success) {
                    toastr.success(response.message);
                    resetForm();
                } else {
                    // Show server-side validation errors
                    if (response.errors && response.errors.length > 0) {
                        response.errors.forEach(function (error) {
                            toastr.error(error);
                        });
                    } else {
                        toastr.error(response.message);
                    }
                }
            },
            error: function () {
                toastr.error("An unexpected error occurred.");
            }
        });
    });

    function resetForm() {
        debugger
        $('#LeaveTypeName, #LeaveDays, #Code').val('');
        $('#IsApidPaid').prop('checked', true);
        $('#OrganizationID').val([]).trigger('change');
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

    function validateLeaveForm() {
        // Clear previous error styles/messages
        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();

        let isValid = true;

        if (!$('#OrganizationID').val()) {
            $('#OrganizationID').addClass('is-invalid')
                .after('<div class="text-danger">This field is required</div>');
            isValid = false;
        }

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
});


