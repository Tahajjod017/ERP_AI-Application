$(document).ready(function () {


    $(document).on('click', '#ApplyLeaveSubmitButtonApproval', function (e) {
        e.preventDefault();

        //if (!validateLoanForm()) {
        //    return;
        //}
        const approvalStatus = $('input[name="ApprovalStatus"]:checked').val();
        const isApproved = approvalStatus === "true";

        const formdata = {
             LoanID: parseInt ($('#LoanID').val()),
             IssueDate: flatpickrHelper.getDate('IssueDate'),
            StartDate: flatpickrHelper.getDate('StartDate'),
            EmployeeIDs: parseInt(choiceManager.getChoiceValue('EmployeeIDs')),
            LoanInstallmentPeriodID: parseInt(choiceManager.getChoiceValue('LoanInstallmentPeriodID')),
             Approved: isApproved,
             Declined: !isApproved,
             ApproverNote: $('#ApproverNote').val(),
             LoanAmount: $('#LoanAmount').val()
        };
        console.log(formdata);
        $.ajax({
            url: '/PayRollLoanView/UpdateFromAppDecAsync',
            type: 'POST',
            data: JSON.stringify(formdata),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    resetForm();
                    var applyModalEl = document.getElementById('ApprovedDeclineModal');
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


    function resetForm() {
        var $form = $('#loanForm');

        // Reset native form fields
        $form[0].reset();
        choiceManager.resetChoice('LoanInstallmentPeriodID')
        choiceManager.resetChoice('EmployeeIDs')
        // Reset multi-selects (CoreUI)
       

        // Clear flatpickr datepickers
        $form.find('.datetimepicker').each(function () {
            if (this._flatpickr) {
                this._flatpickr.clear();
            } else {
                $(this).val('');
            }
        });
    }


    

    


    $(document).on('click', '.edit-loan', function () {
        const loanId = $(this).data('id');
        $.ajax({
            url: '/PayRollLoanView/GetByIdApprovedOrDecline', 
            type: 'GET',
            dataType:'json',
            data: { id: loanId },
            success: function (response) {
               
                if (response.success) {
                    const data = response.data;

                    // Fill form fields
                    $('#loanForm').find('[name="LoanID"]').val(data.loanID);
                    choiceManager.setChoiceValue('EmployeeIDs', data.employeeIDs);
                    $('#loanForm').find('[name="LoanAmount"]').val(data.loanAmount);
                    choiceManager.setChoiceValue('LoanInstallmentPeriodID', data.loanInstallmentPeriodID);
                    flatpickrHelper.setDate('IssueDate', data.issueDate);
                    

                    const $effectiveDateInput = $(`#StartDate`);
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


    initializeDatepickerDMY("IssueDate");
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

    $(document).on('change', 'input[name="ApprovalStatus"]', function () {
        const isApproved = $(this).val() === 'true';
        const $button = $('#ApplyLeaveSubmitButtonApproval');

        if (isApproved) {
            $button
                .removeClass('d-none btn-danger')
                .addClass('btn-primary')
                .text('APPROVE');
        } else {
            $button
                .removeClass('d-none btn-primary')
                .addClass('btn-danger')
                .text('DECLINE');
        }
    });
    $(document).ready(function () {
        $('#ApplyLeaveSubmitButtonApproval').addClass('d-none');
    });
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

    $('#loanEntryWaiting-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableDataWaiting();
        }
    });

    $(document).ready(function () {
        loadTableDataWaiting();

        $("#loanEntryWaiting-searchInput").on("input", function () {
            currentPage = 1;
            loadTableDataWaiting();
        });

        $("#loanEntryWaiting-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableDataWaiting();
            }
        });

        $("#loanEntryWaiting-nextPageBtn").on('click', function () {
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
        var searchTerm = $("#loanEntryWaiting-searchInput").val();
        const organizationId = $('#OrganizationID').val();
        const departmentIds = $('#DepartmentIDs option:selected').map(function () { return $(this).val(); }).get();
        const employeeIds = $('#EmployeeIDs option:selected') .map(function () { return $(this).val(); }).get();

      
        $.ajax({
            url: '/PayRollLoanView/GetAllTableAboveAsync',
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
                      <td class="align-middle white-space-nowrap text-end pe-2 ps-1">
                          <div class="d-flex  align-items-center">
                              <a href="#"
                                 title="View"
                                
                                 data-id="${item.loanID}"
                                 class="edit-loan btn btn-outline-light btn-icon d-flex align-items-center justify-content-center"
                                 data-bs-toggle="modal"
                                 data-bs-target="#ApprovedDeclineModal">
                                  <i class="fas fa-eye text-primary"></i>
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

                $("#loanEntryWaiting-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#loanEntryWaiting-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#loanEntryWaiting-paginationLinks");
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
        $("#loanEntryWaiting-prevPageBtn").prop('disabled', currentPage === 1);
        $("#loanEntryWaiting-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableDataWaiting();
    });
    });
    //#endregion



    // #region  Data below Table

    $(document).ready(function () {
        var currentPage = 1;
        var pageSize = 5;

        $('#loanEntryBelow-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableDatabelow();
            }
        });

        $(document).ready(function () {
            loadTableDatabelow();

            $("#loanEntryBelow-searchInput").on("input", function () {
                currentPage = 1;
                loadTableDatabelow();
            });

            $("#loanEntryBelow-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableDatabelow();
                }
            });

            $("#loanEntryBelow-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableDatabelow();
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

            loadTableDatabelow(currentSortColumn, currentSortOrder);
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
            loadTableDatabelow(currentSortColumn, currentSortOrder); // pass sorting info
        });


        function getBadgeClass(status) {
            if (!status || status.trim() === '') return 'text-bg-success';

            switch (status.trim().toUpperCase()) {
                case 'DECLINED':
                    return 'badge-phoenix badge-phoenix-danger';
                case 'APPROVED':
                    return 'badge-phoenix badge-phoenix-success';
                case 'PENDING':
                    return 'badge-phoenix-warning';
                default:
                    return 'text-bg-success';
            }
        }


        function loadTableDatabelow(currentSortColumn, currentSortOrder) {
            var searchTerm = $("#loanEntryBelow-searchInput").val();
            const organizationId = $('#OrganizationID').val();
            const departmentIds = $('#DepartmentIDs option:selected').map(function () { return $(this).val(); }).get();
            const employeeIds = $('#EmployeeIDs option:selected').map(function () { return $(this).val(); }).get();


            $.ajax({
                url: '/PayRollLoanView/GetAllTableBelowAsync',
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
                    var tableBody = $("#LoanViwBelow-tBody");
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
                   <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.monthlyEMITT || 0}</td>
                     <td class="dptStatus align-middle white-space-nowrap ps-5 fw-semibold text-body py-0">
                          <span class="badge ${getBadgeClass(item.statusName)}">${item.statusName || 'NEW'}</span>
                     </td>
              </tr>
                `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#loanEntryBelow-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#loanEntryBelow-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#loanEntryBelow-paginationLinks");
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
            $("#loanEntryBelow-prevPageBtn").prop('disabled', currentPage === 1);
            $("#loanEntryBelow-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableDatabelow();
        });
    });
    //#endregion


})