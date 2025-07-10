
$(document).ready(function () {
    const designationsUrl = '/ApprovalSettings/GetDesignation';

    const approvers = [
        { checkboxId: "chkFirst", selectId: "selFirst" },
        { checkboxId: "chkSecond", selectId: "selSecond" },
        { checkboxId: "chkThird", selectId: "selThird" }
    ];

    // Function to fetch data and populate dropdown using jQuery's $.ajax()
    function populateOptions(selectEl, url, organId) {
        let defaultOptionText = 'Select Designation';
        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            data: { organizationId: organId },
            success: function (data) {
                console.log('Fetched data:', data);

                const simplifiedRoles = data.map(role => ({
                    value: role.value,
                    label: role.text
                }));

                // Use choiceManager to populate dropdown
                choiceManager.populateDropdown(selectEl.attr('id'), simplifiedRoles);
            },
            error: function (xhr, status, error) {
                console.error('Error loading options:', error);
            }
        });
    }

    // Initial population on page load
    var organizationId = $('#OrganizationID').val();

    approvers.forEach(ap => {
        const checkbox = $("#" + ap.checkboxId);
        const select = $("#" + ap.selectId);

        // Initially disable all select dropdowns
        select.prop('disabled', true);
        choiceManager.disableChoice(ap.selectId);

        // Populate options for the dropdown
        populateOptions(select, designationsUrl, organizationId);

        // Enable/Disable select dropdown based on checkbox change
        checkbox.on('change', function () {
            const isChecked = this.checked;

            // Enable/Disable the select dropdown based on checkbox state
            select.prop('disabled', !isChecked);

            if (isChecked) {
                // Enable dropdown using Choices.js
                choiceManager.enableChoice(ap.selectId);
                populateOptions(select, designationsUrl, organizationId); // Re-populate options when enabled
            } else {
                // Disable dropdown using Choices.js
                choiceManager.disableChoice(ap.selectId);
            }

            // Handle dependent checkboxes if needed (e.g., disabling second or third checkboxes)
            if (ap.checkboxId === "chkFirst" && !isChecked) {
                $("#chkSecond").prop('disabled', true);
                $("#selSecond").prop('disabled', true);
            } else if (ap.checkboxId === "chkFirst" && isChecked) {
                $("#chkSecond").prop('disabled', false);
            }

            if (ap.checkboxId === "chkSecond" && !isChecked) {
                $("#chkThird").prop('disabled', true);
                $("#selThird").prop('disabled', true);
            } else if (ap.checkboxId === "chkSecond" && isChecked) {
                $("#chkThird").prop('disabled', false);
            }
        });
    });

    // Repopulate dropdowns when organization changes
    $('#OrganizationID').on('change', function () {
        var organizationId = $(this).val();

        approvers.forEach(ap => {
            const select = $("#" + ap.selectId);
            populateOptions(select, designationsUrl, organizationId);
        });
    });
});












// Function to handle form submission

$(document).ready(function () {
    

    $('#aprovalSettingsForm').on('submit', function (e) {
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
                    loadTableData();
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


// delete 
$(document).on('click', '#approvalSettingsDelete-singleDelBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#confirmDeleteModal').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});
$(document).on('click', '#confirmDeleteBtn', function () {
    var id = $(this).data('id');
    if (id) {
        $.ajax({
            url: '/ApprovalSettings/SoftDelete',
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
//$(document).on('click', '#approvalSettingsDelete-single-delete', function () {

//    var id = $(this).data('id');
//    alert(id)
//    if (id) {
//        showDeleteModal(function () {
//            $.ajax({
//                url: '/ApprovalSettings/SoftDelete',
//                method: 'POST',
//                data: { ids: [id] },
//                success: function (response) {
//                    if (response.isSuccess) {
//                        toastr.success(response.message);
//                        $("#approvalSettings-check-all").prop('checked', false);
//                        clear();
//                    } else {
//                        toastr.error(response.message);
//                    }
//                },
//                error: function () {
//                    toastr.error("Error occurred while deleting.");
//                }
//            });
//        });
//    } else {
//        toastr.error("Invalid action.");
//    }
//});

//edit
$(document).on('click', '#edit_approval_settingBtn', function () {
    var approvalSettingID = $(this).data('id');
    $('#edit_approval_setting').modal('show'); // Show the delete confirmation modal
    $('#confirmDeleteBtn').data('id', approvalSettingID); // Store the approvalSettingID on the "Yes, Delete" button
});

//////////////////////////////Data Table Initialization//////////////////////////////
var currentPage = 1;
var pageSize = 5;

$('#approvalSettings-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#approvalSettings-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#approvalSettings-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#approvalSettings-nextPageBtn").on('click', function () {
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
    var searchTerm = $("#approvalSettings-searchInput").val();
    $.ajax({
        url: '/ApprovalSettings/GetAllData',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#approvalSettings-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.approvalSettingID}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap pe-4">${rowIndex}</td>
                            
                             <td class="align-middle white-space-nowrap ps-4">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.approvalTypeName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.firstApprovalName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.secondApprovalName}</td>
                             <td class="align-middle white-space-nowrap ps-4">${item.thirdApprovalName}</td>
                           
                             <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="edit_approval_settingBtn"
                               data-id="${item.approvalSettingID}"
                               class="btn btn-outline-light btn-icon me-1 " 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves"
                              >
                               <i class="fas fa-edit text-black"></i>
                        </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.approvalSettingID}"
                              class="btn btn-outline-light btn-icon"  
                              id="approvalSettingsDelete-singleDelBtn" >
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

            $("#approvalSettings-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#approvalSettings-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
} 

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#approvalSettings-paginationLinks");
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
    $("#approvalSettings-prevPageBtn").prop('disabled', currentPage === 1);
    $("#approvalSettings-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});

