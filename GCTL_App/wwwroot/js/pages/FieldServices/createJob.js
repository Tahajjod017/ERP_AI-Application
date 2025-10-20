//#region and ids
var actions = {
    create: "/CreateJobs/Upsert",
    delete : "",
    getLists : "",
    getItemData: ""
}

var ids = {
    formID: "#CreateJob-form",
    submitBtn: "#submitBtn",
    resetBtn: "#resetBtn",
}
//#endregion


$(function () {
    //#region submit function
    $(ids.submitBtn).on("click", function (e) {
        e.preventDefault();
        try {
            const formData =
            {
                CreateJobID: $("#CreateJobID").val(),
                CustomerID: 1,
                //CustomerID: $("#CustomerID").val(),
                JobTitle: $("#JobTitle").val(),
                JobID: $("#JobID").val(),
                TeamMembers: $("#TeamMembers").val(),
                StartDate: $("#StartDate").val(),
                EndDate: $("#EndDate").val(),
                StatusID: $("#StatusID").val(),
                JobLocation: $("#JobLocation").val(),
                Note: $("#Note").val(),
                FileLink: $("#FileLink").val()
            }
            showDev(formData);
            $.ajax({
                url: actions.create,
                type: "POST",
                data: formData,
                headers: {
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
            toastr.error(ex);
        }
        
    });
    //#endregion
});