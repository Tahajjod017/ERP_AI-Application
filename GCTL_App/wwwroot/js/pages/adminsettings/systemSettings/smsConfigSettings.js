$('#smsForm').on('submit', function (e) {
    e.preventDefault();

    var form = $(this);
    var formData = form.serialize();

    $.ajax({
        url: form.attr('action'),
        method: 'POST',
        data: formData,
        success: function (response) {
            if (response.isSuccess) {
                toastr.success(response.message, '');
                form.trigger("reset");
            } else {
                toastr.error(response.message, 'Error');
            }
        },
        error: function (xhr, status, error) {
            toastr.error("Unexpected error: " + error, 'Server Error');
        }
    });
});

//////////////////Table
var currentPage = 1;
var pageSize = 5;

$('#addSmsConfig-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#addSmsConfig-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#addSmsConfig-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#addSmsConfig-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});


let currentSortColumn = 'BloodGroupName';
let currentSortOrder = 'asc';

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
    var searchTerm = $("#addSmsConfig-searchInput").val();

    $.ajax({
        url: '/EmailSetting/GetAll',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#addSmsConfig-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input bloodGroup-selectItem" data-id="${item.emailSettingID}" />
                            </td>
                           
                             <td class="align-middle white-space-nowrap ps-0">${item.organizationName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.serverName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.portNumber}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.isSSLRequired ? "Yes" : "No"}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.isSMTPAuthenticationRequired ? "Yes" : "No"}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.friorityIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.userName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.password}</td>
                           
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 bloodGroup-bulkDelete" href="#!" id="bloodGroup-edit" data-id="${item.emailSettingID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 bloodGroup-bulkEdit" href="#!" id="bloodGroup-single-delete" data-id="${item.emailSettingID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#addSmsConfig-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#addSmsConfig-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#addSmsConfig-paginationLinks");
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
    $("#addSmsConfig-prevPageBtn").prop('disabled', currentPage === 1);
    $("#addSmsConfig-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});