$(document).ready(function () {
    $('#changePasswordForm').submit(function (e) {
        e.preventDefault();

        const formData = $(this).serialize();

        $.ajax({
            url: '/UserProfile/ChangePassword',
            type: 'POST',
            data: formData,
            success: function (response) {
                // Clear old errors
                $('span[data-valmsg-for]').text('');

                if (response.success) {
                    toastr.success(response.message, 'Success');
                    // ✅ Clear the password fields
                    $('#changePasswordForm').trigger('reset');

                    // ✅ Also clear validation messages
                    $('span[data-valmsg-for]').text('');
                } else if (response.fieldErrors) {
                    $.each(response.fieldErrors, function (fieldName, errorMessage) {
                        $(`span[data-valmsg-for="${fieldName}"]`).text(errorMessage);
                    });

                    toastr.error('Please fix the errors.', 'Validation Failed');
                }
            },
            error: function () {
                toastr.error('An error occurred.', 'Error');
            }
        });
    });
});

