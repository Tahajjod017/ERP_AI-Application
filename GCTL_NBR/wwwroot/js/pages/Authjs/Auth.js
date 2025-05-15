// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Document ready function for initializing event listeners when the page loads






$(document).ready(function () {

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


    // testing
    $('#roleSelect2').on('change', function () {
        const roleId = $(this).val();


        $.ajax({
            url: '/RolePermission/LoadPermissions',
            type: 'GET',
            data: { roleId: roleId },
            success: function (response) {
                $('#permissionTableContainer').html(response); // Replace the table content
                $(".link-module").on("click", function () {
                    const targetClass = $(this).data("target");
                    $("." + targetClass).slideToggle("fast");
                });
                $('.permission-toggle').on('change', function () {

                    const moduleId = $(this).data('module');
                    const permission = $(this).data('permission');
                    const isGranted = $(this).is(':checked');


                    updatePermission(roleId, moduleId, permission, isGranted);
                });
            },
            error: function () {
                alert('Error loading permissions. Please try again.');
            }
        });
    });


    $('#roleSelect2').trigger('change');




    function updatePermission(roleId, moduleId, permission, isGranted) {
        $.ajax({
            url: '/RolePermission/UpdatePermission',
            type: 'POST',
            contentType: 'application/json',

            data: JSON.stringify({
                roleId: roleId,
                moduleId: moduleId,
                permission: permission,
                isGranted: isGranted
            }),
            success: function () {
                // alert("Permission updated successfully");
            },
            error: function (xhr, status, error) {
                console.error("Error updating permission:", error);
                alert("Error updating permission");
            }
        });
    }


    // for superAdmin tenantModule
    //const moduleTree = $("#module-tree");

    //// Handle checkbox hierarchy
    //moduleTree.on("change", "input[type='checkbox']", function () {
    //    const $checkbox = $(this);

    //    if ($checkbox.hasClass("primary-checkbox")) {
    //        toggleChildren($checkbox, "secondary-checkbox");
    //    } else if ($checkbox.hasClass("secondary-checkbox")) {
    //        toggleChildren($checkbox, "tertiary-checkbox");
    //        toggleParent($checkbox, "primary-checkbox");
    //    } else if ($checkbox.hasClass("tertiary-checkbox")) {
    //        toggleParent($checkbox, "secondary-checkbox");
    //    }
    //});

    //// Handle link-based toggle for visibility
    //moduleTree.on("click", ".toggle-module", function (event) {
    //    const $link = $(this);
    //    const targetClass = $link.data("target");
    //    const $childGroup = $(`.${targetClass}`);

    //    if ($childGroup.length > 0) {
    //        $childGroup.slideToggle();
    //    }
    //});

    //function toggleChildren($parentCheckbox, childClass) {
    //    const parentId = $parentCheckbox.val();
    //    const $children = $(`input.${childClass}[data-parent-id='${parentId}']`);

    //    $children.prop("checked", $parentCheckbox.prop("checked"));
    //    // Recursively toggle grand-children
    //    if (childClass === "secondary-checkbox") {
    //        $children.each(function () {
    //            toggleChildren($(this), "tertiary-checkbox");
    //        });
    //    }
    //}

    //function toggleParent($childCheckbox, parentClass) {
    //    const parentId = $childCheckbox.data("parent-id");
    //    if (!parentId) return;

    //    const $parentCheckbox = $(`input.${parentClass}[value='${parentId}']`);
    //    const $siblings = $(`input[data-parent-id='${parentId}']`);

    //    // Check parent only if all siblings are checked
    //    const allChecked = $siblings.length > 0 && $siblings.filter(":checked").length === $siblings.length;
    //    $parentCheckbox.prop("checked", allChecked);
    //}

    //const moduleTree = $("#module-tree");

    //// Handle checkbox hierarchy
    //moduleTree.on("change", "input[type='checkbox']", function () {

    //    const $checkbox = $(this);

    //    if ($checkbox.hasClass("primary-checkbox")) {
    //        // Get the primary module ID
    //        const primaryModuleId = $checkbox.data("module");

    //        // Toggle all secondary checkboxes under this primary module
    //        toggleChildren($checkbox, "secondary-checkbox", "data-parent", primaryModuleId);

    //        // Toggle all tertiary checkboxes under this primary module
    //        $(`.secondary-checkbox[data-parent='${primaryModuleId}']`).each(function () {
    //            const secondaryModuleId = $(this).data("module");
    //            console.log(secondaryModuleId);
    //            toggleChildren($(secondaryModuleId), "tertiary-checkbox", "data-parent", secondaryModuleId);
    //        });
    //        if ($checkbox.hasClass("tertiary-checkbox")) {
    //            // Update the secondary checkbox based on the state of all tertiary checkboxes
    //            toggleParent($checkbox, "secondary-checkbox", "data-module", $checkbox.data("parent"));
    //        }

    //    } else if ($checkbox.hasClass("secondary-checkbox")) {
    //        // Toggle all tertiary checkboxes under this secondary module
    //        const secondaryModuleId = $checkbox.data("module");
    //        toggleChildren($checkbox, "tertiary-checkbox", "data-parent", secondaryModuleId);

    //        // Update the primary checkbox based on the state of all secondary checkboxes
    //        toggleParent($checkbox, "primary-checkbox", "data-module", $checkbox.data("parent"));
    //    } else if ($checkbox.hasClass("tertiary-checkbox")) {
    //        // Update the secondary checkbox based on the state of all tertiary checkboxes
    //        toggleParent($checkbox, "secondary-checkbox", "data-module", $checkbox.data("parent"));
    //    }
    //});

    //// Function to toggle child checkboxes
    //function toggleChildren($parentCheckbox, childClass, parentAttribute, parentId) {
    //    const $children = $(`input.${childClass}[${parentAttribute}='${parentId}']`);
    //    $children.prop("checked", $parentCheckbox.prop("checked"));
    //}

    //// Function to update parent checkboxes
    //function toggleParent($childCheckbox, parentClass, parentAttribute, parentId) {
    //    const $parentCheckbox = $(`input.${parentClass}[${parentAttribute}='${parentId}']`);
    //    const $siblings = $(`input[${parentAttribute}='${parentId}']`);

    //    // Check parent only if all siblings are checked
    //    const allChecked = $siblings.length > 0 && $siblings.filter(":checked").length === $siblings.length;
    //    $parentCheckbox.prop("checked", allChecked);
    //}

    // Handle form submission
    $("#assignModulesForm").on("submit", function (event) {
        event.preventDefault();


        const moduleIds = $("#primaryModulesList input:checked")
            .map(function () {
                return $(this).val();
            })
            .get();

        $.ajax({
            url: '/SuperAdmin/AssignModules',
            method: 'POST',
            data: {

                moduleIds: moduleIds
            },
            success: function (response) {
                alert("Modules assigned successfully!");
                $("#assignModulesModal").modal("hide");
                location.reload();
            },
            error: function () {
                alert("An error occurred while assigning modules.");
            }
        });
    });
});
$(document).on('change', '.company-dropdown', function () {
    const companyId = $(this).val(); // Get the selected company ID
    const row = $(this).closest('tr'); // Get the row containing the changed dropdown
    const branchDropdown = row.find('.branch-dropdown'); // Find the branch dropdown in the same row

    // Clear and reset the branch dropdown
    branchDropdown.empty();
    branchDropdown.append('<option value="">-- Select Branch --</option>');

    if (!companyId) {
        return;
    }


    ranchId = row.find('select[name="branchId"]').val();


    //    $.ajax({
    //        url: '/Account/AssignRole', 
    //        type: 'POST',
    //        data: {
    //            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(), // Anti-forgery token
    //            userId: userId,
    //            selectedRole: selectedRole,
    //            companyId: companyId,
    //            branchId: branchId
    //        },
    //        success: function (response) {
    //            alert('Role assigned successfully.');
    //            location.reload(); 
    //        },
    //        error: function (xhr, status, error) {
    //            console.error(error);
    //            alert('An error occurred while assigning the role. Please try again.');
    //        }
    //    });
    //});


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

$(document).ready(function () {
    console.log("Permission script loaded! ✅");

    // When any permission checkbox (View, Edit, Create, Delete) in a Primary Module is clicked
    $(document).on("change", ".primary-checkbox", function () {
        let moduleId = $(this).data("module");
        let permissionType = $(this).data("permission");
        let isChecked = $(this).is(":checked");

        console.log(`Primary ${permissionType} changed:`, moduleId, "Checked:", isChecked);

        // Select/deselect all Secondary Modules for the same permission
        $(`.secondary-checkbox[data-parent="${moduleId}"][data-permission="${permissionType}"]`)
            .prop("checked", isChecked)
            .trigger("change"); // Triggers secondary selection for Tertiary Modules
    });

    // When any permission checkbox in a Secondary Module is clicked
    $(document).on("change", ".secondary-checkbox", function () {
        let moduleId = $(this).data("module");
        let parentId = $(this).data("parent");
        let permissionType = $(this).data("permission");
        let isChecked = $(this).is(":checked");

        console.log(`Secondary ${permissionType} changed:`, moduleId, "Checked:", isChecked);

        // Select/deselect all Tertiary Modules for the same permission
        $(`.tertiary-checkbox[data-parent="${moduleId}"][data-permission="${permissionType}"]`).prop("checked", isChecked);

        // If all Secondary Modules have this permission, check the Primary Module
        let allChecked = $(`.secondary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]`).length ===
            $(`.secondary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]:checked`).length;
        $(`.primary-checkbox[data-module="${parentId}"][data-permission="${permissionType}"]`).prop("checked", allChecked);
    });

    // When any permission checkbox in a Tertiary Module is clicked
    $(document).on("change", ".tertiary-checkbox", function () {
        let parentId = $(this).data("parent");
        let permissionType = $(this).data("permission");

        console.log(`Tertiary ${permissionType} changed for parent:`, parentId);

        // If all Tertiary Modules have this permission, check the Secondary Module
        let allChecked = $(`.tertiary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]`).length ===
            $(`.tertiary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]:checked`).length;
        $(`.secondary-checkbox[data-module="${parentId}"][data-permission="${permissionType}"]`).prop("checked", allChecked);
    });

});













