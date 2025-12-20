
//Customer Dropdown
$(document).ready(function () {

    $('#CustomerID2').select2({
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

        language: {
            noResults: function () {
                return $(
                    `<span>Data not found. Create a <a id="createCustomer" href="#">Customer</a></span>`
                );
            }
        },
        width: '100%'
    });


    // Save Functionality
    $('#saveBtn').on('click', function (e) {
        e.preventDefault();

        // Using form data
        var formData = new FormData();
        formData.append('CustomerID2', $('#CustomerID2').val());
        formData.append('JobID', $('#JobID').val());
        formData.append('RequestedByUserID', $('#RequestedByUserID').val());
        formData.append('AmountRequested', $('#AmountRequested').val());
        formData.append('StartDate', $('#StartDate').val());
        formData.append('EndDate', $('#EndDate').val());
        formData.append('ApprovedByUserID', $('#ApprovedByUserID').val());
        

        $.ajax({
            url: '/EmployeeAdvanced/Create/',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function () {
                showLoadingIndicator();
            },
            success: function (response) {
                console.log("Response:", response);
                if (response.success) {
                    toastr.success("Saved Successfully");
                    clearForm();
                } else {
                    if (response.errors) {
                        // Display validation errors
                        response.errors.forEach(function (error) {
                            toastr.error(error.message);
                        });
                    } else {
                        toastr.error(response.Message || "Validation failed");
                    }
                }
                // Handle success (e.g., show a success message, redirect, etc.)
            },
            error: function (xhr, status, error) {
                // Handle error (e.g., show an error message)
                toastr.error("Error during Save");
            },
            complete: function () {
                hideLoadingIndicator();
            }
        });
    });

//// After successful job creation (in your modal's save handler)
//$('#JobID').val(null).trigger('change'); // Clear current selection
//$('#JobID').select2('destroy'); // Destroy the current instance

// Job Dropdown Reinitialize Select2
$('#JobID').select2({
    placeholder: 'Select Job',
    width: '100%',
    ajax: {
        url: '/CreateJobs/GetJobs',
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
    language: {
        noResults: function () {
            return $(
                `<span>Data not found. Create a <a id="createJob" href="#">Job</a></span>`
            );
        }
    },
    width: '100%'
});


    //Modal Job Type

    $('#RequestedByUserID').select2({
        placeholder: 'Select Job',
        width: '100%',
        ajax: {
            url: '/EmployeeAdvanced/GetJobsType',
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
        language: {
            noResults: function () {
                return $(
                    `<span>Data not found. Create a <a id="createJob" href="#">Job</a></span>`
                );
            }
        },
        width: '100%'
    });


    //Modal
    $(document).on("click", "#createCustomer", function () {
        $.get('/Customers/IndexModal', function (html) {
            $('.create-lead-modal-body').html(html);
            // Load script if needed
            $.getScript('/js/pages/CRM/Customer/customer.bundle.js')
                .done(() => {
                    if (typeof initCreateLeadModal === "function") {
                        initCreateLeadModal();
                    }
                });

            const modalEl = document.getElementById('createCustomerModalToggle');
            modalEl.setAttribute("data-bs-backdrop", "static");
            modalEl.setAttribute("data-bs-keyboard", "false");
            // Now open modal
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });
    });


    //Modal
    $(document).on("click", "#createJob", function () {
        $.get('/CreateJobs/IndexModal', function (html) {
            $('.create-job-modal-body').html(html);
            // Load script if needed
            $.getScript('/js/pages/FieldServices/CreateJob.js');
                //.done(() => {
                //    if (typeof initCreate === "function") {
                //        initCreate();
                //    }
                //});

            const modalEl = document.getElementById('createJobModalToggle');
            modalEl.setAttribute("data-bs-backdrop", "static");
            modalEl.setAttribute("data-bs-keyboard", "false");
            // Now open modal
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });
    });



    const checkbox = document.getElementById('toggleCheckbox');
   
    const hiddenDiv2 = document.getElementById('hiddenDiv2');

    checkbox.addEventListener('change', function () {
     
        hiddenDiv2.style.display = this.checked ? 'block' : 'none';
    });


    //Nested Dropdown for Job By Customer

    $("#JobID").on('change', function () {
        const jobId = $(this).val();

        if (jobId) {
            $.ajax({
                url: '/CreateJobs/GetCustomerInfo', // set this in Razor
                type: "GET",
                data: { id: jobId },
                success: function (customer) {
                    $("#CustomerID").empty();

                    if (customer) {
                        $("#CustomerID").append(
                            `<option value="${customer.id}" selected>
                            ${customer.text}
                         </option>`
                        );
                    } else {
                        $("#CustomerID").append(
                            `<option value="">No Customer Found</option>`
                        );
                    }
                }
            });
        } else {
            $("#CustomerID").empty().append(`<option value="">Select Client</option>`);
        }
    });

    //Nested System Job by JobType

    $("#RequestedByUserID").on('change', function () {
        const jobTypeIds = $(this).val(); //multiple select

        $("#JobID").empty().append(`<option value="">Select JobType</option>`);

        if (jobTypeIds && jobTypeIds.length > 0) {
            $.ajax({
                url: "/CreateJobs/GetJobsByJobType",
                type: "GET",
                traditional: true, // important for array parameters
                data: { jobTypeIds: jobTypeIds },
                success: function (jobs) {
                    $("#JobID").empty().append(`<option value="">Slect Job</option>`);

                    if (jobs && jobs.length > 0) {
                        $.each(jobs, function (i, job) {
                            $("#JobID").append(
                                `<option value="${job.id}">${job.text}</option>`
                            );
                        });
                    } else {
                        $("#JobID").append(`<option value="">No Jobs Found</option>`);
                    }
                }

            });
        } else {
            $("#JobID").empty().append(`<option value="">Select Job</option>`)
        }
    })

});

//Clear Function

function clearForm() {
    $('#formClear')[0].reset();
    $('#EmployeeAdvanceID').val('0');
    $('#saveBtn').text('Reset');
}
