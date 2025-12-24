
//Customer Dropdown with Select2 and AJAX
$(document).ready(function () {
    // local veriable
    let customerId = null;

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

    $("#CustomerID2").on("change", function () {
        $("#JobID").val(null).trigger("change");
        document.getElementById("JobID").disabled = false;
        customerId = $(this).val();

    });

    // Save Functionality
    $('#saveBtn').on('click', function (e) {
        e.preventDefault();

         var formData = new FormData();
        formData.append('CustomerID2', $('#CustomerID2').val());
        formData.append('JobID', $('#JobID').val());

        var requestedUsers = $("#RequestedByUserID").val(); 
        requestedUsers.forEach(function (id) {
            formData.append('RequestedByUserID', id);
        }); //Multiple Select

        //Save Multiple Employee
        var groupemp = $("#GroupEmployeeID").val();
        groupemp.forEach(function (id) {
            formData.append('GroupEmployeeID', id);
        });

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


    $('#JobID').select2({
        placeholder: 'Select Job',
        width: '100%',
        ajax: {
            url: `/createjobs/getjobs`,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    customerId: customerId, search: params.term || '',
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
                return 'Data not found. Create a <a href="#" id="createJob">job</a>';
            }
        },
        escapeMarkup: function (markup) { return markup; } // allow HTML in noResults
    });


    //Modal Job Type Dropdown

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

    let customerScriptLoaded = false

    //Modal
    $(document).on("click", "#createCustomer", function () {
        $.get('/Customers/IndexModal', function (html) {
            $('.create-lead-modal-body').html(html);
            // Load script if needed
            $.getScript('/js/pages/CRM/Customer/customer.bundle.js')
                .done(function () {

                    initCreateJobModal();
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
        debugger
        $.get('/CreateJobs/IndexModal', function (html) {
            $('.create-job-modal-body').html(html);
            // Load script if needed
            $.getScript('/js/pages/FieldServices/CreateJob.js')
                .done(() => {
                    debugger;
                    if (typeof initCreateJobModal === "function") {
                        initCreateJobModal();
                    }

                const modalEl = document.getElementById('createJobModalToggle');
                modalEl.setAttribute("data-bs-backdrop", "static");
                modalEl.setAttribute("data-bs-keyboard", "false");
                // Now open modal
                bootstrap.Modal.getOrCreateInstance(modalEl).show();

                    if (typeof LoadMainPageData === "function") {
                    LoadMainPageData(customerId);
                }
            });

           
        });
    });



    const checkbox = document.getElementById('toggleCheckbox');
   
    const hiddenDiv2 = document.getElementById('hiddenDiv2');

    checkbox.addEventListener('change', function () {
     
        hiddenDiv2.style.display = this.checked ? 'block' : 'none';
    });


    // propagate CustomerID2 value into modal after modal is shown
    $('#createJobModalToggle').on('shown.bs.modal', function () {
        const selectedCustomerId = $("#CustomerID2").val();
        $(this).find('#CustomerID2').val(selectedCustomerId).trigger('change');
    });


    window.finishModalProcess = function (value, text) {
        debugger;
        alert("I Got response");
        const modalEl = document.getElementById('createCustomerModalToggle');
        // Now open modal
        bootstrap.Modal.getOrCreateInstance(modalEl).hide();
    }

});

//Clear Function

function clearForm() {
    $('#formClear')[0].reset();
    $('#EmployeeAdvanceID').val('0');
    $('#CustomerID2').val(null).trigger('change');
    $('#JobID').val(null).trigger('change');
    $('#RequestedByUserID').val(null).trigger('change');
    $('#ApprovedByUserID').val(null).trigger('change');
    
}
