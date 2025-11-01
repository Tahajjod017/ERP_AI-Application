
$('#departmentSettingsForm').on('submit', function (e) {
    e.preventDefault();
    // Add this simple validation check

    // === ONLY OrganizationID Validation ===
    if (!$('#OrganizationID').val()) {
        $('#OrganizationID').closest('.choices').addClass('is-invalid');
        toastr.error('Please select an organization');
        $('span[data-valmsg-for="OrganizationID"]').html('Organization is required');
        return;
    }
    // === END VALIDATION ===
    if (!$(this).valid()) return; // Built-in validation check
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
 
// Clear validation when user selects organization
$('#OrganizationID').on('change', function () {
    $(this).closest('.choices').removeClass('is-invalid');
    $('span[data-valmsg-for="OrganizationID"]').html('');
});
$(document).on('click', '#addDepartmentSettings-singleDelBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/DepartmentSettings/SoftDelete',
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


$(document).on('click', '#departmentIDEditButton', function () {
    var departmentID = $(this).data('id');
   // $('#edit_department_setting').modal('show'); // Show the delete confirmation modal
    var myModal = new bootstrap.Modal(document.getElementById('edit_department_setting'));
    myModal.show();


    // Store the ID in the hidden input field
    $('#DepartmentIDEdit').val(departmentID);

    //// Load the existing data for the selected holiday setting
    $.ajax({
        url: '/DepartmentSettings/GetById',
        method: 'GET',
        data: { id: departmentID },
        success: function (response) {
            if (response.isSuccess) {
                // Populate the form fields with the existing data  
                choiceManager.setChoiceValue('OrganizationIDEdit', response.data.organizationID);
                $('#DepartmentNameEdit').val(response.data.departmentName);
                

                choiceManager.setChoiceValue('DepartmentHeadEmpIDEdit', response.data.departmentHeadEmpID);
              


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

$('#departmentSettingsFormEdit').submit(function (event) {
    event.preventDefault(); // Prevent default form submission

    var formData = $(this).serialize(); // Serialize the form data

    // Append the approvalSettingID to the form data
    // formData += '&approvalSettingID=' + weekendSettingID;

    // Send the data via AJAX
    $.ajax({
        url: '/DepartmentSettings/Updates', // Adjust URL if necessary
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.isSuccess) {
                // Handle success
                toastr.success('Department setting updated successfully!');
                $('#edit_department_setting').modal('hide'); // Hide the modal
                loadTableData();
                const modalEl = document.getElementById('edit_department_setting');
                const modalInstance = bootstrap.Modal.getOrCreateInstance(modalEl);
                modalInstance.hide();

                //// Force cleanup
                //modalEl.classList.remove('show');
                //modalEl.style.display = 'none';
                //document.body.classList.remove('modal-open');
                //document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
                //document.body.style.overflow = '';
            } else {
                // Handle failure
                toastr.error('Failed to update Department setting: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            // Handle AJAX errors
            toastr.error('Error: ' + error);
        }
    });
});

// Table

var currentPage = 1;
var pageSize = 5;

$('#addDepartmentSettings-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#addDepartmentSettings-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#addDepartmentSettings-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#addDepartmentSettings-nextPageBtn").on('click', function () {
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
    var searchTerm = $("#addDepartmentSettings-searchInput").val();

    $.ajax({
        url: '/DepartmentSettings/GetAll',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#addDepartmentSettings-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.departmentID}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap pe-4">${rowIndex}</td>
                            
                             <td class="align-middle white-space-nowrap ps-4">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.departmentName}</td>
                            <td class="align-middle white-space-nowrap ps-4">${item.headEmployeeName}</td>
                             <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="departmentIDEditButton"
                               data-id="${item.departmentID}"
                               class="btn btn-outline-light btn-icon me-1 " 
                              
                              >
                               <i class="fas fa-edit text-black"></i>
                        </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.departmentID}"
                              class="btn btn-outline-light btn-icon"  
                              id="addDepartmentSettings-singleDelBtn" >
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

            $("#addDepartmentSettings-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#addDepartmentSettings-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#addDepartmentSettings-paginationLinks");
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
    $("#addDepartmentSettings-prevPageBtn").prop('disabled', currentPage === 1);
    $("#addDepartmentSettings-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});