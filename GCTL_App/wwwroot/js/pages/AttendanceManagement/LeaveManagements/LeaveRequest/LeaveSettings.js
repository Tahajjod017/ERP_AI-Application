$(document).ready(function () {
    $('#addNewLeaveForm').on('submit', function (e) {
        e.preventDefault();

        var formData = {
            OrganizationID: $('#OrganizationID').val(),
            LeaveTypeName: $('#LeaveTypeName').val(),
            IsApid: $('#IsApid').val() === 'true',
            LeaveDays: $('#LeaveDays').val()
        };

        $.ajax({
            url: '/LeaveSettings/AddNewLeave', // Replace with actual controller
            type: 'POST',
            data: formData,
            success: function (response) {
                console.log("Response:", response);
                if (response.success) {
                    toastr.success(response.message);
                    resetForm(); // Reset after successful save
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
       
        $('#OrganizationID').val([]).trigger('change'); 
        $('#IsApid').val([]).trigger('change');        

        // Reset text inputs
        $('#LeaveTypeName').val('');
        $('#LeaveDays').val('');

        // Optional: Reset form validation messages (if any)
        $('.field-validation-error').text('');
    }

});