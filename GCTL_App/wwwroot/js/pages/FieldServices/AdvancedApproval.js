



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
        url: '/AdvancedApproval/GetAll',
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
            var tableBody = $("#empApproval-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td>
                                <input type="checkbox" class="form-check-input addEmpCheck-selectedItem" data-id="${item.employeeAdvanceID}" />
                            </td>
                            <td class="empId align-middle justify-text-center white-space-nowrap ps-5 fw-semibold text-body py-1">${item.customerID2}
                            </td>
                            <td class="empName align-middle justify-text-center white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">${item.customerName}  
                            </td>
                            <td class="empProjectName align-middle justify-text-center white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTitle || 'N/A'}</td>
                            <td class="empProjectType align-middle justify-text-center white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTypeName || 'N/A'}</td>
                            <td class="empSalary align-middle justify-text-center white-space-nowrap ps-4 fw-semibold text-body py-1">${item.amountRequested || 0}</td>
                            <td class="empGroupName align-middle justify-text-center white-space-nowrap ps-4 fw-semibold text-body py-1">${item.groupEmployeeName}  
                            </td>
                           <td class="empStatus align-middle justify-text-center white-space-nowrap ps-4 fw-semibold text-body py-1">
                              <span class="badge badge-phoenix badge-phoenix-warning">
                                ${item.statusName}
                              </span>
                            </td>
                            <td class="empapprovedName align-middle justify-text-center white-space-nowrap ps-4 fw-semibold text-body py-1">${item.requestedByUser || 0}</td>
                            <td class="empDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.startDate} </td>
                            <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                                <div class="d-flex btn-reveal-trigger position-static">
                                    <a href="#!"
                                       class="btn btn-phoenix-danger btn-icon me-1 fs-10 text-body px-0 employeeAdvance-editBtn"
                                       data-bs-toggle="modal" data-bs-target="#editModal"
                                       data-id="${item.employeeAdvanceID}"
                                       title="Add"
                                       >
                                       <i class="fa-solid fa-eye"></i>
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

//Modal Show
document.querySelector('.employeeAdvance-editBtn').addEventListener('click', function (e) {
    e.preventDefault();
    const modal = new bootstrap.Modal(document.getElementById('editModal'));
    modal.show();
});

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



//#region loadTable2

var currentPage2 = 1;
var pageSize2 = 5;
$('#empAdvanced2-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();
    if (selectedSize) {
        pageSize2 = parseInt(selectedSize, 10);
        currentPage2 = 1;
        loadTableData2();
    }
});

$(document).ready(function () {
    loadTableData2();

    $("#empAdvanced-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData2();
    });

    $("#empAdvanced-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData2();
        }
    });

    $("#empAdvanced-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData2();
    });
});


let currentSortColumn2 = 'EmployeeAdvanceID';
let currentSortOrder2 = 'desc';

$('th.sort').on('click', function () {
    const column = $(this).data('sort');

    if (currentSortColumn2 === column) {
        currentSortOrder2 = currentSortOrder2 === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn2 = column;
        currentSortOrder2 = 'asc';
    }

    loadTableData2(currentSortColumn2, currentSortOrder2);
    updateSortingIndicator(column, currentSortOrder2);
});


function updateSortingIndicator() {
    $('th.sort').each(function () {
        const $th = $(this);
        const column = $th.data('sort');
        $th.find('.sort-icon').remove();

        if (column === currentSortColumn2) {
            const iconClass = currentSortOrder2 === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
            $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
        } else {
            $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
        }
    });
}

function loadTableData2(sortColumn, sortOrder) {
    var searchTerm = $("#empAdvanced-searchInput").val();

    $.ajax({
        url: '/AdvancedApproval/GetAll',
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
            var tableBody = $("#advancedApproval2-tbody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="empId align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">${item.customerID2}
                            </td>
                            <td class="empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">${item.customerName} 
                            </td>
                            <td class="empProjectName align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTitle}</td>
                            <td class="empProjectType align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.jobTypeName}</td>
                            <td class="empDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.startDate} </td>
                            <td class="empSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.amountRequested}</td>
                            <td class="empStatus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">
                              <span class="badge badge-phoenix badge-phoenix-warning">
                                ${item.statusName}
                              </span>
                            </td>
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
    const windowSize = 1;
    const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;
   
    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
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
    loadTableData2();
});
//#endregion


