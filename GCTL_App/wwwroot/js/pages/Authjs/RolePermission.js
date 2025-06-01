
$(document).ready(function () {
    console.log("Permission script loaded! ✅");
    function updateSubmitButtonState() {
        if (updatedPermissions.length > 0) {
            $('#savePermissions').prop('disabled', false);
        } else {
            $('#savePermissions').prop('disabled', true);
        }
    }

    let updatedPermissions = [];

    function updatePermissionArray(menuTabId, permissionType, isGranted) {
        // Step 1: Remove any previous item that has the same menuTabId + permission + isGranted
        updatedPermissions = updatedPermissions.filter(p =>
            !(p.menuTabId === menuTabId && p.permission === permissionType && p.isGranted === isGranted)
        );
        const original = $(`.permission-toggle[data-module="${menuTabId}"][data-permission="${permissionType}"]`).prop("defaultChecked");
        if (isGranted === original) {
            updatedPermissions = updatedPermissions.filter(p =>
                !(p.menuTabId === menuTabId && p.permission === permissionType)
            );
            updateSubmitButtonState();
            return;
        }

        // Step 2: Push this new one (latest change)
        updatedPermissions.push({ menuTabId, permission: permissionType, isGranted });
        updateSubmitButtonState(); // ✅ Update after adding
        console.log("Updated Permissions:", updatedPermissions);
    }






    function checkAllChildrenRecursive(moduleId, permissionType, isChecked) {
        const selector = permissionType
            ? `.permission-toggle[data-parent="${moduleId}"][data-permission="${permissionType}"]`
            : `.permission-toggle[data-parent="${moduleId}"]`;

        const children = $(selector);
        children.each(function () {
            $(this).prop("checked", isChecked);
            updatePermissionArray($(this).data("module"), $(this).data("permission"), isChecked);
            checkAllChildrenRecursive($(this).data("module"), $(this).data("permission"), isChecked);
        });
    }

    function syncParentRecursive(moduleId, permissionType) {
        const parent = $(`.permission-toggle[data-module="${moduleId}"]`).data("parent");
        if (!parent) return;

        const siblings = $(`.permission-toggle[data-parent="${parent}"][data-permission="${permissionType}"]`);
        const allChecked = siblings.length === siblings.filter(":checked").length;

        const parentCheckbox = $(`.permission-toggle[data-module="${parent}"][data-permission="${permissionType}"]`);
        parentCheckbox.prop("checked", allChecked);
        updatePermissionArray(parent, permissionType, allChecked);

        if (siblings.filter(":checked").length > 0) {
            parentCheckbox.prop("checked", true);
            updatePermissionArray(parent, permissionType, true);
        }

        syncParentRecursive(parent, permissionType);
    }

    function toggleModuleAndChildren(targetClass) {
        const $rows = $("." + targetClass);
        const isVisible = $rows.is(":visible");

        $rows.each(function () {
            const $row = $(this);

            if (isVisible) {
                $row.slideUp("fast");
                const nestedIcons = $row.find(".module-dropdown-indicator");
                nestedIcons.each(function () {
                    const nestedTarget = $(this).data("target");
                    $("." + nestedTarget).slideUp("fast");
                    $(this).removeClass("fa-caret-down").addClass("fa-caret-right");
                });
            } else {
                $row.slideDown("fast");
            }
        });

        const mainIcon = $(`.module-dropdown-indicator[data-target="${targetClass}"]`);
        if (isVisible) {
            mainIcon.removeClass("fa-caret-down").addClass("fa-caret-right");
        } else {
            mainIcon.removeClass("fa-caret-right").addClass("fa-caret-down");
        }
    }

    function loadPermissions(roleId) {
        $.ajax({
            url: '/RolePermission/LoadPermissions',
            type: 'GET',
            data: { roleId: roleId },
            success: function (response) {
                $('#permissionTableContainer').html(response);
                bindPermissionEvents();
            },
            error: function () {
                toastr.error('Error loading permissions. Please try again.');
            }
        });
    }

    function bindPermissionEvents() {
        $(".link-module").on("click", function () {
            const targetClass = $(this).data("target");
            toggleModuleAndChildren(targetClass);
        });

        $(".permission-toggle").on("change", function () {
            const moduleId = $(this).data("module");
            const permissionType = $(this).data("permission");
            const isChecked = $(this).is(":checked");

            updatePermissionArray(moduleId, permissionType, isChecked);
            checkAllChildrenRecursive(moduleId, permissionType, isChecked);
            syncParentRecursive(moduleId, permissionType);
        });

        $(".select-all").on("change", function () {
            let isChecked = $(this).is(":checked");
            $(".permission-toggle").each(function () {
                $(this).prop("checked", isChecked);
                updatePermissionArray($(this).data("module"), $(this).data("permission"), isChecked);
            });
        });

        $(".select-row").on("change", function () {
            let moduleId = $(this).data("module");
            let isChecked = $(this).is(":checked");

            $(`.permission-toggle[data-module="${moduleId}"]`).each(function () {
                $(this).prop("checked", isChecked).trigger("change");
            });

            checkAllChildrenRecursive(moduleId, null, isChecked);

            const permissions = [...new Set($(`.permission-toggle[data-module="${moduleId}"]`).map(function () {
                return $(this).data("permission");
            }).get())];

            permissions.forEach(p => syncParentRecursive(moduleId, p));
        });
    }

    $('#roleSelect2').on('change', function () {
        const roleId = $(this).val();
        loadPermissions(roleId);
    });

    $('#roleSelect2').trigger('change');
    $(document).on("click", "#savePermissions", function () {
        if (updatedPermissions.length === 0) {
            toastr.warning("No changes to save.");
            return;
        }
  

        // Show the confirmation modal before saving the changes
        $('#confirmPermissionChangeModal').modal('show');

        // When the user clicks "Yes, Save Changes"
        $('#confirmChangePermissionsBtn').on('click', function () {
            // Proceed with the AJAX call to save the permissions
            $.ajax({
                url: "/RolePermission/UpdatePermissions",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    roleId: $("#roleSelect2").val(),
                    permissions: updatedPermissions
                }),
                success: function () {
                    toastr.success("Permissions updated successfully!");
                    updatedPermissions = [];
                    updateSubmitButtonState(); // ✅ Reset button state
                    // Hide the confirmation modal after success
                    $('#confirmPermissionChangeModal').modal('hide');
                    // Reload the page to reflect the changes
                    location.reload();
                },
                error: function (xhr, status, error) {
                    toastr.warning("Failed to update permissions.");
                    console.error("Error:", error);
                    // Hide the confirmation modal after error
                    $('#confirmPermissionChangeModal').modal('hide');
                }
            });
        });

        // If the user clicks "Cancel", hide the confirmation modal
        $(".btn-light").on('click', function () {
            $('#confirmPermissionChangeModal').modal('hide');
        });
    });

});


function toggleModuleAndChildren(targetClass) {
    const $rows = $("." + targetClass);
    const isVisible = $rows.is(":visible");

    $rows.each(function () {
        const $row = $(this);

        if (isVisible) {
            // Collapse: hide self AND all nested children AND reset carets
            $row.slideUp("fast");

            // Find all nested children from this row
            const nestedIcons = $row.find(".module-dropdown-indicator");
            nestedIcons.each(function () {
                const nestedTarget = $(this).data("target");
                $("." + nestedTarget).slideUp("fast");

                // Reset all nested carets to right
                $(this).removeClass("fa-caret-down").addClass("fa-caret-right");
            });
        } else {
            // Expand: ONLY show immediate children (not nested)
            $row.slideDown("fast");
        }
    });

    // Toggle the main caret
    const mainIcon = $(`.module-dropdown-indicator[data-target="${targetClass}"]`);
    if (isVisible) {
        mainIcon.removeClass("fa-caret-down").addClass("fa-caret-right");
    } else {
        mainIcon.removeClass("fa-caret-right").addClass("fa-caret-down");
    }
}



