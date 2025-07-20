// Data Table for Peresona
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

function getAvatarHtml(employee) {
    if (employee.employeeImage && employee.employeeImage !== '') {
        return `<img class="rounded-circle" src="${employee.employeeImage}" alt="${employee.employeeName}" />`;
    } else {
        const initial = employee.employeeName.charAt(0).toUpperCase();
        return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
    }
}

$(document).on("change", "#StatusIDFilterDD,#LeaveTypeIDFilterDD", function () {

    currentPage = 1;
    loadTableData();
});

$('#EmployeeIDs').on('changed.coreui.multi-select', function () {
    currentPage = 1;
    loadTableData(); // Make AJAX call or reload the table
});
// Filtering according to formdate to ToDate
initializeGlobalDateRangePicker(
    'basic-daterange',
    'basic-daterange_fromHidden',
    'basic-daterange_toHidden',
    function () {
        currentPage = 1;
        loadTableData();
    }
);


function loadTableData(currentSortColumn, currentSortOrder) {
    var searchTerm = $("#leaveRequest-searchInput").val();
    var leaveTypeID = $('#LeaveTypeIDFilterDD').val();
    var statusID = $('#StatusIDFilterDD').val();
    const organizationId = $('#OrganizationID').val();
    const departmentIds = $('#DepartmentIDs').val() || [];
    const employeeIds = $('#EmployeeIDs').val() || [];
    const fromDate = $('#basic-daterange_fromHidden').val(); // YYYY-MM-DD
    const toDate = $('#basic-daterange_toHidden').val();     // YYYY-MM-DD
    console.log("Dept: " + departmentIds + " | Emp: " + employeeIds + " | Org: " + organizationId);
    console.log("From: " + fromDate + " | To: " + toDate);

    $.ajax({
        url: '/LeaveHistoryRoute/GetAllTableHistoryAsync',
        method: 'GET',
        traditional: true,
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            currentSortColumn: currentSortColumn,
            currentSortOrder: currentSortOrder,
            leaveTypeID: leaveTypeID,
            statusID: statusID,
            organizationId: organizationId,
            departmentIds: departmentIds,
            employeeIds: employeeIds,
            fromDate: fromDate,    // 👈 added
            toDate: toDate
        },
        success: function (response) {



            console.log("Datassssss", response);
            var tableBody = $("#leaveHistory-tBody");
            tableBody.empty();
            var totalItems = response.paginationInfo.totalItems;

            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {

                    if (currentSortOrder === 'asc') {
                        rowIndex = (currentPage - 1) * pageSize + index + 1;
                    } else {
                        rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
                    }
                    //

                    //
                    let status = item.statusName; // Assuming this is your status value
                   
                    const isFullDay = item.isFullDay;
                    // pick the right label and pluralize
                    const unitLabel = isFullDay
                        ? (item.period > 1 ? 'Days' : 'Day')
                        : (item.period > 1 ? 'Hours' : 'Hour');
                    //
                    //

                    const avatar = getAvatarHtml(item);

                    //
                    tableBody.append(`
                       <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                        
                        <td class="fs-9 align-middle py-0">
                          <div class="form-check mb-0 fs-8">
                            <input class="form-check-input" data-id="${item.leaveApplicationID}" type="checkbox" />
                          </div>
                        </td>
  
                        
                        <td class="approveByEmployee align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                          <div class="d-flex align-items-center file-name-icon">
                            <div class="avatar avatar-m avatar-bordered me-2">
                             ${avatar}
                            </div>
                            <div class="ms-1">
                              <h6 class="fw-bold">${item.employeeName}</h6>
                              <span class="fs-12 fw-normal ">${item.employeeDepartment || 'HRM'}</span>
                            </div>
                          </div>
                        </td>
                        
                        <td class="hdDescription align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                          <div class="d-flex align-items-center">
                            <p class="fs-14 fw-medium d-flex align-items-center mb-0">${item.leaveType}</p>
                            <span href="#" class="ms-2" data-bs-toggle="tooltip" data-bs-placement="right"
                              data-bs-title="I am currently experiencing a fever and design & Development">
                              <i class="ti ti-info-circle text-info"></i>
                          </span>
                          </div>
                        </td>

                        <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.fromDate}</td>
                        <td class="leaveTo align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.toDate}</td>
                        <td class="leaveTotalDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.period}</td>
                         <td class="leaveTotalDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${unitLabel}</td>
  
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



