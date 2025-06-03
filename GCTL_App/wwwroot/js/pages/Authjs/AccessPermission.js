// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Document ready function for initializing event listeners when the page loads



$(document).ready(function () {
    let roleNameToDelete = null;
    let roleIdToDelete = null;

    $(document).on('click', '.open-confirm-delete-modal', function () {
        roleNameToDelete = $(this).data('role-name');
        roleIdToDelete = $(this).data('role-id');
        $('#confirmDeleteModal .modal-title').text(`Are you sure you want to delete "${roleNameToDelete}"?`);
        $('#confirmDeleteModal').modal('show');
    });

    $('#confirmDeleteBtn').on('click', function () {
        if (!roleNameToDelete) return;

        $.ajax({
            url: '/AccessPermission/DeleteRoleById', // Backend endpoint for role deletion by name
            type: 'POST',
            data: { roleId: roleIdToDelete },
            success: function () {
                toastr.success(`Role "${roleNameToDelete}" deleted successfully.`);
                $(`tr[data-role-name="${roleNameToDelete}"]`).remove();
                $('#confirmDeleteModal').modal('hide');
            },
            error: function () {
                toastr.error("Failed to delete the role.");
            }
        });
    });

    let formToRemove = null;

    $(document).on('submit', '.remove-user-form', function (event) {
        event.preventDefault();
        formToRemove = $(this); // store form for later use
        $('#confirmRemoveUserModal').modal('show'); // show modal
    });

    $(document).on('click', '#confirmRemoveBtn', function () {
        if (!formToRemove) return;

        const roleName = formToRemove.find('input[name="roleName"]').val();
        const userName = formToRemove.find('input[name="userName"]').val();
        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: formToRemove.attr('action'),
            type: 'POST',
            data: formToRemove.serialize(),
            headers: {
                'RequestVerificationToken': token
            },
            success: function (response) {
                if (response.success) {
                    formToRemove.closest('tr').remove();
                    toastr.warning('User removed from role successfully!');
                } else {
                    toastr.error('Error occurred: ' + response.message);
                }
            },
            error: function () {
                toastr.error('Error occurred while processing the request.');
            },
            complete: function () {
                $('#confirmRemoveUserModal').modal('hide'); // hide modal after request
                formToRemove = null; // reset
            }
        });
    });

    $(document).on('click', function (event) {
        const $target = $(event.target);

        // Check if click is outside both the input and the results container
        if (
            !$target.closest('.user-search').length &&
            !$target.closest('.user-results-scroll').length
        ) {
            $('.user-results-scroll').hide(); // Hide all result boxes
        }
    });




    $(document).on("click", ".assign-role-btnR", function (e) {
        e.preventDefault();

        var role = $(this).data('role');
        const roleId = $(this).data('role-id'); 
        var selectedUsers = [];

        // Collect user IDs from the selected list
        $(`#selectedUsers-${role} .user-item`).each(function () {
            selectedUsers.push($(this).data('user-id'));
        });

        // Store role and user IDs in the modal's data for later use
        $('#confirmNewRoleModal').data('roleId', roleId);
        $('#confirmNewRoleModal').data('role', role);
        $('#confirmNewRoleModal').data('users', selectedUsers);

        // Show the confirmation modal
        $('#confirmNewRoleModal').modal('show');
    });

    // Handle the confirmation click inside the modal
    $(document).on("click", "#confirmNewRoleBtn", function () {
        const modal = $('#confirmNewRoleModal');
        const role = modal.data('role');
        const roleId = modal.data('roleId');
        const selectedUsers = modal.data('users');

        assignRole(roleId, role, selectedUsers);
        modal.modal('hide');
    });



    function assignRole(roleId,role, selectedUsers) {
        $.ajax({
            url: '/AccessPermission/AssignRole',
            type: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                roleId: roleId,
                role: role,
                selectedUsers: selectedUsers
            },
            success: function () {
                toastr.success('Role assigned successfully.');
                location.reload();
            },
            error: function (xhr) {
                if (xhr.status === 403) {
                    toastr.error('Access Denied');
                } else {
                    toastr.error('Something went wrong');
                }
               // toastr.error('An error occurred while assigning the role.');

            }
        });
    }

    $(document).ready(function () {

        let previousQuery = "";  // Store the last search query
        let previousResults = [];  // Store the last search results

        $(document).on('input', '.user-search', function () {
            const searchQuery = $(this).val();
            const role = $(this).data('role');
            const resultsList = $(`#userSearchResults-${role}`);
            const scrollContainer = resultsList.closest('.user-results-scroll');

            // If search term is the same as previous, no need to fetch again
            if (searchQuery === previousQuery) {
                return;
            }

            previousQuery = searchQuery; // Update the last query

            // Clear previous results
            resultsList.empty();
            scrollContainer.hide(); // Hide the dropdown initially

            if (searchQuery.length < 2) return; // Avoid searching if the query is too short

            // Show loading spinner or message if you want
            // You can add a "loading..." text or spinner here
   
            $.ajax({
                url: '/AccessPermission/SearchUsers',  // Ensure this URL is correct
                type: 'GET',
                data: { query: searchQuery },
                success: function (data) {
                    console.log(data);  // Check the data in the console
                    previousResults = data; // Store the new results

                    resultsList.empty();  // Clear existing results before appending new ones
                    if (data.length === 0) {
                        resultsList.append(`<li class="text-muted fst-italic">No users found.</li>`);
                        scrollContainer.show(); // Show the dropdown with "No results"
                        return;
                    }
                  
                    // Show only 5 results at a time
                    const limitedResults = data.slice(0, 5);

                    // Use requestAnimationFrame for smoother updates
                    requestAnimationFrame(() => {
                        limitedResults.forEach(user => {
                            const listItem = `
                            <li class="user-result d-flex justify-content-between align-items-center border rounded px-3 py-2 mb-2 shadow-sm"
                                data-user-id="${user.id}" data-role="${role}">
                                <div class="d-flex align-items-center">
                                    <span class="fw-semibold text-dark me-2">
                                        <strong>${user.employeeCode.split(' - ')[0]}</strong> 
                                        ${user.userName.split(' - ')[0]}
                                    </span>
                                </div>
                                <button class="btn btn-sm btn-outline-primary add-user-btn ms-2" 
                                    data-user-id="${user.id}" data-employee-code="${user.employeeCode}" 
                                    data-user-name="${user.userName}" data-role="${role}">Add</button>
                            </li>`;
                            resultsList.append(listItem); // Append the result to the dropdown
                        });
                        scrollContainer.show(); // Show the dropdown with the results
                    });

                },
                error: function () {
                    console.log("Error fetching data."); // Log any errors
                    resultsList.append(`<li class="text-danger">Error fetching users. Please try again.</li>`);
                }
            });
        });

        $(document).on('click', function (event) {
            const $target = $(event.target);

            // Check if click is outside both the input and the results container
            if (
                !$target.closest('.user-search').length &&
                !$target.closest('.user-results-scroll').length
            ) {
                $('.user-results-scroll').hide(); // Hide the dropdown if clicked outside
            }
        });

        $(document).on('focus', '.user-search', function () {
            const role = $(this).data('role');
            const resultsList = $(`#userSearchResults-${role}`);
            const scrollContainer = resultsList.closest('.user-results-scroll');

            // Show previous results if any
            if (previousResults.length > 0) {
                requestAnimationFrame(() => {
                    scrollContainer.show(); // Display results container when focused
                });
            }
        });
    });





    $(document).on('click', '.add-user-btn', function () {
        const userId = $(this).data('user-id');
        const role = $(this).data('role');
        const selectedUsersList = $(`#selectedUsers-${role}`);
        const userName = $(this).closest('li').find('span').text();

        // Prevent adding the same user again
        if (selectedUsersList.find(`[data-user-id="${userId}"]`).length > 0) {
            toastr.warning('This user is already selected.');
            return;
        }

        // Check if the selected users list already has 20 users
        if (selectedUsersList.children().length >= 20) {
            toastr.warning('You can only select up to 20 users.');
            return;
        }

        // Check if the user already has a role
        $.ajax({
            url: '/AccessPermission/CheckUserRole',  // Your endpoint to check user role
            type: 'GET',
            data: { userId: userId },  // Pass the user ID to check if they have a role
            success: function (response) {
                if (!response.found) {
                    // Show modal: No user account found
                    $('#createUserModal').modal('show');

                    $('#confirmCreateUserBtn').off('click').on('click', function () {
                        // Proceed to create user logic
                        createUserForEmployee(userId);
                        $('#createUserModal').modal('hide');
                    });

                } else if (!response.hasEmail) {
                    // Show modal: User has no email
                    $('#missingEmailModal').modal('show');
                } else if (response.hasRole) {
                    // Show confirmation to replace role
                    $('#confirmSaveModal').modal('show');

                    $('#confirmSaveBtn').off('click').on('click', function () {
                        assignUserToRole(userId, userName, selectedUsersList);
                        $('#confirmSaveModal').modal('hide');
                    });

                } else {
                    assignUserToRole(userId, userName, selectedUsersList);
                }
            },
            error: function () {
                toastr.error('Error checking user role.');
            }
        });
    });

    // Function to assign the user to the selected list
    function assignUserToRole(userId, userName, selectedUsersList) {
       

        // Add user to selected list with "X" icon for removal (no box, just text)
        selectedUsersList.append(`
        <span class="user-item" data-user-id="${userId}">
            ${userName} 
            <i class="fas fa-times remove-user-btn" data-user-id="${userId}" style="cursor: pointer; color: #dc3545;"></i>
        </span> 
        &nbsp;&nbsp;
    `);

        // Apply smooth transition after a small delay for better UX
        requestAnimationFrame(() => {
            const userItem = selectedUsersList.find(`.user-item[data-user-id="${userId}"]`);
            userItem.addClass('added');  // Add smooth transition class
        });

        // Remove from search results
        $(this).closest('li').remove();
        updateScroll();
        updateAssignButtonState(selectedUsersList);
    }
    function createUserForEmployee(userId) {
        const companyId = $('#SelectedCompanyId').val();
        const tenantId = $('#SelectedTenantId').val();
        $.ajax({
            url: '/AccessPermission/CreateUserForEmployee',
            type: 'POST',
            data: {
                employeeId: userId,
                companyId: companyId,
                tenantId: tenantId
            },
            success: function (response) {
                if (response.success) {
                    toastr.success('User account created successfully.');
                    // Optionally, you can reload or re-check the role
                    // location.reload(); 
                } else {
                    toastr.error(response.message || 'Failed to create user.');
                }
            },
            error: function () {
                toastr.error('Error creating user account.');
            }
        });
    }

    // Update the scroll area if there are more than 3 rows
    function updateScroll() {
        const selectedUsersList = $('.user-results-scroll');
        const totalRows = selectedUsersList.find('.user-item').length;

        if (totalRows > 3) {
            selectedUsersList.css('max-height', '240px').css('overflow-y', 'auto');
        } else {
            selectedUsersList.css('max-height', 'none').css('overflow-y', 'hidden');
        }
    }

    $(document).on('click', '.remove-user-btn', function () {
        const selectedUsersList = $(this).closest('.list-unstyled');
        $(this).closest('.user-item').remove();
        updateScroll();
        updateAssignButtonState(selectedUsersList);
    });

    function updateAssignButtonState(selectedUsersList) {
        const role = selectedUsersList.attr('id').split('-')[1];
        const assignBtn = $(`.assign-role-btnR[data-role="${role}"]`);
        const userCount = selectedUsersList.find('.user-item').length;

        assignBtn.prop('disabled', userCount === 0);
    }


    $(document).on('click', '.create-user-btn', function () {
        const button = $(this);
        const employeeCode = button.data('employee-code');
        const userName = button.data('user-name');
        const listItem = button.closest('li');
        const role = listItem.data('role');
        const userId = listItem.data('user-id');

        if (!confirm(`Create a user account for "${userName}"?`)) return;

        $.ajax({
            url: '/Account/CreateUsers',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify([{ employeeCode: employeeCode }]),
            success: function (response) {
                // ✅ Optionally get updated name from response if provided
                const updatedName = response.userName || userName;

                // Update name and badge in left area
                const userDisplay = listItem.find('div:first');
                userDisplay.html(`
                <span class="fw-semibold text-dark me-2">${updatedName}</span>
                <span class="badge bg-success">HasUser</span>
                
            `);

                // Update button area
                const successHtml = `
                <span class="text-success me-2"><i class="fas fa-check-circle"></i></span>
                <button class="btn btn-sm btn-outline-primary add-user-btn ms-2"
                        data-user-id="${userId}" data-role="${role}">Add</button>`;

                button.parent().html(successHtml);

                toastr.success(response.message || 'User created successfully.');
            },
            error: function (xhr) {
                toastr.error('Failed to create user. ' + (xhr.responseJSON?.message || ''));
            }
        });
    });




});


















