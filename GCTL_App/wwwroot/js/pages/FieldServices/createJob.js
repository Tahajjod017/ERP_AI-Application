//#region and ids
let customers = [];

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

    $('#select2').select2({
        placeholder: 'Select Customer',
        width: '100%',
        ajax: {
            url: '/CreateJobs/GetCustomers',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    search: params.term || '',
                    page: params.page || 1
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    
                    results: data.results,
                    pagination: {
                        more: data.pagination.more
                    }
                };
            },
            cache: true
        },
        width: '100%'
    });


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

    //#region initialCustomer
    async function initCutomer() {
        await getCustomerList(); // wait for AJAX to complete
    }
    initCutomer();
    //#endregion
});

//#region getCustomerList
async function getCustomerList() {
    try {
        customers = await new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/GetCustomerList',
                method: 'GET',
                success: function (data) {
                    resolve(data);
                },
                error: function (xhr) {
                    toastr.error('Failed to load Contact Name');
                    reject(xhr);
                }
            });
        });
    } catch (err) {
        console.error(err);
        customers = [];
    }
}
//#endregion