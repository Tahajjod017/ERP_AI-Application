
$(document).ready(function () {
    let userCount = 0;

    function createUserBlock(id) {
        return `
        <div class="border rounded-3 p-2 pt-1 pb-2 mb-3 user-block bg-light-subtle shadow-sm position-relative" id="user-block-${id}">
            <div class="position-absolute top-0 start-100 translate-middle">
                <button type="button" class="btn btn-sm btn-danger rounded-circle remove-user" data-id="${id}" title="Remove User">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <h6 class="fw-semibold text-primary mb-2 fs-sm">User ${id}</h6>

            <div class="row g-2">
                <div class="col-12">
                    <div class="form-floating form-floating-advance-select" >
                        <select class="form-select employeeCode" id="employeeCode-${id}" data-choices="data-choices">
                            <option value="">Select Employee</option>
                        </select>
                        <label for="employeeCode-${id}">Employee Code</label>
                    </div>
                </div>
            </div>
        </div>
    `;
    }


    function addUserBlock() {
        userCount++;
        const $block = $(createUserBlock(userCount)).hide().fadeIn(300);
        $("#userInputsContainer").append($block);

        $.ajax({
            url: '/Account/GetEmployeeCodes',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                const $select = $(`#employeeCode-${userCount}`);
                data.forEach(function (emp) {
                    $select.append(`<option value="${emp.code}" data-name="${emp.name}">${emp.code}</option>`);
                });

                var choiceInstance = new Choices(`#employeeCode-${userCount}`, {
                    removeItemButton: true,
                    placeholder: true,
                    searchEnabled: true,
                    shouldSort: true
                });
         
                // ✅ Attach event immediately
                $(`#employeeCode-${userCount}`).on('change', function () {
                    const selectedOption = $(this).find('option:selected');
                    const employeeName = selectedOption.data('name') || "";
                    $(this).closest('.user-block').find('.employeeName').val(employeeName);
                });

            }
        });
    }

    // Initially add one user block
    addUserBlock();

    // Add new user block on click
    $("#addNewUserBlock").click(function () {
        addUserBlock();
    });

    // Remove user block
    $(document).on("click", ".remove-user", function () {
        const id = $(this).data("id");
        $(`#user-block-${id}`).fadeOut(300, function () {
            $(this).remove();
        });
    });

    $("#multiUserForm").submit(function (e) {
        
        e.preventDefault(); // Prevent default form action

        const users = []; // Initialize empty array to collect users
   
        // Loop through each dynamic user block
        $(".user-block").each(function () {
        
            // Correct way to fetch selected value even with Choices.js
            const employeeCode = $(this).find(".employeeCode")[0].value;
           // const employeeName = $(this).find(".employeeName").val();

            if (employeeCode /*&& employeeName*/) {
               
                users.push({
                    employeeCode: employeeCode,
                    //employeeName: employeeName
                });
            }
        });

        if (users.length === 0) {
            alert("Please fill at least one user.");
            return;
        }
        
        // Now send the collected users array to server
        $.ajax({
            url: '/Account/CreateUsers', //  Your API endpoint
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(users),
            success: function (response) {
                toastr.success(response.message);
                $("#userInputsContainer").empty(); // Clear all user blocks
                userCount = 0; // Reset counter
                addUserBlock(); // Add a fresh first block
                loadUserTableData();
                window.location.reload(); // Reload the page to reflect changes
            },
            error: function (xhr) {
                if (xhr.status === 403) {
                    toastr.error('Access Denied');
                } else {
                    toastr.error(xhr.message);
                }
            }
        });
    });

});
$('#changePasswordForm').submit(function (e) {
    e.preventDefault();

    var oldPassword = $('#oldPassword').val();
    var newPassword = $('#newPassword').val();
    var confirmNewPassword = $('#confirmNewPassword').val();

    if (newPassword !== confirmNewPassword) {
        toastr.warning("New password and confirm password do not match!");
        return;
    }

    $.ajax({
        url: '/Account/ChangePassword',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            oldPassword: oldPassword,
            newPassword: newPassword
        }),
        success: function (response) {
            toastr.success(response.message);
            // Optionally, redirect or clear form
            $('#changePasswordForm')[0].reset();
        },
        error: function (xhr) {
            alert("Error: " + xhr.responseText);
        }
    });
});




////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                  Pagination Starts (User List)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

var currentPage = 1;
var pageSize = 5;

