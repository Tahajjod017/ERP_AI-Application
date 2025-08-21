
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
                    clear();
                } else {
                    const allFields = ["OrganizationID", "HolidayTitle", "StartDate", "EndDate", "TotalDays", "StatusID"];

                    allFields.forEach(function (fieldId) {
                        validateField(fieldId, response);
                    });

                    toastr.info(response.message, 'Error');
                }
            },
            error: function (xhr, status, error) {
                // Handle Access Denied error (403)
                if (xhr.status === 403 && xhr.responseJSON && xhr.responseJSON.message === "Access denied.") {
                    // Redirect to AccessDenied page
                    window.location.href = '/Home/AccessDenied'; // Change URL to your actual AccessDenied page
                } else {
                    toastr.info("Unexpected error: " + error, 'Server Error');
                }
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

//edit
$(document).on('click', '#edit_holiday_settingBtn', function () {
    var holidaySettingID = $(this).data('id');
    $('#edit_holiday_setting').modal('show'); // Show the delete confirmation modal

    // Store the ID in the hidden input field
    $('#HolidayIDEdit').val(holidaySettingID);

    // Load the existing data for the selected holiday setting
    $.ajax({
        url: '/HolidaySettings/GetById',
        method: 'GET',
        data: { id: holidaySettingID },
        success: function (response) {
            if (response.isSuccess) {
                // Populate the form fields with the existing data  
                choiceManager.setChoiceValue('OrganizationEditID', response.data.organizationID);
                $('#HolidayTitleedit').val(response.data.holidayTitle);
                $('#HolidayDescriptionEdit').val(response.data.holidayDescription);
                $('#StartDateEdit').val(response.data.startDate);
                $('#EndDateEdit').val(response.data.endDate);
                $('#TotalDaysEdit').val(response.data.totalDays);
               
                choiceManager.setChoiceValue('StatusEditID', response.data.statusID);
                // Initialize the datepicker for the edit form
                initializeDatepickerDMY("editStartDate, editEndDate"); // For dd/MM/yyyy
                // For Date restriction with total days count
                $(document).on('change', "#editStartDate", function () {
                    const fromDate = $("#editStartDate").val();
                    updateDatepickerWithMinDateTotalDays("editEndDate", fromDate, {}, "editTotalDays", "editStartDate");
                });
            } else {
                toastr.error(response.message, 'Error');
            }
        },
        error: function (xhr, status, error) {
            // Handle Access Denied error (403)
            if (xhr.status === 403 && xhr.responseJSON && xhr.responseJSON.message === "Access denied.") {
                // Redirect to AccessDenied page
                window.location.href = '/Home/AccessDenied'; // Change URL to your actual AccessDenied page
            } else {
                toastr.error("Unexpected error: " + error, 'Server Error');
            }
        }
    });

   /* $('#confirmDeleteBtn').data('id', approvalSettingID); /*/// Store the approvalSettingID on the "Yes, Delete" button
});

$('#holidayEditForm').submit(function (event) {
    event.preventDefault(); // Prevent default form submission
    
    var formData = $(this).serialize(); // Serialize the form data

    // Append the approvalSettingID to the form data
    // formData += '&approvalSettingID=' + weekendSettingID;

    // Send the data via AJAX
    $.ajax({
        url: '/HolidaySettings/Updates', // Adjust URL if necessary
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.isSuccess) {
                // Handle success
                toastr.success('Holiday setting updated successfully!');
                $('#edit_holiday_setting').modal('hide'); // Hide the modal
                loadTableData();
            } else {
                // Handle failure
                toastr.error('Failed to update weekend setting: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            // Handle AJAX errors
            toastr.error('Error: ' + error);
        }
    });
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


$('#holidaySettings_resetBtn').on('click', function (e) {
    e.preventDefault();

    clear();
})

function clear() {
    $('#holidayForm')[0].reset();
    choiceManager.resetChoice('OrganizationID');
    choiceManager.resetChoice('StatusID');
    loadTableData();
    resetValidation(["OrganizationID", "HolidayTitle", "StartDate", "EndDate", "TotalDays", "StatusID"]);
}


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
                            <td class="align-middle text-start white-space-nowrap">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap">${item.holidayTitle}</td>
                             <td class="align-middle white-space-nowrap">${item.holidayDescription}</td>
                            <td class="align-middle white-space-nowrap">${item.startDate}</td>
                            <td class="align-middle white-space-nowrap">${item.endDate}</td>
                            <td class="text-center align-middle white-space-nowrap">${item.totalDays}</td>
                            <td class="text-start align-middle white-space-nowrap">${item.statusName}</td>
                             <td class="align-middle white-space-nowrap text-end">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="edit_holiday_settingBtn"
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