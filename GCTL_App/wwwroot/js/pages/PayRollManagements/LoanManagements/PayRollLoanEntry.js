$(document).ready(function () {


    initializeDatepickerDMY("IssueDate,IssueDateEdit");
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

    $(document).on('click', '#SaveButton', function (e) {
        e.preventDefault();
        var formData = $('form').serialize();
        $.ajax({
            url: '/PayRollLoanEntry/SaveAsync', 
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    resetForm();
                    var applyModalEl = document.getElementById('apply_loan');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal) {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                } else {
                    toastr.error("Failed: " + response.message);
                }
                if (response.errors) {
                    response.errors.forEach(err => toastr.error(err));
                } 
            },
            error: function (xhr, status, error) {
                toastr.error("An error occurred: " + error);
            }
        });
    });
    //

    $(document).on('click', '#ButtonUpdate', function (e) {
        e.preventDefault();

        //if (!validateLoanForm()) {
        //    return;
        //}
      
        const formdata = {
            LoanIDEdit: parseInt($('#LoanIDEdit').val()),
            IssueDateEdit: flatpickrHelper.getDate('IssueDateEdit'),
            StartDateEdit: flatpickrHelper.getDate('StartDateEdit'),
            EmployeeIDEdit: parseInt(choiceManager.getChoiceValue('EmployeeIDEdit')),
            LoanInstallmentPeriodIDEdit: parseInt(choiceManager.getChoiceValue('LoanInstallmentPeriodIDEdit')),
            LoanAmountEdit: $('#LoanAmountEdit').val()
        };
        debugger
        console.log(formdata);
        $.ajax({
            url: '/PayRollLoanEntry/UpdateAsync',
            type: 'POST',
            data: JSON.stringify(formdata),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    resetForm();
                    var applyModalEl = document.getElementById('EditEntry_loan');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal) {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                } else {
                    toastr.error("Failed: " + response.message);
                }
                if (response.errors) {
                    response.errors.forEach(err => toastr.error(err));
                }
            },
            error: function (xhr, status, error) {
                toastr.error("An error occurred: " + error);
            }
        });
    });

    //
    $(document).on('click', '#edit-loan', function () {
        const loanId = $(this).data('id');
        $.ajax({
            url: '/PayRollLoanEntry/GetByAsync',
            type: 'GET',
            dataType: 'json',
            data: { id: loanId },
            success: function (response) {
                if (response.success) {
                    const data = response.data;

                    // Fill form fields
                    $('#loanFormEdit').find('[name="LoanIDEdit"]').val(data.loanID);
                    choiceManager.setChoiceValue('EmployeeIDEdit', data.employeeID);
                    $('#loanFormEdit').find('[name="LoanAmountEdit"]').val(data.loanAmount);
                    choiceManager.setChoiceValue('LoanInstallmentPeriodIDEdit', data.loanInstallmentPeriodID);
                    flatpickrHelper.setDate('IssueDateEdit', data.issueDate);


                    const $effectiveDateInput = $(`#StartDateEdit`);
                    if ($effectiveDateInput.length && data.startDate) {
                        try {
                            const date = new Date(data.startDate);
                            if (!isNaN(date.getTime())) {
                                const monthNames = ["January", "February", "March", "April", "May", "June",
                                    "July", "August", "September", "October", "November", "December"];
                                const formattedDate = `${monthNames[date.getMonth()]} ${date.getFullYear()}`;
                                $effectiveDateInput.val(formattedDate);
                                if ($effectiveDateInput[0]._flatpickr) {
                                    $effectiveDateInput[0]._flatpickr.setDate(formattedDate, true);
                                }
                            }
                        } catch (e) {
                            console.error(`Failed to set date for index ${index}:`, e.message);
                        }
                    }

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
    // Separate validation method
    function validateLoanForm() {
        let isValid = true;
        // EmployeeID validation (must select at least one)
        let $employeeSelect = $('select[asp-for="EmployeeIDs"]');
        if ($employeeSelect.length === 0) {
            console.error("EmployeeIDs select element not found!");
            toastr.error("Form configuration error: Employee select not found.");
            return false;
        }

        let employeeIDs = $employeeSelect.val();
        console.log("Selected EmployeeIDs:", employeeIDs); // Debug output
        if (!employeeIDs || employeeIDs.length === 0) {
            toastr.error("Please select at least one Employee.");
            isValid = false;
            return isValid;
        }

        // LoanAmount validation
        let loanAmount = $('input[asp-for="LoanAmount"]').val();
        if (!loanAmount) {
            toastr.error("Loan Amount is required.");
            isValid = false;
            return isValid;
        }
        if (isNaN(loanAmount))
        {
            toastr.error("Loan Amount must be a valid number.");
            isValid = false;
            return isValid;
        }
        if (parseFloat(loanAmount) <= 0)
        {
            toastr.error("Loan Amount must be greater than 0.");
            isValid = false;
            return isValid;
        }

        // Installment Period validation
        let installmentPeriod = $('select[asp-for="LoanInstallmentPeriodID"]').val();
        if (!installmentPeriod) {
            toastr.error("Please select an Installment Period.");
            isValid = false;
            return isValid;
        }

        // Issue Date validation
        let issueDate = $('input[asp-for="IssueDate"]').val();
        if (!issueDate) {
            toastr.error("Issue Date is required.");
            isValid = false;
            return isValid;
        }

        // Start Date validation
        let startDate = $('input[asp-for="StartDate"]').val();
        if (!startDate) {
            toastr.error("Installment Start Date is required.");
            isValid = false;
            return isValid;
        }

        return isValid; // All validations passed
    }

    //
    $(document).on('click', '#ResetButton', function (e) {
        e.preventDefault();
        resetForm();
    });

    // Reset Function
    function resetForm() {
        var $form = $('#loanForm');
        $form[0].reset();
        choiceManager.resetChoice('LoanInstallmentPeriodID')
        choiceManager.resetChoice('OrganizationID')
        resetCoreuiMultiSelect('.form-multi-select');
        loadTableData();
        // Clear flatpickr datepickers
        $form.find('.datetimepicker').each(function () {
            if (this._flatpickr) {
                this._flatpickr.clear();
            } else {
                $(this).val('');
            }
        });
        
    }

    function resetCoreuiMultiSelect(selector) {
        document.querySelectorAll(selector).forEach(select => {
            try {
                const instance = coreui?.MultiSelect?.getInstance(select);
                if (instance && typeof instance.clear === 'function') {
                    instance.clear();
                } else {
                    // Fallback: manually deselect all options
                    Array.from(select.options).forEach(option => option.selected = false);
                    select.dispatchEvent(new Event('change'));
                }
            } catch (err) {
                console.error('Error resetting CoreUI MultiSelect:', err);
            }
        });
    }

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
            url: '/AssignSpiralPattern/GetDepartmentByOrganization',
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
            url: '/AssignSpiralPattern/GetEmployeesByOrgBraDepId',
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




    //#region TooTip Modal
    //
    let hideTooltipTimer;
    // 1. Create the tooltip only once and append to <body>
    let $tooltip = $('<div class="custom-tooltip-box"></div>').css({
        position: 'fixed',
        top: '0px',
        left: '1258px',
        zIndex: 9999999,
        backgroundColor: 'rgb(255 247 209)',
        border: '1px solid #ccc',
        padding: '10px',
        minWidth: '250px',
        maxWidth: '400px',
        maxHeight: '300px',
        overflowY: 'auto',
        boxShadow: '0 3px 8px rgba(0,0,0,0.15)',
        display: 'none',
        fontSize: '13px',
        borderRadius: '4px'
    });
    $('body').append($tooltip);

    // 2. Show tooltip on hover
    $(document).on('mouseenter', '.custom-tooltip-container', function () {
        const $container = $(this);
        const $button = $container.find('.info-button');
        const id = $button.data('id2');
        const offset = $button.offset();

        clearTimeout(hideTooltipTimer);

        // Show loading state
        $tooltip.html('<div style="text-align: center; color: #666;">Loading...</div>').css({
            top: offset.top + 25,
            left: offset.left - 100
        }).fadeIn(200);

        $.ajax({
            url: '/PayRollLoanView/PayRollLoanStep',
            type: 'GET',
            data: { id: id },
            dataType: 'json',
            success: function (data) {
                const steps = Array.isArray(data) ? data : [data];
                let html = '';

                if (steps.length > 0) {
                    steps.forEach((item, index) => {
                        const approverStep = item.approverStep ?? '';
                        const statusName = item.statusName ?? '';
                        const author = item.approvarPerson ?? '';
                        const statusDescription = item.approvarNote ?? '';
                        const approvedOrDeclineDate = item.approvedOrDeclineDate ?? '';

                        html += `
                <div class="timeline-item" style="margin-bottom:1px>
                    <div class="timeline-item position-relative">
                        <div class="row g-md-3">
                            <div class="col-12 col-md-auto d-flex">
                                <div class="timeline-item-date order-1 order-md-0 me-md-4">
                                    <p class="fs-10 fw-semibold text-body-tertiary text-opacity-85 text-end">
                                        ${approverStep} 
                                    </p>
                                </div>

                                <div class="timeline-item-bar position-md-relative me-3 me-md-0">
                                    <div class="icon-item icon-item-sm rounded-7 shadow-none bg-primary-subtle">
                                        <span class="fa-solid far fa-file-alt text-primary-dark fs-10"></span>
                                    </div>
                                    <span class="timeline-bar border-end border-dashed"></span>
                                </div>
                            </div>
                            <div class="col">
                                <div class="timeline-item-content ps-6 ps-md-3">
                                    <h5 class="fs-9 lh-sm">${statusName}</h5>
                                    <p class="fs-9 mb-0">by <a class="fw-semibold" href="#!">${author}</a></p>
                                    <h5 class="fs-9 lh-sm">${approvedOrDeclineDate}</h5>
                                    <p class="fs-9 text-body-secondary">${statusDescription}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div> `;
                    });
                } else {
                    html = '<div class="text-muted" style="color: #999;">No approval steps found</div>';
                }

                $tooltip.html(html);
            }
            ,
            error: function () {
                $tooltip.html('<div class="text-danger" style="color: #d32f2f;">Error loading data</div>');
            }
        });
    });

    // 3. Hide tooltip on mouse leave from container
    $(document).on('mouseleave', '.custom-tooltip-container', function () {
        hideTooltipTimer = setTimeout(() => {
            $tooltip.fadeOut(200);
        }, 300);
    });

    // 4. Handle tooltip hover to prevent hiding when mouse moves to tooltip
    $tooltip.on('mouseenter', function () {
        clearTimeout(hideTooltipTimer);
    }).on('mouseleave', function () {
        hideTooltipTimer = setTimeout(() => {
            $tooltip.fadeOut(200);
        }, 300);
    });

    // 5. Optional: Hide tooltip when clicking elsewhere
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.custom-tooltip-container, .custom-tooltip-box').length) {
            clearTimeout(hideTooltipTimer);
            $tooltip.fadeOut(200);
        }
    });

    //#endregion



    // #region 🔵 Get Badge Class Based on Status
    function getBadgeClass(status) {
        if (!status || status.trim() === '') return 'text-bg-success';

        switch (status.trim().toUpperCase()) {
            case 'DECLINED':
                return 'badge-phoenix badge-phoenix-danger';
            case 'APPROVED':
                return 'badge-phoenix badge-phoenix-success';
            case 'PENDING':
            case 'WAITING FOR APPROVAL':
                return 'badge-phoenix badge-phoenix-warning';
            case 'NEW':
                return 'badge-phoenix text-bg-success';
            case 'ONGOING':
                return 'badge-phoenix badge-phoenix-primary';
            default:
                return 'text-bg-success';
        }
    }
    // #endregion

    // #region 🟡 Get Status Text Based on Approver Steps & Timing
    function getStatusText(item) {
        const rawStatus = item.statusName?.trim().toUpperCase();
        const isNewStatus = !rawStatus || rawStatus === 'NEW';
        if (item.approverStep === 1 || item.approverStep === 2) {
            return 'OnGoing';
        } else if (item.approverStep === 3) {
            return 'APPROVED';
        }
  
        if (isNewStatus && item.applicationDate) {

            const applicationDate = new Date(item.applicationDate.replace(' ', 'T'));
            //const applicationDate = new Date(item.applicationDate);
            const now = new Date();
            const hoursPassed = (now - applicationDate) / (1000 * 60 * 60);

            if (hoursPassed >= 24) {
                return 'Waiting for Approval';
            }
            return 'New';
        }
        return rawStatus || '<i class="text-success"></i> New';
    }
    // #endregion

    // #region 🟠 Check Whether to Show Info Icon
    function shouldShowInfoIcon(item) {
        const status = getStatusText(item)?.trim().toUpperCase();
        return !(status === 'NEW' || status === 'WAITING FOR APPROVAL');
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


    // #region  Data Table for Peresonal
    var currentPage = 1;
    var pageSize = 5;

    $('#loanEntry-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });

    $(document).ready(function () {
        loadTableData();

        $("#loanEntry-searchInput").on("input", function () {
            currentPage = 1;
            loadTableData();
        });

        $("#loanEntry-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        });

        $("#loanEntry-nextPageBtn").on('click', function () {
            currentPage++;
            loadTableData();
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

    $(document).on("change", "#StatusIDFilterDD,#LeaveTypeIDFilterDD", function () {

        currentPage = 1;
        loadTableData();
    });

    $('#EmployeeIDs').on('changed.coreui.multi-select', function () {
        currentPage = 1;
        loadTableData(); 
    });

   
    function loadTableData(currentSortColumn, currentSortOrder) {
        var searchTerm = $("#loanEntry-searchInput").val();
        const organizationId = $('#OrganizationID').val();
        const departmentIds = $('#DepartmentIDs').val() || [];
        const employeeIds = $('#EmployeeIDs').val() || [];
       

        $.ajax({
            url: '/PayRollLoanView/LoanEntryList',
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
                var tableBody = $("#LoanView-tBody");
                tableBody.empty();
                var totalItems = response.paginationInfo.totalItems;

                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {

                        if (currentSortOrder === 'asc') {
                            rowIndex = (currentPage - 1) * pageSize + index + 1;
                        } else {
                            rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
                        }
                        let status = item.statusName; // Assuming this is your status value
                        let isDisabled = status && (status.toUpperCase() === 'APPROVED' || status.toUpperCase() === 'DECLINED');

                        const avatar = getAvatarHtml(item);
                        tableBody.append(`
                       <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                        
                              <td class="fs-9 align-middle py-0">
                          <div class="form-check mb-0 fs-8">
                              <input class="form-check-input" data-id="${item.loanID}" type="checkbox" />
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
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.employeeEarlyPayment || 0}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.tenureMonth || 'Month'}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.monthlyEMI || 0}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.outSatndingbalance || 0}</td>
                        <td class="dptStatus align-middle white-space-nowrap ps-5 fw-semibold text-body py-0">
                          <span class="badge ${getBadgeClass(getStatusText(item))}">${getStatusText(item)} </span>
                           ${shouldShowInfoIcon(item) ? `
        <div class="custom-tooltip-container position-relative d-inline-block">
            <i class="fa-solid fa-circle-info info-button"
               data-id2="${item.loanID}"
               style="cursor: pointer; font-size: 14px; color: #007bff;"></i>
        </div>` : ''}
                        </td>
                        <td class="leaveTotalDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.applicationDate}</td>
                     <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="edit-loan"
                               data-id="${item.loanID}"
                               class="btn btn-outline-light btn-icon me-1 ${isDisabled ? 'disabled' : ''}" 
                               data-bs-toggle="modal" 
                               data-bs-target="#EditEntry_loan"
                               ${isDisabled ? 'aria-disabled="true" tabindex="-1"' : ''}>
                               <i class="fas fa-edit text-black"></i>
                    </a>
                            <a
                              href="#" title="Delete"  data-id="${item.loanID}"
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

                $("#loanEntry-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#loanEntry-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#loanEntry-paginationLinks");
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
        $("#loanEntry-prevPageBtn").prop('disabled', currentPage === 1);
        $("#loanEntry-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    //#endregion



})


