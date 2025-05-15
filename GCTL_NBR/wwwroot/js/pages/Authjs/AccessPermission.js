// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Document ready function for initializing event listeners when the page loads



$(document).ready(function () {

    $(document).on('submit', '.remove-user-form', function (event) {
        event.preventDefault(); // Prevent the default form submission

        var form = $(this);
        var roleName = form.find('input[name="roleName"]').val();
        var userName = form.find('input[name="userName"]').val();
        var token = $('input[name="__RequestVerificationToken"]').val(); // Get the Anti-Forgery Token
        // Confirm before deleting
        if (!confirm('Are you sure you want to remove this user from the role?')) {
            return;
        }

        // Perform the AJAX request
        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            headers: {
                'RequestVerificationToken': token // Add token to the headers
            },
            success: function (response) {
                if (response.success) {
                    // Remove the row from the table
                    form.closest('tr').remove();  // Remove the corresponding row
                    alert('User removed from role successfully!');
                } else {
                    alert('Error occurred while removing the user: ' + response.message);
                }
            },
            error: function () {
                alert('Error occurred while processing the request.');
            }
        });
    });



    $('.assign-role-btnR').on('click', function (e) {
        e.preventDefault();


        var row = $(this).closest('tr');

        // Collect data from the row
        var role = $(this).data('role');
        //var selectedRole = row.find('select[name="selectedRole"]').val();

        var selectedUsers = [];

        $(`#selectedUsers-${role} li`).each(function () {
            selectedUsers.push($(this).data('user-id'));
        });

        $.ajax({
            url: '/AccessPermission/AssignRole',
            type: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(), // Anti-forgery token
                role: role,
                selectedUsers: selectedUsers
            },
            success: function (response) {
                alert('Role assigned successfully.');
                location.reload();
            },
            error: function (xhr, status, error) {
                console.error(error);
                alert('An error occurred while assigning the role. Please try again.');
            }
        });
    });




    $(document).on('input', '.user-search', function () {
        const role = $(this).data('role');
        const searchQuery = $(this).val();
        const resultsList = $(`#userSearchResults-${role}`);

        // Clear previous results
        resultsList.empty();

        if (searchQuery.length < 2) return; // Search after 2 characters

        // Fetch users via AJAX
        $.ajax({
            url: '/AccessPermission/SearchUsers',
            type: 'GET',
            data: { query: searchQuery },
            success: function (data) {
                data.forEach(user => {
                    const listItem = `<li class="user-result" data-user-id="${user.id}" data-role="${role}">
                                        <span>${user.userName}</span> 
                                        <button class="btn btn-link add-user-btn" data-user-id="${user.id}" data-role="${role}">Add</button>
                                      </li>`;
                    resultsList.append(listItem);
                });
            },
            error: function () {
                alert('Error fetching users. Please try again.');
            }
        });
    });
    $(document).on('click', '.add-user-btn', function () {
        const userId = $(this).data('user-id');
        const role = $(this).data('role');
        const selectedUsersList = $(`#selectedUsers-${role}`);
        const userName = $(this).closest('li').find('span').text();

        // Add user to selected list
        selectedUsersList.append(`<li data-user-id="${userId}">${userName} 
                                    <button class="btn btn-link remove-user-btn" data-user-id="${userId}" data-role="${role}">Remove</button>
                                  </li>`);

        // Remove from search results
        $(this).closest('li').remove();
    });
    $(document).on('click', '.remove-user-btn', function () {
        $(this).closest('li').remove();
    });
  
      
  
});















