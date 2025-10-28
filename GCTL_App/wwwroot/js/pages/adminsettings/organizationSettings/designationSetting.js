
$('#designationSettingsForm').on('submit', function (e) {
    e.preventDefault();
    if (!$('#OrganizationID').val()) {
        $('#OrganizationID').closest('.choices').addClass('is-invalid');
        toastr.error('Please select an organization');
        $('span[data-valmsg-for="OrganizationID"]').html('Organization is required');
        return;
    }
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
                choiceManager.resetChoice('OrganizationID');
            } else {
                toastr.error(response.message, 'Error');
            }
        },
        error: function (xhr, status, error) {
            toastr.error("Unexpected error: " + error, 'Server Error');
        }
    });
});

$(document).on('click', '#addDesignationSetting-singleDelBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/DesignationSettings/SoftDelete',
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


$(document).on('click', '#designationIDEditButton', function () {
    var designationID = $(this).data('id');
    $('#edit_designation_setting').modal('show'); // Show the delete confirmation modal

    // Store the ID in the hidden input field
    $('#DesignationIDEdit').val(designationID);

    //// Load the existing data for the selected holiday setting
    $.ajax({
        url: '/DesignationSettings/GetById',
        method: 'GET',
        data: { id: designationID },
        success: function (response) {
            if (response.isSuccess) {
                // Populate the form fields with the existing data  
                choiceManager.setChoiceValue('OrganizationIDEdit', response.data.organizationID);
                $('#DesignationNameEdit').val(response.data.designationName);
              
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

$('#designationSettingsFormEdit').submit(function (event) {
    event.preventDefault(); // Prevent default form submission

    var formData = $(this).serialize(); // Serialize the form data

    // Append the approvalSettingID to the form data
    // formData += '&approvalSettingID=' + weekendSettingID;

    // Send the data via AJAX
    $.ajax({
        url: '/DesignationSettings/Updates', // Adjust URL if necessary
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.isSuccess) {
                // Handle success
                toastr.success('Designation setting updated successfully!');
                $('#edit_designation_setting').modal('hide'); // Hide the modal
                loadTableData();
            } else {
                // Handle failure
                toastr.error('Failed to update Designation setting:' + response.message);
            }
        },
        error: function (xhr, status, error) {
            // Handle AJAX errors
            toastr.error('Error: ' + error);
        }
    });
});

//table

var currentPage = 1;
var pageSize = 5;

$('#addDesignationSetting-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#addDesignationSetting-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#addDesignationSetting-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#addDesignationSetting-nextPageBtn").on('click', function () {
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
    var searchTerm = $("#addDesignationSetting-searchInput").val();

    $.ajax({
        url: '/DesignationSettings/GetDesignations',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#addDesignationSetting-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.designationID}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap pe-12">${rowIndex}</td>
                            
                             <td class="align-middle white-space-nowrap ps-4">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.designationName}</td>
                             <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="designationIDEditButton"
                               data-id="${item.designationID}"
                               class="btn btn-outline-light btn-icon me-1 " 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves"
                              >
                               <i class="fas fa-edit text-black"></i>
                        </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.designationID}"
                              class="btn btn-outline-light btn-icon"  
                              id="addDesignationSetting-singleDelBtn" >
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

            $("#addDesignationSetting-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#addDesignationSetting-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#addDesignationSetting-paginationLinks");
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
    $("#addDesignationSetting-prevPageBtn").prop('disabled', currentPage === 1);
    $("#addDesignationSetting-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});





