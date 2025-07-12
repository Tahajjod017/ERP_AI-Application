
$(document).ready(function () {

    //added By 404
    initializeDatepickerDMY("StartDate, EndDate");  // For dd/MM/yyyy
    // for Date restriction with total days count
    $(document).on('change', "#StartDate", function () {
        const fromDate = $("#StartDate").val();
        updateDatepickerWithMinDateTotalDays("EndDate", fromDate, {}, "TotalDays", "StartDate");
    });


    // Also initialize flatpickr for other date fields
    //flatpickr("#StartDate", { dateFormat: "Y-m-d" });
    //flatpickr("#EndDate", { dateFormat: "Y-m-d" });

    $('#holidayForm').on('submit', function (e) {
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
});

$(document).on('click', '#addHolidayConfig-singleDelBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/HolidaySettings/SoftDelete',
            method: 'POST',
            data: { ids: [id] },
            success: function (response) {
                if (response.isSuccess) {
                    toastr.success(response.message);
                    // Optionally, reload the table data or remove the deleted row from the table
                    loadTableData(); // Reload data after delete
                } else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error("Error occurred while deleting.");
            },
            complete: function () {
                // Hide the modal after the action
                $('#confirmDeleteModal').modal('hide');
            }
        });
    } else {
        toastr.error("Invalid action.");
    }
});

//////////////////////////////Data Table Initialization//////////////////////////////
var currentPage = 1;
var pageSize = 5;

$('#addHolidayConfig-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#addHolidayConfig-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#addHolidayConfig-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#addHolidayConfig-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});

let currentSortColumn = 'HolidayTitle';
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
    var searchTerm = $("#addHolidayConfig-searchInput").val();
    $.ajax({
        url: '/HolidaySettings/GetAlls',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#addHolidayConfig-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.holidayID}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap pe-4">${rowIndex}</td>
                            
                             <td class="align-middle white-space-nowrap ">${item.holidayTitle}</td>
                             <td class="align-middle white-space-nowrap ">${item.holidayDescription}</td>
                            <td class="align-middle white-space-nowrap ps-4">${item.startDate}</td>
                            <td class="align-middle white-space-nowrap ps-4">${item.endDate}</td>
                            <td class="text-center align-middle white-space-nowrap pe-4">${item.totalDays}</td>
                            <td class=" text-center align-middle white-space-nowrap pe-4">${item.statusName}</td>
                             <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="LeaveRequestEditButton"
                               data-id="${item.holidayID}"
                               class="btn btn-outline-light btn-icon me-1 " 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves"
                              >
                               <i class="fas fa-edit text-black"></i>
                        </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.holidayID}"
                              class="btn btn-outline-light btn-icon"  
                              id="addHolidayConfig-singleDelBtn" >
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

            $("#addHolidayConfig-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#addHolidayConfig-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#addHolidayConfig-paginationLinks");
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
    $("#addHolidayConfig-prevPageBtn").prop('disabled', currentPage === 1);
    $("#addHolidayConfig-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});