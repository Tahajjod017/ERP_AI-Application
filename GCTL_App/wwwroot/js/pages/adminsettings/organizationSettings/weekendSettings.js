$('#OrganizationID').on('change', function () {
    var orgId = $(this).val();

    if (orgId) {
        $.ajax({
            url: '/WeekendSettings/GetBranches',
            type: 'GET',
            data: { organizationId: orgId },
            success: function (branches) {
                //var $branchSelect = $('#OrganizationBranchID');
                //$branchSelect.empty().append('<option value="">-- Select Branch --</option>');
                //$.each(branches, function (i, branch) {
                //    $branchSelect.append(`<option value="${branch.value}">${branch.text}</option>`);
                //});
                const simplifiedRoles = branches.map(role => ({
                    id: role.value,
                    name: role.text
                }));

                choiceManager.populateDropdown('OrganizationBranchID', simplifiedRoles);
                
            }
        });
    } else {
        $('#OrganizationBranchID').empty().append('<option value="">-- Select Branch --</option>');
    }
});

$(document).ready(function () {
    // Also initialize flatpickr for other date fields
    flatpickr("#StartDate", { dateFormat: "Y-m-d" });
    flatpickr("#EndDate", { dateFormat: "Y-m-d" });

    $('#weekendForm').on('submit', function (e) {
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
                // Handle Access Denied error (403)
                if (xhr.status === 403 && xhr.responseJSON && xhr.responseJSON.message === "Access denied.") {
                    // Redirect to AccessDenied page
                    window.location.href = '/Account/AccessDenied'; // Change URL to your actual AccessDenied page
                }
                // Handle Not Found error (404)
                else if (xhr.status === 404 && xhr.responseJSON && xhr.responseJSON.message) {
                    // Display the message from the JSON response
                    toastr.error(xhr.responseJSON.message, 'Not Found');
                }
                else {
                    toastr.error("Unexpected error: " + error, 'Server Error');
                }
            }

        });
    });
});

$(document).on('click', '#weekendSettings-singleDelBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/WeekendSettings/SoftDelete',
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
//edit
$(document).on('click', '#edit_weekend_settingBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#edit_weekend_setting').modal('show'); // Show the edit modal

    // Fetch the current data for the weekend setting using the ID (Example: Get the existing values from your backend)
    $.ajax({
        url: '/WeekendSettings/GetWeekendSettingById',  // Adjust the URL as per your endpoint
        type: 'GET',
        data: { id: approvalSettingID },
        success: function (data) {
            console.log(data.model.organizationID)
            if (data.model.success) {
                // Populate the modal fields with the data returned from the server
/*                $('#OrganizationEditID').val(data.OrganizationID);  // Set Organization ID*/

                choiceManager.setChoiceValue('#OrganizationEditID', data.model.organizationID)
               /* choiceManager.setChoiceValue('#OrganizationBranchEditID', data.model.OrganizationBranchID)*/
                

                // Pre-select the weekend days
                /*$('#WeekendDaysEdit').val(data.WeekendDays).trigger('change');*/ // Assuming WeekendDays is an array
            } else {
                
            }
        },
        error: function (xhr, status, error) {
            
        }
    });
});

// Organization Change: Populate Branch Dropdown when Organization is changed
$('#OrganizationEditID').on('change', function () {
    var orgId = $(this).val();

    if (orgId) {
        $.ajax({
            url: '/WeekendSettings/GetBranches',  // API to get branches based on the selected organization
            type: 'GET',
            data: { organizationId: orgId },
            success: function (branches) {
                const simplifiedRoles = branches.map(role => ({
                    id: role.value,
                    name: role.text
                }));

                choiceManager.populateDropdown('OrganizationBranchEditID', simplifiedRoles);
            },
            error: function (xhr, status, error) {
                console.error('Error fetching branches:', error);
            }
        });
    } else {
        // Clear the Branch dropdown if no organization is selected
        $('#OrganizationBranchEditID').empty().append('<option value="">-- Select Branch --</option>');
    }
}); 


//////////////////////////////Data Table Initialization//////////////////////////////
//////////////////////////////Data Table Initialization//////////////////////////////
var currentPage = 1;
var pageSize = 5;

$('#weekendSettings-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#weekendSettings-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#weekendSettings-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#weekendSettings-nextPageBtn").on('click', function () {
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
    var searchTerm = $("#weekendSettings-searchInput").val();
    $.ajax({
        url: '/WeekendSettings/GetAlls',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#weekendSettings-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.weekendSettingID}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap pe-10">${rowIndex}</td>
                            
                            <td class="align-middle white-space-nowrap ps-5">${item.organizationName}</td>
                            <td class="align-middle white-space-nowrap ps-5">${item.organizationBranchName}</td>
                            <td class="align-middle white-space-nowrap ps-5">${item.weekendTitle}</td>
                           
                            <td class="align-middle white-space-nowrap text-end pe-0">
                            <div class="d-flex justify-content-end align-items-center">
                                     <a
                                           href="#"
                                           title="Edit"
                                           id="edit_weekend_settingBtn"
                                           data-id="${item.weekendSettingID}"
                                           class="btn btn-outline-light btn-icon me-1 " 
                                           data-bs-toggle="" 
                                           data-bs-target=""
                                          >
                                           <i class="fas fa-edit text-black"></i>
                                    </a>
                                        <a 
                                          href="#" title="Delete"  data-id="${item.weekendSettingID}"
                                          class="btn btn-outline-light btn-icon"  
                                          id="weekendSettings-singleDelBtn" >
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

            $("#weekendSettings-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#weekendSettings-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#weekendSettings-paginationLinks");
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
    $("#weekendSettings-prevPageBtn").prop('disabled', currentPage === 1);
    $("#weekendSettings-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});