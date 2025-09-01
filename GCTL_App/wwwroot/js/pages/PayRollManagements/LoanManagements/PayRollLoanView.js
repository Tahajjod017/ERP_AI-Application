$(document).ready(function () {


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


    // #region  Data Table for Peresonal
    var currentPage = 1;
    var pageSize = 5;

    $('#leaveRequest-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });

    $(document).ready(function () {
        loadTableData();

        $("#leaveRequest-searchInput").on("input", function () {
            currentPage = 1;
            loadTableData();
        });

        $("#leaveRequest-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        });

        $("#leaveRequest-nextPageBtn").on('click', function () {
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

   
    $('#EmployeeIDs').on('changed.coreui.multi-select', function () {
        currentPage = 1;
        loadTableData(); // Make AJAX call or reload the table
    });

   
    function loadTableData(currentSortColumn, currentSortOrder) {
        var searchTerm = $("#leaveRequest-searchInput").val();
        const organizationId = $('#OrganizationID').val();
        const departmentIds = $('#DepartmentIDs').val() || [];
        const employeeIds = $('#EmployeeIDs').val() || [];
      
        $.ajax({
            url: '/PayRollLoanView/GetAllTableListAsync',
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
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.tenureMonthtytyt || 'Early Payment'}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.tenureMonth || 'Month'}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.monthlyEMI || 0}</td>
                      <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.monthlyEMITT || 'Outstanding Balance'}</td>
                      <td class="align-middle white-space-nowrap text-end pe-2 ps-1">
                          <div class="d-flex  align-items-center">
                              <a href="#"
                                 title="View"
                                 id="LeaveRequestEditButton"
                                 data-id="${item.loanID}"
                                 class="btn btn-outline-light btn-icon d-flex align-items-center justify-content-center"
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

                $("#leaveRequest-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#leaveRequest-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#leaveRequest-paginationLinks");
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
        $("#leaveRequest-prevPageBtn").prop('disabled', currentPage === 1);
        $("#leaveRequest-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    //#endregion

})