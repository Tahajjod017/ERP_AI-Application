$('body').on('submit', '#LeaveRequestForm', function (e) {
    e.preventDefault();

    var $form = $(this);
    var url = $form.attr('action');
    var formData = new FormData(this);
    $.ajax({
        type: 'POST',
        url: url,
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            console.log("Dataaaaaaaaaa", response);
            debugger
            if (response.success) {
                toastr.success(response.message);
            }
        }, error: function () {
            toastr.error(response.message);
        }
    });
});