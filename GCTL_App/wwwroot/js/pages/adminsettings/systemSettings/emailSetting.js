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

$('#smtpForm').on('submit', function (e) {
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

//delete 
$(document).on('click', '#emailSettingssDelete_singleDelBtn', function () {
    var emailSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', emailSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/EmailSetting/SoftDelete',
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

/////////table
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
                                <input type="checkbox" class="form-check-input bloodGroup-selectItem" data-id="${item.emailSettingID}" />
                            </td>
                            
                             <td class="align-middle white-space-nowrap ">${item.organizationName}</td>
                            <td class="align-middle white-space-nowrap ">${item.serverName}</td>
                          
                           
                            <td class="align-middle white-space-nowrap ps-0">${item.userName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.password}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.priorityIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.isActive ? "Active" : "InActive"}</td>
                             <td>
                            <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="edit_emailSettings_settingBtn"
                               data-id="${item.emailSettingID}"
                               class="btn btn-outline-light btn-icon me-1 " 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_emailSettings_setting"
                              >
                               <i class="fas fa-edit text-black"></i>
                        </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.emailSettingID}"
                              class="btn btn-outline-light btn-icon"  
                              id="emailSettingssDelete_singleDelBtn" >
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

/// edit function 
    //edit
$(document).on('click', '#edit_emailSettings_settingBtn', function () {
    var weekendSettingID = $(this).data('id');
    $('#edit_emailSettings_setting').modal('show'); // Show the edit modal

    // Store the ID in the hidden input field
    $('#EmailSettingIDEdit').val(weekendSettingID);


    // Fetch the current data for the weekend setting using the ID (Example: Get the existing values from your backend)
    $.ajax({
        url: '/EmailSetting/GetById',  // Adjust the URL as per your endpoint
        type: 'GET',
        data: { id: weekendSettingID },
        success: function (response) {
            debugger
            //orgazationEditDropdown();
            choiceManager.setChoiceValue('OrganizationIDedit', response.data.organizationID);
            $('#ServerNameEdit').val(response.data.serverName);
            $('#PortNumberEdit').val(response.data.portNumber);
            $('#IsSSLRequiredEdit').val(response.data.isSSLRequired);
            $('#IsSMTPAuthenticationRequiredEdit').val(response.data.isSMTPAuthenticationRequired);
            $('#UserNameEdit').val(response.data.userName);
            $('#PasswordEdit').val(response.data.password);
            $('#PriorityIndexEdit').val(response.data.priorityIndex);
            $('#IsActiveEdit').val(response.data.isActive);


        },
        error: function (xhr, status, error) {

        }
    });
});

$('#smtpEditFormEdit').submit(function (event) {
    event.preventDefault(); // Prevent default form submission
   // var weekendSettingID = $('#localizationId').data('id');  // Get ID from the modal trigger
    var formData = $(this).serialize(); // Serialize the form data

    // Append the approvalSettingID to the form data
    // formData += '&approvalSettingID=' + weekendSettingID;

    // Send the data via AJAX
    $.ajax({
        url: '/EmailSetting/Updates', // Adjust URL if necessary
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.isSuccess) {
                // Handle success
                toastr.success('Email setting updated successfully!');
                $('#edit_Localization_setting').modal('hide'); // Hide the modal
                loadTableData();
            } else {
                // Handle failure
                toastr.error('Failed to update the setting: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            // Handle AJAX errors
            toastr.error('Error: ' + error);
        }
    });
});