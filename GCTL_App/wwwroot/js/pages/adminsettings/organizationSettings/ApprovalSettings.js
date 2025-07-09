//$(document).ready(function () {
//    // Get references to the elements
//    const employeesUrl = '/ApprovalSettings/GetEmployee';
//    const designationsUrl = '/ApprovalSettings/GetDesignation';

//    const approvers = [
//        { checkboxId: "chkFirst", selectId: "selFirst", toggleId: "toggleFirst" },
//        { checkboxId: "chkSecond", selectId: "selSecond", toggleId: "toggleSecond" },
//        { checkboxId: "chkThird", selectId: "selThird", toggleId: "toggleThird" }
//    ];

//    approvers.forEach(ap => {
//        const checkbox = $("#" + ap.checkboxId);
//        const select = $("#" + ap.selectId);
//        const toggle = $("#" + ap.toggleId);

//        // Enable/Disable select dropdown based on checkbox
//        checkbox.on('change', function () {
//            select.prop('disabled', !this.checked);
//            toggle.prop('disabled', !this.checked);
//        });

//        // Toggle between employees and designations
//        toggle.on('change', function () {
//            const url = this.checked ? employeesUrl : designationsUrl;
//            populateOptions(select, url);
//        });

//        // Initialize with designations (default)
//        select.prop('disabled', true);
//        toggle.prop('disabled', true);
//        populateOptions(select, designationsUrl); // Default to designations


//    });
//    //$(document).ready(function () {
//    //    // Get the checkbox element by ID
//    //    const checkbox = document.getElementById('chkThird');

//    //    // Function to check if the checkbox is checked
//    //    function checkCheckbox() {
//    //        const isChecked = checkbox.checked; // true if checked, false if unchecked
//    //        alert('Checkbox checked: ' + isChecked); // Show value in alert
//    //    }

//    //    // Call this function to check the checkbox state
//    //    checkCheckbox();

//    //    // Optionally, add an event listener to trigger the check when the checkbox changes
//    //    checkbox.addEventListener('change', function () {
//    //        alert('Checkbox checked: ' + checkbox.checked); // Show value in alert on change
//    //    });

//    //});


//    // Function to fetch data and populate dropdown using jQuery's $.ajax()
//    function populateOptions(selectEl, url) {
//        let defaultOptionText = '';

//        // Determine the default text based on the URL (employees or designations)
//        if (url === employeesUrl) {
//            defaultOptionText = 'Select Employee';
//        } else if (url === designationsUrl) {
//            defaultOptionText = 'Select Designation';
//        }

//        $.ajax({
//            url: url,
//            type: 'GET',
//            dataType: 'json',
//            success: function (data) {
//                selectEl.empty();  // Clear existing options
//                selectEl.append('<option value="">' + defaultOptionText + '</option>');  // Set dynamic default option

//                // Append options from the response data
//                $.each(data, function (index, item) {
//                    const option = $('<option></option>').val(item.value).text(item.text);  // Adjust item properties based on your API response
//                    selectEl.append(option);
//                });
//            },
//            error: function (xhr, status, error) {
//                console.error('Error loading options:', error);
//            }
//        });
//    }

//});

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
                selectEl.empty();
                selectEl.append('<option value="">' + defaultOptionText + '</option>');
                $.each(data, function (index, item) {
                    const option = $('<option></option>').val(item.value).text(item.text);
                    selectEl.append(option);
                });
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

        // Enable/Disable select dropdown based on checkbox change
        checkbox.on('change', function () {
            select.prop('disabled', !this.checked);

            if (ap.checkboxId === "chkFirst") {
                $("#chkSecond").prop('disabled', !this.checked);
                if (!this.checked) $("#selSecond").prop('disabled', true);
               
            }
            if (ap.checkboxId === "chkSecond") {
                $("#chkThird").prop('disabled', !this.checked);
                if (!this.checked) $("#selThird").prop('disabled', true);
            }
        });

        select.prop('disabled', true);
        populateOptions(select, designationsUrl, organizationId);
    });

    // Repopulate dropdowns when organization changes
    $('#OrganizationID').on('change', function () {
        var organizationId = $(this).val();
        approvers.forEach(ap => {
            const select = $("#" + ap.selectId);
            populateOptions(select, designationsUrl, organizationId);
            // Remove alert(populateOptions); -- not useful for users
        });
    });
});


//$(document).ready(function () {
//    // 1. Initialize Choices.js for all dropdowns
//    choiceManager.initAll();

//    // 2. Checkbox references
//    var $chkFirst = $('#chkFirst');
//    var $chkSecond = $('#chkSecond');
//    var $chkThird = $('#chkThird');

//    // 3. Enable/disable/clear helper using Choices.js instance methods
//    function setDropdownState(id, enabled) {
//        var instance = choiceManager.instances[id];
//        if (instance) {
//            if (enabled) {
//                instance.enable();
//            } else {
//                instance.disable();
//                instance.removeActiveItems(); // Optionally clear selection
//            }
//        }
//    }

//    // 4. Hierarchy logic
//    function updateHierarchy() {
//        setDropdownState('selFirst', $chkFirst.is(':checked'));
//        setDropdownState('selSecond', $chkFirst.is(':checked') && $chkSecond.is(':checked'));
//        setDropdownState('selThird', $chkFirst.is(':checked') && $chkSecond.is(':checked') && $chkThird.is(':checked'));
//    }

//    // 5. Attach listeners
//    $chkFirst.change(updateHierarchy);
//    $chkSecond.change(updateHierarchy);
//    $chkThird.change(updateHierarchy);

//    // 6. Initial state
//    updateHierarchy();

//    // 7. AJAX to fetch and populate dropdown data
//    $.ajax({
//        url: '/ApprovalSettings/GetDesignation', // Replace with your actual endpoint
//        method: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            // Populate all three dropdowns with the same data
//            choiceManager.populateDropdown(['selFirst', 'selSecond', 'selThird'], data);
//        },
//        error: function () {
//            alert('Failed to load approver data.');
//        }
//    });

//    // 8. (Optional) Prevent skipping steps by auto-checking parents
//    $chkSecond.change(function () {
//        if ($chkSecond.is(':checked') && !$chkFirst.is(':checked')) {
//            $chkFirst.prop('checked', true);
//            updateHierarchy();
//        }
//    });
//    $chkThird.change(function () {
//        if ($chkThird.is(':checked')) {
//            if (!$chkSecond.is(':checked')) $chkSecond.prop('checked', true);
//            if (!$chkFirst.is(':checked')) $chkFirst.prop('checked', true);
//            updateHierarchy();
//        }
//    });
//});




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

