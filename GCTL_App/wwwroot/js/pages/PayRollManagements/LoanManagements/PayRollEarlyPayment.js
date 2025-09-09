$(document).ready(function () {


    

    $(document).on('click', '#ResetButton', function (e) {
        e.preventDefault();
        resetEarlyPaymentForm();
    });

    function resetEarlyPaymentForm() {
        $('#LoanID').val(0);
        $('#LoanAmount').val('');
        $('#TenureMonth').val('');
        $('#MonthlyEMI').val('');
        $('#EarlyPayAmount').val('');
        choiceManager.resetChoice('OrganizationID');
        const departmentSelect = document.getElementById('DepartmentIDs');
        if (departmentSelect && coreui.MultiSelect.getInstance(departmentSelect)) {
            coreui.MultiSelect.getInstance(departmentSelect).deselectAll();
        }
        const employeeSelect = document.getElementById('EmployeeIDs');
        if (employeeSelect && coreui.MultiSelect.getInstance(employeeSelect)) {
            coreui.MultiSelect.getInstance(employeeSelect).deselectAll();
        }
        const paymentDateInput = document.querySelector('input[name="PaymentDateTime"]');
        if (paymentDateInput && paymentDateInput._flatpickr) {
            paymentDateInput._flatpickr.clear(); 
        }
    }

    $('#EarlyPayAmount, #PaymentDateTime').on('input change', function () {
        $(this).removeClass('is-invalid');
    });


    $('#SaveButton').on('click', function (e) {
        e.preventDefault();
        $('#EarlyPayAmount').removeClass('is-invalid');
        $('#PaymentDateTime, #PaymentDateTime + .flatpickr-input').removeClass('is-invalid');

        var earlyPayAmount = $('#EarlyPayAmount').val().trim();
        if (!earlyPayAmount || isNaN(earlyPayAmount) || parseFloat(earlyPayAmount) <= 0) {
            toastr.error('Please enter a valid Early Pay Amount.');
            $('#EarlyPayAmount').addClass('is-invalid').focus();
            return;
        }

        var paymentDate = $('#PaymentDateTime').val().trim();
        if (!paymentDate) {
            toastr.error('Please select a Payment Date.');
            $('#PaymentDateTime + .flatpickr-input').addClass('is-invalid');
            $('#PaymentDateTime').addClass('is-invalid');
            const flatpickrInstance = flatpickr('#PaymentDateTime');
            flatpickrInstance.open();
            return;
        }
        if (paymentDate.includes('/')) {
            var parts = paymentDate.split('/');
            paymentDate = `${parts[2]}-${parts[1]}-${parts[0]}`;
        }

        var payload = {
            LoanID: parseInt($('#LoanID').val()) || 0,
            PaymentDateTime: paymentDate,
            EarlyPayAmount: parseFloat($('#EarlyPayAmount').val()) || null,
            TenureMonth: $('#TenureMonth').val() || null
        };

        $.ajax({
            url: '/PayRollEarlyPayment/SaveAsynce',
            type: 'POST',
            data: payload,
            success: function (response) {
                debugger
                if (response.success)
                {
                    resetEarlyPaymentForm();
                    var applyModalEl = document.getElementById('add_employee_loan_early_payment');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal) {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                    toastr.success(response.message || 'Saved successfully!');
                } else
                {
                    toastr.error(response.message || 'Failed to save.');
                }
            }
        });
    });


    //
    function populateEarlyPaymentForm(data) {
        $('#LoanDetailsID').val(data.loanDetailsID);
        $('#LoanID').val(data.loanID);
        choiceManager.setChoiceValue('EmployeeEdit', data.employeeIDs);
        $('#LoanAmountEdit').val(data.loanAmount);
        $('#TenureMonthEdit').val(data.tenureMonth);
        $('#MonthlyEMIEdit').val(data.monthlyEMI);
        $('#EarlyPayAmountEdit').val(data.earlyPayAmount);
        if (data.paymentDateTime) {
            const fp = document.querySelector('#PaymentDateTimeEdit')._flatpickr;
            if (fp) {
                fp.setDate(data.paymentDateTime, true, "Y-m-d"); // value in backend format
            }
        }
    }

    // On click of edit button
    $(document).on('click', '#edit-loanDetails', function () {
        const loanDetailsId = $(this).data('id');

        $.ajax({
            url: '/PayRollEarlyPayment/GetLaonDetailsAsync',
            type: 'GET',
            data: { id: loanDetailsId },
            success: function (response) {
                if (response.success) {
                    populateEarlyPaymentForm(response.data);
                } else {
                    alert(response.message);
                }
            }
        });
    });

    $('#SaveEdit').on('click', function (e) {
        e.preventDefault();

        // Collect form data
        var model = {
            LoanDetailsID: parseInt($('#LoanDetailsID').val()),
            LoanID: parseInt($('#LoanID').val()),
            EmployeeIDs: parseInt($('#EmployeeEdit').val()) || null,
            LoanAmount: parseFloat($('#LoanAmountEdit').val()) || 0,
            EarlyPayAmount: parseFloat($('#EarlyPayAmountEdit').val()) || 0,
            PaymentDateTime: $('#PaymentDateTimeEdit').val() ? $('#PaymentDateTimeEdit').val() : null
        };
        if (!model.EmployeeIDs) {
            alert("Please select an employee.");
            return;
        }
        if (!model.EarlyPayAmount || model.EarlyPayAmount <= 0) {
            alert("Please enter a valid Early Pay Amount.");
            return;
        }
        if (!model.PaymentDateTime) {
            alert("Please select a payment date.");
            return;
        }

        // AJAX call to update
        $.ajax({
            url: '/PayRollEarlyPayment/UpdatePayRollEarlyPaymentAsync',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message); // optional: use toastr for notification
                    UpdateReset();
                    var applyModalEl = document.getElementById('edit_employee_loan_early_payment');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal) {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                } else {
                    toastr.error(response.message || "Update failed.");
                }
            },
            error: function (xhr, status, error) {
                toastr.error("An error occurred: " + error);
            }
        });
    });

    $(document).on('click', '#CancelEdit', function (e) {
        e.preventDefault();
        UpdateReset();
    });
    // Handle Cancel button
    function UpdateReset() {
        // Optional: close modal or reset form
        $('#LoanDetailsID').val('');
        $('#LoanID').val('');
        $('#EmployeeEdit').val('').trigger('change');
        $('#LoanAmountEdit').val('');
        $('#TenureMonthEdit').val('');
        $('#MonthlyEMIEdit').val('');
        $('#EarlyPayAmountEdit').val('');
        $('#PaymentDateTimeEdit').val('');
    };
    $(document).on('changed.coreui.multi-select', '#EmployeeIDs', function () {
        const selected = $(this).val();
        if (selected.length === 0) {
            $('#LoanID').val(0);
            $('#LoanAmount').val('');
            $('#TenureMonth').val('');
            $('#MonthlyEMI').val('');
            $('#EarlyPayAmount').val('');
            return;
        }
        if (!selected) {
            toastr.warning('Please select an employee.');
            return;
        }

        const empID = selected[selected.length - 1]; 

        $.ajax({
            url: '/PayRollEarlyPayment/GetPayRollEarlyPaymentAsync',
            type: 'GET',
            dataType: 'json',
            data: { id: empID }, 
            success: function (response) {
                if (response.success) {
                    const data = response.data;
                    $('#LoanID').val(data.loanID ?? 0)
                    $('#LoanAmount').val(data.loanAmount ?? '');
                    $('#TenureMonth').val(data.tenureMonth ?? '');
                    $('#MonthlyEMI').val(data.monthlyEMI ?? '');
                    console.log('Loan Data:', data);
                
                } else {
                    toastr.error(response.message || 'Failed to load loan data.');
                }
            },
            error: function (err) {
                console.error(err);
                toastr.error('An error occurred while fetching loan data.');
            }
        });
    });


    


    initializeDatepickerDMY("PaymentDateTime,PaymentDateTimeEdit");
    initializeMonthYearPicker(".StartDateMOnthYear");
    function calculateLoanSummary() {
        let loanAmount = parseFloat($("#LoanAmount").val()) || 0;
        let months = parseInt($("#LoanInstallmentPeriodID").val()) || 0;
        let startDate = $("#StartDate").val();

        if (loanAmount > 0 && months > 0 && startDate) {
            let perMonth = (loanAmount / months).toFixed(2);
            let date = new Date(startDate);
            if (!isNaN(date)) {
                date.setMonth(date.getMonth() + months - 1); // End date (inclusive)
                let endMonth = date.toLocaleString('default', { month: 'long' });
                let endYear = date.getFullYear();
                $("#loanSummary").text(
                    `Per Month Payment:${perMonth} tk/month. 
                    It will end in ${endMonth} ${endYear}.`
                );
            }
        } else {
            $("#loanSummary").text("");
        }
    }

    $(document).on("change keyup", "#LoanAmount, #LoanInstallmentPeriodID, #StartDate", calculateLoanSummary);



    // #region OrganizationID on change
    $('#OrganizationID').on('change', function (e) {
        e.preventDefault();

        var organizationId = $(this).val();
        loadDepartmentsByOrganization(organizationId);
        getEmployeesByOrgBraDepId(organizationId, null);
    });
    // #endregion


    // #region DepartmentIDs on change
    document.getElementById('DepartmentIDs').addEventListener('changed.coreui.multi-select', function (event) {
        const orgId = $('#OrganizationID').val();

        const selected = event.value || []; // array of {text, value}
        const depIds = selected.map(x => parseInt(x.value));

        getEmployeesByOrgBraDepId(orgId, depIds);
    });
    // #endregion


    // #region loadDepartmentsByOrganization
    function loadDepartmentsByOrganization(organizationId) {
        $.ajax({
            url: '/PayRollEarlyPayment/GetDepartmentByOrganization',
            type: 'GET',
            data: { id: organizationId },
            success: function (departments) {
                var select = $('#DepartmentIDs');
                select.empty();

                const grouped = {};

                departments.forEach(dep => {
                    const group = dep.groupName || 'No Group';
                    if (!grouped[group]) {
                        grouped[group] = [];
                    }
                    grouped[group].push(dep);
                });

                Object.keys(grouped).forEach(group => {
                    const optgroup = $('<optgroup>').attr('label', group);
                    grouped[group].forEach(dep => {
                        optgroup.append(
                            $('<option>').val(dep.id).text(dep.name)
                        );
                    });
                    select.append(optgroup);
                });

                // Get the CoreUI multiselect instance
                const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                if (multiSelectInstance) {
                    multiSelectInstance.update(); // Refresh the UI
                } else {
                    // Reinitialize if not already initialized (in case it's dynamically added)
                    new coreui.MultiSelect(select[0]);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading departments:', error);
            }
        });
    }
    // #endregion


    // #region GetEmployeesByOrgBraDepId
    function getEmployeesByOrgBraDepId(orgId, depIds = []) {
        $.ajax({
            url: '/PayRollEarlyPayment/GetEmployeesByOrgBraDepId',
            type: 'GET',
            traditional: true,
            data: {
                orgId: orgId,
                depIds: depIds
            },
            success: function (employees) {
                const select = $('#EmployeeIDs');
                select.empty();

                const grouped = {};

                // Group employees by GroupName (DepartmentName)
                employees.forEach(emp => {
                    const group = emp.groupName || 'No Department';
                    if (!grouped[group]) {
                        grouped[group] = [];
                    }
                    grouped[group].push(emp);
                });

                // Build <optgroup> structure
                Object.keys(grouped).forEach(group => {
                    const optgroup = $('<optgroup>').attr('label', group);
                    grouped[group].forEach(emp => {
                        optgroup.append(
                            $('<option>').val(emp.id).text(emp.name)
                        );
                    });
                    select.append(optgroup);
                });

                const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                if (multiSelectInstance) {
                    multiSelectInstance.update(); // Refresh UI
                } else {
                    new coreui.MultiSelect(select[0]); // Init CoreUI multiselect
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading employees:', error);
            }
        });
    }
    // #endregion

   

    // #region GetEmployeesByOrgBraDepId
    function getEmployeesByOrgBraDepId(orgId, depId = null) {
        $.ajax({
            url: '/PayRollEarlyPayment/GetEmployeesByOrgBraDepId',
            type: 'GET',
            data: { orgId: orgId, depId: depId },
            success: function (employees) {
                const select = $('#EmployeeIDs');
                select.empty().append('<option value="">Select Employee</option>');

                const grouped = {};
                employees.forEach(emp => {
                    const group = emp.groupName || 'No Department';
                    if (!grouped[group]) grouped[group] = [];
                    grouped[group].push(emp);
                });

                Object.keys(grouped).forEach(group => {
                    const optgroup = $('<optgroup>').attr('label', group);
                    grouped[group].forEach(emp => {
                        optgroup.append($('<option>').val(emp.id).text(emp.name));
                    });
                    select.append(optgroup);
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading employees:', error);
            }
        });
    }
    // #endregion


   
    // #region 🟣 Get Employee Avatar HTML (Initial or Image)
    function getAvatarHtml(employee) {
        if (employee.employeeImage && employee.employeeImage !== '') {
            return `<img class="rounded-circle" src="${employee.employeeImage}" alt="${employee.employeeName}" />`;
        } else {
            const initial = employee.employeeName.charAt(0).toUpperCase();
            return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
        }
    }
    // #endregion


    // #region  Data above Table

    $(document).ready(function () {
        var currentPage = 1;
        var pageSize = 5;

        $('#earlyLoanPayment-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableDataWaiting();
            }
        });

        $(document).ready(function () {
            loadTableDataWaiting();

            $("#earlyLoanPayment-searchInput").on("input", function () {
                currentPage = 1;
                loadTableDataWaiting();
            });

            $("#earlyLoanPayment-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableDataWaiting();
                }
            });

            $("#earlyLoanPayment-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableDataWaiting();
            });
        });
        let currentSortColumn = '';
        let currentSortOrder = '';

        $('th.sort').on('click', function () {
            const column = $(this).data('sort');
            if (currentSortColumn === column) {
                currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
            } else {
                currentSortColumn = column;
                currentSortOrder = 'asc';
            }

            loadTableDataWaiting(currentSortColumn, currentSortOrder);
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

        $(document).on("change", "#OrganizationID,#DepartmentIDs,#EmployeeIDs", function () {
            currentPage = 1;
            loadTableDataWaiting(currentSortColumn, currentSortOrder); // pass sorting info
        });

        function loadTableDataWaiting(currentSortColumn, currentSortOrder) {
            var searchTerm = $("#earlyLoanPayment-searchInput").val();
            const organizationId = $('#OrganizationID').val();
            const departmentIds = $('#DepartmentIDs option:selected').map(function () { return $(this).val(); }).get();
            const employeeIds = $('#EmployeeIDs option:selected').map(function () { return $(this).val(); }).get();


            $.ajax({
                url: '/PayRollEarlyPayment/GetAllTableAsync',
                method: 'GET',
                traditional: true,
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    currentSortColumn: currentSortColumn,
                    currentSortOrder: currentSortOrder,
                    organizationId: organizationId,
                    departmentIds: departmentIds,
                    employeeIds: employeeIds,
                },
                success: function (response) {
                    console.log("Datassssss", response);
                    var tableBody = $("#earlyLoanPayment-tBody");
                    tableBody.empty();
                    var totalItems = response.paginationInfo.totalItems;

                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {

                            if (currentSortOrder === 'asc') {
                                rowIndex = (currentPage - 1) * pageSize + index + 1;
                            } else {
                                rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
                            }
                            const avatar = getAvatarHtml(item);
                            tableBody.append(`
                       <tr class="hover-actions-trigger btn-reveal-trigger position-static">

                      <td class="fs-9 align-middle py-0">
                          <div class="form-check mb-0 fs-8">
                              <input class="form-check-input" data-id="${item.loanDetailsID}" type="checkbox" />
                          </div>
                      </td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.employeeID}</td>
                      <td class="empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                          <div class="d-flex align-items-center position-relative">
                              <div class="avatar avatar-m me-3">
                                  ${avatar}
                              </div><a class="text-body-highlight fw-bold stretched-link" href="#!">${item.employeeName}</a>
                          </div>
                      </td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.employeeDepartment || 'HRM'}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.loanAmount || 0}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.earlyPayAmount || 0}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.paymentDateTime || ''}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.createdByName || 0}</td>
                      <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="edit-loanDetails"
                               data-id="${item.loanDetailsID}"
                               class="btn btn-outline-light btn-icon me-1" 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_employee_loan_early_payment">
                               <i class="fas fa-edit text-black"></i>
                    </a>
                            <a
                              href="#" title="Delete"  data-id="${item.loanDetailsID}"
                              class="btn btn-outline-light btn-icon d-none"  
                              id="loanEntryDelete-singleDelBtn" >
                              <i class="far fa-trash-alt text-black"></i>
                            </a>
                          </div>
                    </td>
                  
                 </tr>
                   `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#earlyLoanPayment-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#earlyLoanPayment-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#earlyLoanPayment-paginationLinks");
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
            $("#earlyLoanPayment-prevPageBtn").prop('disabled', currentPage === 1);
            $("#earlyLoanPayment-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableDataWaiting();
        });
    });
    //#endregion
})