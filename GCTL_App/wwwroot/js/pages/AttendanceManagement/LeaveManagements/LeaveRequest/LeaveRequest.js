$(document).ready(function () {

    // Handle form submit
    $('body').on('submit', '#LeaveRequestForm', function (e) {
        e.preventDefault();

        var $form = $(this);
        if (!$form.valid()) {
            return false;
        }
        $('#Reason').hide().text('');
        var reason = $('#Reason').val();
        if (reason === '') {
            debugger
            $('#Reason').addClass('is-invalid')
        }
        var url = $form.attr('action');
        var formData = new FormData(this);
        $('#Reason').hide().text('');
        $.ajax({
            type: 'POST',
            url: url,
            data: formData,
            contentType: false,
            processData: false,
            dataType: 'json',
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



    $("#Reason,").on("input", function () {
        var reason = $("#Reason").val().trim();
        

        // Hide FullName error if it's being filled
        if (reason !== "") {
            $("#Reason").removeClass("is-invalid");
            $("#ReasonError").hide().text("");
        }

    });

    // Reset button click
    $('#ResetButton').on('click', function () {
        resetForm();
    });

    // Reset logic function
    function resetForm() {
     
        // Reset form
        //$('#LeaveRequestForm')[0].reset();

        choiceManager.clearChoice('EmployeeID')
        choiceManager.clearChoice('LeaveTypeID')
        $('#Reason').val('');
        // Optional: reset tabs to first tab
        $('#myTab a:first').tab('show');
    }

});
