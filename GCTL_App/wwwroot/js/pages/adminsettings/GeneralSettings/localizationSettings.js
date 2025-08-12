$('#localizationForm').on('submit', function (e) {
    e.preventDefault();

    var form = $(this);
    var formData = form.serialize();

    $.ajax({
        url: form.attr('action'),
        method: 'POST',
        data: formData,
        success: function (response) {
            if (response.isSuccess) {
                toastr.success(response.message, '');
                form.trigger("reset");
            } else {
                toastr.error(response.message, 'Error');
            }
        },
        error: function (xhr, status, error) {
            // Handle Access Denied error (403)
            if (xhr.status === 403 && xhr.responseJSON && xhr.responseJSON.message === "Access denied.") {
                window.location.href = '/Home/AccessDenied';
            } else {
                toastr.error("Unexpected error: " + error, 'Server Error');
            }
        }
    });
});
