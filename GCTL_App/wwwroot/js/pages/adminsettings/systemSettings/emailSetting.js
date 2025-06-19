$(document).ready(function () {
    $('#bloodGroup-check-all').on('change', function () {
        var isChecked = $(this).prop('checked');
        $('.bloodGroup-selectItem').prop('checked', isChecked);

        toggleBulkActions();
    });

    $(document).on('change', '.bloodGroup-selectItem', function () {
        toggleBulkActions();
    });
});

function toggleBulkActions() {
    const allItems = $('.bloodGroup-selectItem');
    const checkedItems = $('.bloodGroup-selectItem:checked');

    const allChecked = allItems.length === checkedItems.length;
    const someChecked = checkedItems.length > 0 && !allChecked;

    $('#bloodGroup-check-all').prop('checked', allChecked);
    $('#bloodGroup-check-all').prop('indeterminate', someChecked);

    if (checkedItems.length > 1) {
        $('#bloodGroup-bulkSelectActions').removeClass('d-none');
        $('#bloodGroup-searchBox').addClass('d-none');
        $('.bloodGroup-bulkDelete').addClass('disabled');
        $('.bloodGroup-bulkEdit').addClass('disabled');
    } else {
        $('#bloodGroup-bulkSelectActions').addClass('d-none');
        $('#bloodGroup-searchBox').removeClass('d-none');
        $('.bloodGroup-bulkDelete').removeClass('disabled');
        $('.bloodGroup-bulkEdit').removeClass('disabled');
    }
}




var currentPage = 1;
var pageSize = 5;

$('#bloodGroup-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#addEmailConfig-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#addEmailConfig-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#addEmailConfig-nextPageBtn").on('click', function () {
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
    var searchTerm = $("#addEmailConfig-searchInput").val();

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
            var tableBody = $("#addEmailConfig-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input bloodGroup-selectItem" data-id="${item.bloodGroupID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.bloodGroupName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 bloodGroup-bulkDelete" href="#!" id="bloodGroup-edit" data-id="${item.bloodGroupID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 bloodGroup-bulkEdit" href="#!" id="bloodGroup-single-delete" data-id="${item.bloodGroupID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#addEmailConfig-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#bloodGroup-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#addEmailConfig-paginationLinks");
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
    $("#addEmailConfig-prevPageBtn").prop('disabled', currentPage === 1);
    $("#addEmailConfig-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});