// Handle page size dropdown
$('.dropdown-item').on('click', function () {
    var selectedSize = $(this).data("size");
    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
    } else {
        return;
    }
    $('#selectedPageSize').text(selectedSize);
    loadUserTableData();
});

$(document).ready(function () {
    loadUserTableData();

    // Handle search input change
    $("#userList-searchInput").on("input", function () {
        currentPage = 1;  // Reset to first page when searching
        loadUserTableData();
    });

    // Handle pagination
    $("#userList-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadUserTableData();
        }
    });

    $("#userList-nextPageBtn").on('click', function () {
        currentPage++;
        loadUserTableData();
    });
});


// Declare sortColumn and sortOrder globally
let currentSortColumn = 'employeeCode';
let currentSortOrder = 'asc';

// Handle column sorting
$('th.sort').on('click', function () {
    const column = $(this).data('sort');

    if (currentSortColumn === column) {
        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortOrder = 'asc';
    }

    loadUserTableData(currentSortColumn, currentSortOrder);
});

function loadUserTableData(sortColumn, sortOrder) {
    var searchTerm = $("#userList-searchInput").val();

    $.ajax({
        url: '/UserList/GetPaginatedUsers', // <-- your controller action URL
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {

            var tableBody = $("#userList-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item) {
                   
                    var statusButtonHtml = `
                <a class="btn btn-icon fs-10 px-3 ps-4 userStatus-toggle" 
                   data-bs-toggle="modal" 
                   data-bs-target="${item.isActive === 'Active' ? '#confirmInactiveModal' : '#confirmActiveModal'}" 
                   data-user-id="${item.id}" 
                   data-isactive="${item.isActive}"
                   title="${item.isActive === 'Active' ? 'Deactivate' : 'Activate'}"
                   data-bs-placement="top">

                    <span class="${item.isActive === 'Active' ? 'fas fa-check-circle text-success' : 'fas fa-times-circle text-danger'}"></span>
                    ${item.isActive === 'Active' ? 'Active' : 'Inactive'}

                </a>
            `;
                    tableBody.append(`
                        <tr class="position-static">
                            
                            <td class="align-middle white-space-nowrap ps-3">${item.employeeCode}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.employeeName}</td>
                           
                            <td class="align-middle text-end">
                            <div class="row g-3">
                                <a class="btn btn-phoenix-secondary btn-icon me-1 fs-10 text-warning px-0 userList-single-reset"
                                    data-bs-toggle="modal" data-bs-target="#confirmResetModal"
                                    data-user-id="${item.id}" title="Reset Password" data-bs-placement="top">
                                    <span class="fas fa-key"></span>
                                </a>
                                ${statusButtonHtml}  <!-- Active/Inactive button with text -->
                                
                            </div>
                        </td>

                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            // Update pagination info
            var paginationInfo = response.paginationInfo;
            $("#userList-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);

            // Update pagination buttons
            updateUserListPagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        }
    });
}

// Handle the click event for user status change (active or inactive)
$(document).on('click', '.userStatus-toggle', function () {
    var userId = $(this).data('user-id'); // Get user id
    var userId2 = $(this).data('user-id'); 
    var isActive = $(this).data('isactive'); // Get isActive status from data attribute (string: 'Active' or 'Inactive')
    
    if (isActive === 'Inactive') {
        $('#confirmActivateBtn').data('user-id', userId); // Store user id for deactivation
       
    } else {
        // If user is inactive ('Inactive'), trigger the Deactivate Confirmation Modal
      
        $('#confirmDeactivateBtn').data('user-id', userId2); // Store user id for activation
        
    }
});


// Handle activation confirmation (Active User)
$('#confirmActivateBtn').click(function () {
    // Get the selected userId (you must set this before opening modal)
    var userId = $(this).data('user-id');
    if (!userId) {
        toastr.warning("User ID not found!");
        return;
    }

    // Call the API to activate the user (restore user)
    $.ajax({
        url: '/Account/RestoreUser/' + userId,
        type: 'PUT',  // Use PUT for restoration (you can use POST if needed in your API)
        contentType: 'application/json',
        success: function (response) {
            // Success message
            toastr.success(response.message);
            $('#confirmActiveModal').modal('hide');
            // Optional: Reload the table or update UI here
            loadUserTableData()
        },
        error: function (xhr) {
            // Error message
            toastr.error("Error: " + xhr.responseText);
        }
    });
});

// Handle deactivation confirmation (Inactive User)
$('#confirmDeactivateBtn').click(function () {
    // Get the selected userId (you must set this before opening modal)
    var userId = $(this).data('user-id');

    if (!userId) {
        toastr.warning("User ID not found!");
        return;
    }

    // Call the API to deactivate the user (delete user)
    $.ajax({
        url: '/Account/DeleteUser/' + userId,
        type: 'DELETE',  // Use DELETE for soft-deletion
        contentType: 'application/json',
        success: function (response) {
            // Success message
            toastr.success(response.message);
            $('#confirmInactiveModal').modal('hide');
            // Optional: Reload the table or update UI here
            loadUserTableData();
        },
        error: function (xhr) {
            // Error message
            toastr.error("Error: " + xhr.responseText);
        }
    });
});


function updateUserListPagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#paginationLinks");
    paginationLinks.empty();

    const windowSize = 1;

    const createPageButton = (page) => `
        <li class="page-item ${page === currentPage ? 'active' : ''}">
            <button class="page-link" onclick="goToUserPage(${page})">${page}</button>
        </li>
    `;
    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

    if (currentPage > windowSize + 1) {
        paginationLinks.append(createPageButton(1), addEllipsis());
    }

    const startPage = Math.max(1, currentPage - windowSize);
    const endPage = Math.min(totalPages, currentPage + windowSize);
    for (let i = startPage; i <= endPage; i++) {
        paginationLinks.append(createPageButton(i));
    }

    if (currentPage < totalPages - windowSize) {
        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
    }

    $("#userList-prevPageBtn").prop('disabled', currentPage === 1);
    $("#userList-nextPageBtn").prop('disabled', currentPage === totalPages);
}

function goToUserPage(page) {
    currentPage = page;
    loadUserTableData();
}
// When user clicks on Reset button
$(document).on('click', '.userList-single-reset', function () {
    var userId = $(this).data('user-id'); // Get user id
    $('#confirmResetBtn').data('user-id', userId); // Store user id in confirm button
});
// When Reset Confirm Button is clicked
$('#confirmResetBtn').click(function () {
    // Get the selected userId (you must set this before opening modal) 
    var userId = $(this).data('user-id');

    if (!userId) {
        toastr.warning("User ID not found!");
        return;
    }

    $.ajax({
        url: '/Account/ResetPassword',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(userId),
        success: function (response) {
            // Success message
            toastr.success(response.message);
            $('#confirmResetModal').modal('hide');
            // Optional: Reload the table or update UI here
        },
        error: function (xhr) {
            // Error message
            alert("Error: " + xhr.responseText);
        }
    });
});

$(document).on('click', '.userList-single-delete', function () {
    const userId = $(this).data('user-id');
    $('#confirmDeleteModal').modal('show'); // corrected from 'open' to 'show'

    // Optional: Bind a confirm delete button inside modal
    $('#confirmDeleteBtn').off('click').on('click', function () {
        deleteUser(userId);
    });
});

function deleteUser(userId) {
    $.ajax({
        url: `/Account/DeleteUser/${userId}`,
        type: 'DELETE',
        success: function (response) {
            toastr.success('✅ User deleted successfully!');
            $('#confirmDeleteModal').modal('hide');
            loadUserTableData();
        },
        error: function (xhr) {
            toastr.error('❌ Failed to delete user. Please try again.');
        }
    });
}



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                  Pagination End (User List)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

$('#export-pdf-btn').on('click', function () {
    const pageNumber = window.currentPageNumber || 1;
    const pageSize = $('#selectedPageSize').text().trim() || 5;
   // const searchTerm = $('#roleList-searchInput').val().trim();


    // Build export URL
    const url = `/UserList/ExportUsersToPdf?pageNumber=${pageNumber}&pageSize=${pageSize}`
        /*+ `&searchTerm=${encodeURIComponent(searchTerm)}`*/
        ;

    // Trigger download
    window.open(url, '_blank');
});

$('#exportToExcelButton').on('click', function () {
    const pageNumber = window.currentPageNumber || 1;
    const pageSize = $('#selectedPageSize').text().trim() || 5;
   // const searchTerm = $('#roleList-searchInput').val().trim();


    // Build export URL
    const url = `/UserList/ExportUsersToExcel?pageNumber=${pageNumber}&pageSize=${pageSize}`
       
        ;

    // Trigger download
    window.open(url, '_blank');
});