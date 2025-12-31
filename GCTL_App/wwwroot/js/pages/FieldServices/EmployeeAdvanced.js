

//Customer Dropdown with Select2 and AJAX
$(document).ready(function () {
    // local veriable
    let customerId = null;

    //#region Customer 
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
    //#endregion

    //#region Job

    
    const initializeSelect1 = () => {
        $('.initSelectEmpAD').select2({
            width: '100%',
            allowClear: true,
            placeholder: 'Select an option',
            language: { noResults: () => 'No results found' },
            escapeMarkup: markup => markup
        });
    };

    initializeSelect1();

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
    //#endregion

    //#region Save 
    $('#saveBtn').on('click', function (e) {
        e.preventDefault();

        var customerValue = $('#CustomerID2').val();

        // Continue with form submission
        var formData = new FormData();
        formData.append('CustomerID2', customerValue);

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
        formData.append('EmployeeAdvanceID', $('#EmployeeAdvanceID').val());
        formData.forEach((value, key) => {
            console.log(`${key}: ${value}`);
        });
        //Update

        var id = $('#EmployeeAdvanceID').val();
        var url = id > 0 ? '/EmployeeAdvanced/Update' : '/EmployeeAdvanced/Create/';
        var type = 'POST';

        $.ajax({
            url: url,
            type: type,
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function () {
                showLoadingIndicator();
            },
            success: function (response) {
                const allFields = ["CustomerID2", "JobID", "RequestedByUserID", "AmountRequested", "ApprovedByUserID"];
                allFields.forEach(fieldId => validateField(fieldId, response));

                if (response.isSuccess) {
                    toastr.success(response.message);
                    clearForm();
                    loadTableData();
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error("Error during Save");
            },
            complete: function () {
                hideLoadingIndicator();
            }
        });
    });

    //#endregion

    //#region Edit
    function setSelect2Value(selector, id, text) {
        if (!id) return;
        const option = new Option(text, id, true, true);
        $(selector).append(option).trigger('change');
    }

    
 
    $(document).on('click', '.employeeAdvance-editBtn', async function (e) {
        e.preventDefault();

        const id = $(this).data('id');
        if (!id) {
            toastr.error('Invalid record ID');
            return;
        }

        try {
            const response = await $.get('/EmployeeAdvanced/GetById', { id });

            if (!response.isSuccess) {
                toastr.warning(response.message);
                return;
            }

            const data = response.data;

            /* ---------- CUSTOMER (Select2 AJAX) ---------- */
            setSelect2Value(
                '#CustomerID2',
                data.customerID2,
                data.customerName
            );

            /* ---------- JOB (Dependent Select2 AJAX) ---------- */
            customerId = data.customerID2;
            document.getElementById("JobID").disabled = false;

            setSelect2Value(
                '#JobID',
                data.jobID,
                data.jobName
            );
            $('#AmountRequested').val(data.amountRequested);
            $('#StartDate').val(data.startDate ? data.startDate.split('T')[0] : '');
            $('#EndDate').val(data.endDate ? data.endDate.split('T')[0] : '');
            $('#EmployeeAdvanceID').val(data.employeeAdvanceID);


            $('#GroupEmployeeID')
                .val(data.groupEmployeeID)   // must be array
                .trigger('change');

            $('#RequestedByUserID')
                .val(data.requestedByUserID)
                .trigger('change'); //must be array

            // Set selected value if data.approvedByUserID exists
            if (data.approvedByUserID) {
                $('#ApprovedByUserID').val(data.approvedByUserID.
                    toString()).trigger('change');
            }

            $('#saveBtn').text('Update');
        }
        catch (err) {
            console.error(err);
            toastr.error('Failed to load data');
        }
    });

    // #endregion

    //#region Update
    function UpdateEmployeeAdvance(employeeAdvanceId) {
        const formData = {
            EmployeeAdvanceID: employeeAdvanceId,
            JobID: $('#JobID').val(),
            AmountRequested: $('#AmountRequested').val(),
            StartDate: $('#StartDate').val(),
            EndDate: $('#EndDate').val(),
            RequestedByUserID: parseInt($('#RequestedByUserID').val()),
            ApprovedByUserID: $('#ApprovedByUserID').val(),
            GroupEmployeeID: $('#GroupEmployeeID').val()?.split(',').map(Number) || [],
            LIP: $('#LIP').val(),
            LMAC: $('#LMAC').val(),
            UpdatedBy: $('#UpdatedBy').val() || currentUserId // Replace with your actual user ID source
        };

        $.ajax({
            url: '/EmployeeAdvanced/Update', // Replace 'YourController' with your actual controller name
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.isSuccess) {
                    toastr.success(response.message || 'Employee Advance Updated Successfully');
                    loadTableData();
                    clearForm();
                    
                    // Optionally reload table or redirect
                    // $('#yourTableId').DataTable().ajax.reload();
                    // window.location.href = '/YourController/Index';
                } else {
                    toastr.error(response.message || 'Update failed');
                    if (response.field) {
                        $(`#${response.field}`).addClass('is-invalid');
                    }
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while updating: ' + error);
            }
        });
    }

    // Example: Bind to update button click
    $(document).on('click', '#btnUpdate', function () {
        const employeeAdvanceId = $('#EmployeeAdvanceID').val();
        UpdateEmployeeAdvance(employeeAdvanceId);
    });
    //#endregion

    // #region Delete
    // Bulk Delete Button Click
    $("#empAdvanced-bulkDelBtn").on('click', function () {
        var selectedItems = $(".addEmpCheck-selectedItem:checked");
        var selectedIds = [];
        selectedItems.each(function () {
            selectedIds.push($(this).data('id'));
        });
        if (selectedIds.length > 0) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/EmployeeAdvanced/Delete',
                    method: 'DELETE',
                    data: { ids:selectedIds },
                    success: function (response) {
                        if (response.isSuccess) {
                            toastr.success(response.message);
                            $("#empAdvanced-check-all").prop('checked', false);
                            $('.addEmpCheck-selectedItem').prop('checked', false);
                            $('#empAdvanced-bulkSelectActions').addClass('d-none');
                            clearForm();
                           loadTableData();
                        } else {
                            toastr.error(response.message);
                        }
                    },
                    error: function () {
                        toastr.error("Error occurred while deleting.");
                    }
                });
            });
        } else {
            toastr.info("Please select at least one item to delete.");
        }
    });

    // Single Delete Button Click
    $(document).on('click', '.advance-delete', function () {
        var id = $(this).data('id');
        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/EmployeeAdvanced/Delete',
                    method: 'DELETE',
                    data: { ids: [id] },
                    success: function (response) {
                        if (response.isSuccess) {
                            toastr.success(response.message);
                            $("#empAdvanced-check-all").prop('checked', false);
                            clearForm();
                            loadTableData();
                        } else {
                            toastr.error(response.message);
                        }
                    },
                    error: function () {
                        toastr.error("Error occurred while deleting.");
                    }
                });
            });
        } else {
            toastr.error("Invalid action.");
        }
    });
    // #endregion

    //#region Modal Job Type Dropdown

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
    //#endregion

    //#region Modal customer Dropdown
    let customerScriptLoaded = false

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
    //#endregion

    //#region Modal job
    $(document).on("click", "#createJob", function () {
        $.get('/CreateJobs/IndexModal', function (html) {
            $('.create-job-modal-body').html(html);
            // Load script if needed
            $.getScript('/js/pages/FieldServices/CreateJob.js')
                .done(() => {
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
    //#endregion

    // #region Clear Function

    function clearForm() {
        $('#formClear')[0].reset();
        $('#EmployeeAdvanceID').val('0');
        $('#CustomerID2').val(null).trigger('change');
        $('#JobID').val(null).trigger('change');
        $('#RequestedByUserID').val(null).trigger('change');
        $('#ApprovedByUserID').val(null).trigger('change');
        $('#saveBtn').text('Send For Approval');
        resetValidation(["CustomerID2", "JobID", "RequestedByUserID", "AmountRequested", "ApprovedByUserID"]);

    }



    // propagate CustomerID2 value into modal after modal is shown
    $('#createJobModalToggle').on('shown.bs.modal', function () {
        const selectedCustomerId = $("#CustomerID2").val();
        $(this).find('#CustomerID2').val(selectedCustomerId).trigger('change');
    });
    //#endregion

    //#region Checkbox
    $(document).ready(function () {
        $('#empAdvanced-check-all').on('change', function () {
            var isChecked = $(this).prop('checked');
            $('.addEmpCheck-selectedItem').prop('checked', isChecked);

           
        });
        $(document).on('change', '.addEmpCheck-selectedItem',
            function () {
                toggleBulkActions();
            });
    });
    //#endregion

    //#region Bulk Action
    function toggleBulkActions() {
        var checkedCount = $('.addEmpCheck-selectedItem:checked').length;
        if (checkedCount > 0) {
            $('#empAdvanced-bulkSelectActions').removeClass('d-none');
        } else {
            $('#empAdvanced-bulkSelectActions').addClass('d-none');
            $('#empAdvanced-check-all').prop('checked', false);
        }
    }
    //#endregion

    // #region loadTableData
    var currentPage = 1;
    var pageSize = 5;

    $('#empAdvanced-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });


    $(document).ready(function () {
        loadTableData();

        $("#empAdvanced-searchInput").on("input", function () {
            currentPage = 1;
            loadTableData();
        });

        $("#empAdvanced-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        });

        $("#empAdvanced-nextPageBtn").on('click', function () {
            currentPage++;
            loadTableData();
        });
    });


    let currentSortColumn = 'EmployeeAdvanceID';
    let currentSortOrder = 'desc';

    $('th.sort').on('click', function () {
        const column = $(this).data('sort');

        if (currentSortColumn === column) {
            currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = column;
            currentSortOrder = 'asc';
        }

        loadTableData(currentSortColumn, currentSortOrder);
        updateSortingIndicator(column, currentSortOrder);
    });


    function updateSortingIndicator() {
        $('th.sort').each(function () {
            const $th = $(this);
            const column = $th.data('sort');
            $th.find('.sort-icon').remove();

            if (column === currentSortColumn) {
                const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
                $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
            } else {
                $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
            }
        });
    }

    function loadTableData(sortColumn, sortOrder) {
        var searchTerm = $("#empAdvanced-searchInput").val();

        $.ajax({
            url: '/EmployeeAdvanced/GetAllAsync',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                console.log(response);
                var tableBody = $("#empAdvanced-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td>
                                <input type="checkbox" class="form-check-input addEmpCheck-selectedItem" data-id="${item.employeeAdvanceID}" />
                            </td>
                            <td class="empId align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">${item.customerID2}
                            </td>
                            <td class="empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">${item.customerName}
                                
                            </td>
                            <td class="empDept align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTitle || 'N/A'}</td>
                            <td class="empDept align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTypeName || 'N/A'}</td>
                            <td class="empSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.amountRequested || 0}</td>
                            <td class="empBonus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.groupEmployeeName}
                               
                            </td>
                            <td class="empDeduction align-middle white-space-nowrap ps-4 fw-semibold text-body py-1"> ${item.statusName}
                                
                            </td>
                            <td class="netSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.requestedByUser || 0}</td>
                            <td class="paySlip align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.startDate} </td>
                            <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                                <div class="d-flex btn-reveal-trigger position-static">
                                    <a href="#!"
                                       class="btn btn-phoenix-danger btn-icon me-1 fs-10 text-body px-0 employeeAdvance-editBtn"
                                      
                                       data-id="${item.employeeAdvanceID}"
                                       title="Edit">
                                        <i class="fas fa-edit text-black"></i>
                                    </a>

                                    <a href="#!" class="btn btn-phoenix-danger btn-icon me-1 fs-10 text-body px-0 advance-delete" data-id="${item.employeeAdvanceID}"title="Delete">
                                        <i class="fa-regular fa-trash-can text-black"></i>
                                    </a>
                                </div>
                            </td>
                        </tr>
                    `);

                        //data - bs - toggle="modal"
                        //data - bs - target="#edit_employee_salary"

                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#empAdvanced-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#empAdvanced-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#empAdvanced-paginationLinks");
        paginationLinks.empty();
        // Window size (number of pages before/after the current page)
        const windowSize = 1;
        const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;
        // Helper function for ellipsis
        const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
        // Add "First Page" and ellipsis if needed
        if (currentPage > windowSize + 1) {
            paginationLinks.append(createPageButton(1), addEllipsis());
        }
        // Add page number buttons within the window range
        const startPage = Math.max(1, currentPage - windowSize);
        const endPage = Math.min(totalPages, currentPage + windowSize);
        for (let i = startPage; i <= endPage; i++) {
            paginationLinks.append(createPageButton(i));
        }
        // Add ellipsis and "Last Page" button if needed
        if (currentPage < totalPages - windowSize) {
            paginationLinks.append(addEllipsis(), createPageButton(totalPages));
        }
        // Disable or enable previous/next buttons
        $("#empAdvanced-prevPageBtn").prop('disabled', currentPage === 1);
        $("#empAdvanced-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    // #endregion





    window.finishModalProcess = function (value, text) {
        alert("I Got response");
        const modalEl = document.getElementById('createCustomerModalToggle');
        // Now open modal
        bootstrap.Modal.getOrCreateInstance(modalEl).hide();
    }

});

