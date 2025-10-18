var actions = {
    create : "",
    update : "",
    delete : "",
    getLists : "",
    getItemData: ""
}

var ids = {
    formID : "#",
    submitBtn: "#",
    resetBtn: "#",
}

$(function () {
    (document).on("click", ids.submitBtn, function () {
        try {
            const id = $(this).data("id");
            const 
            const action = id === 0 ? actions.create : ac.update;
            const formData = {}
            $.ajax({
                url: action,
                type: "POST",
                data: formData,
                headers: {
                    // send anti-forgery token header
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message || "Something went wrong");
                    }
                },
                error: function (xhr) {
                    toastr.error("Error generating PDF");
                    console.error(xhr);
                }
            });
        } catch (ex) {
            Toast.error(ex);
        }
        
    });
});