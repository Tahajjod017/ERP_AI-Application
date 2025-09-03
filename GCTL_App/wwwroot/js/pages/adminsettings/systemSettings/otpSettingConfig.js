//$('#otpForm').on('submit', function (e) {
//    e.preventDefault();

//    var form = $(this);
//    var formData = form.serialize();

//    $.ajax({
//        url: form.attr('action'),
//        method: 'POST',
//        data: formData,
//        success: function (response) {
//            if (response.isSuccess) {
//                toastr.success(response.message, '');
//                form.trigger("reset");
//            } else {
//                toastr.error(response.message, 'Error');
//            }
//        },
//        error: function (xhr, status, error) {
//            toastr.error("Unexpected error: " + error, 'Server Error');
//        }
//    });
//});

$('#otpForm').on('submit', function (e) {
    e.preventDefault();

    var form = $(this);
    var formData = form.serialize();

    // Simulate a successful response
    setTimeout(function () {
        var mockResponse = {
            isSuccess: true,
            message: "Submission successful"
        };

        toastr.success(mockResponse.message, '');
        form.trigger("reset");
    }, 500); // slight delay to mimic async behavior
});