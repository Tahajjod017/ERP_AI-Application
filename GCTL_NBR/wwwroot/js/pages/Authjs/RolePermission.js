
$(document).ready(function () {
    console.log("Permission script loaded! ✅");

    let updatedPermissions = [];

    function updatePermissionArray(menuTabId, permissionType, isGranted) {
        let index = updatedPermissions.findIndex(p => p.menuTabId === menuTabId && p.permission === permissionType);

        if (index > -1) {
            updatedPermissions[index].isGranted = isGranted;
        } else {
            updatedPermissions.push({ menuTabId, permission: permissionType, isGranted });
        }

        console.log("Pending Updates:", updatedPermissions);
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
                alert('Error loading permissions. Please try again.');
            }
        });
    }

    function bindPermissionEvents() {
        // Dropdown expand/collapse for all module levels
        $(".link-module").on("click", function () {
            const targetClass = $(this).data("target");
           
            toggleModuleAndChildren(targetClass);
        });

        // Individual permission checkbox change
        $('.permission-toggle').on('change', function () {
            const menuTabId = $(this).data('module');
            const permissionType = $(this).data('permission');
            const isChecked = $(this).is(":checked");
            updatePermissionArray(menuTabId, permissionType, isChecked);
        });

        // Select All toggle
        $(".select-all").on("change", function () {
            let isChecked = $(this).is(":checked");
            $(".select-row, .permission-toggle").prop("checked", isChecked).trigger("change");
        });

        // Select Row (bulk select for that row's module)
        $(".select-row").on("change", function () {
            let moduleId = $(this).data("module");
            let isChecked = $(this).is(":checked");

            $(`.permission-toggle[data-module="${moduleId}"]`).prop("checked", isChecked).trigger("change");

            // Check for any children and select recursively
            $(`.permission-toggle[data-parent="${moduleId}"]`).each(function () {
                $(this).prop("checked", isChecked).trigger("change");
            });

            // Update 'Select All' status
            let allChecked = $(".select-row").length === $(".select-row:checked").length;
            $(".select-all").prop("checked", allChecked);
        });

        // Primary permission cascade
        $(".primary-checkbox").on("change", function () {
            let moduleId = $(this).data("module");
            let permissionType = $(this).data("permission");
            let isChecked = $(this).is(":checked");

            $(`.secondary-checkbox[data-parent="${moduleId}"][data-permission="${permissionType}"]`).each(function () {
                let secId = $(this).data("module");
                $(this).prop("checked", isChecked).trigger("change");

                $(`.tertiary-checkbox[data-parent="${secId}"][data-permission="${permissionType}"]`).each(function () {
                    let terId = $(this).data("module");
                    $(this).prop("checked", isChecked).trigger("change");

                    $(`.quaternary-checkbox[data-parent="${terId}"][data-permission="${permissionType}"]`).each(function () {
                        let l4Id = $(this).data("module");
                        $(this).prop("checked", isChecked).trigger("change");

                        $(`.quinary-checkbox[data-parent="${l4Id}"][data-permission="${permissionType}"]`)
                            .prop("checked", isChecked).trigger("change");
                    });
                });
            });

            updatePermissionArray(moduleId, permissionType, isChecked);
        });

        // Secondary permission cascade
        $(".secondary-checkbox").on("change", function () {
            let moduleId = $(this).data("module");
            let parentId = $(this).data("parent");
            let permissionType = $(this).data("permission");
            let isChecked = $(this).is(":checked");

            $(`.tertiary-checkbox[data-parent="${moduleId}"][data-permission="${permissionType}"]`).each(function () {
                let terId = $(this).data("module");
                $(this).prop("checked", isChecked).trigger("change");

                $(`.quaternary-checkbox[data-parent="${terId}"][data-permission="${permissionType}"]`).each(function () {
                    let l4Id = $(this).data("module");
                    $(this).prop("checked", isChecked).trigger("change");

                    $(`.quinary-checkbox[data-parent="${l4Id}"][data-permission="${permissionType}"]`)
                        .prop("checked", isChecked).trigger("change");
                });
            });

            // Update parent primary checkbox
            let allChecked = $(`.secondary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]`).length ===
                $(`.secondary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]:checked`).length;

            $(`.primary-checkbox[data-module="${parentId}"][data-permission="${permissionType}"]`).prop("checked", allChecked);

            updatePermissionArray(moduleId, permissionType, isChecked);
        });

        // Tertiary permission cascade
        $(".tertiary-checkbox").on("change", function () {
            let moduleId = $(this).data("module");
            let parentId = $(this).data("parent");
            let permissionType = $(this).data("permission");
            let isChecked = $(this).is(":checked");

            $(`.quaternary-checkbox[data-parent="${moduleId}"][data-permission="${permissionType}"]`).each(function () {
                let l4Id = $(this).data("module");
                $(this).prop("checked", isChecked).trigger("change");

                $(`.quinary-checkbox[data-parent="${l4Id}"][data-permission="${permissionType}"]`)
                    .prop("checked", isChecked).trigger("change");
            });

            // Update parent secondary
            let allChecked = $(`.tertiary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]`).length ===
                $(`.tertiary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]:checked`).length;

            $(`.secondary-checkbox[data-module="${parentId}"][data-permission="${permissionType}"]`).prop("checked", allChecked);

            // Check grandparent primary
            let grandParentId = $(`.secondary-checkbox[data-module="${parentId}"]`).data("parent");

            let allSecondaryChecked = $(`.secondary-checkbox[data-parent="${grandParentId}"][data-permission="${permissionType}"]`).length ===
                $(`.secondary-checkbox[data-parent="${grandParentId}"][data-permission="${permissionType}"]:checked`).length;

            $(`.primary-checkbox[data-module="${grandParentId}"][data-permission="${permissionType}"]`).prop("checked", allSecondaryChecked);

            updatePermissionArray(moduleId, permissionType, isChecked);
        });

        // Quaternary permission cascade
        $(".quaternary-checkbox").on("change", function () {
            let moduleId = $(this).data("module");
            let parentId = $(this).data("parent");
            let permissionType = $(this).data("permission");
            let isChecked = $(this).is(":checked");

            $(`.quinary-checkbox[data-parent="${moduleId}"][data-permission="${permissionType}"]`)
                .prop("checked", isChecked)
                .each(function () {
                    updatePermissionArray($(this).data("module"), permissionType, isChecked);
                });

            let allChecked = $(`.quaternary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]`).length ===
                $(`.quaternary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]:checked`).length;

            $(`.tertiary-checkbox[data-module="${parentId}"][data-permission="${permissionType}"]`).prop("checked", allChecked);

            updatePermissionArray(moduleId, permissionType, isChecked);
        });

        // Quinary permission upward sync
        $(".quinary-checkbox").on("change", function () {
            let moduleId = $(this).data("module");
            let parentId = $(this).data("parent");
            let permissionType = $(this).data("permission");
            let isChecked = $(this).is(":checked");

            let allChecked = $(`.quinary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]`).length ===
                $(`.quinary-checkbox[data-parent="${parentId}"][data-permission="${permissionType}"]:checked`).length;

            $(`.quaternary-checkbox[data-module="${parentId}"][data-permission="${permissionType}"]`).prop("checked", allChecked);

            updatePermissionArray(moduleId, permissionType, isChecked);
        });
    }

    // Load permissions on role change
    $('#roleSelect2').on('change', function () {
        const roleId = $(this).val();
        loadPermissions(roleId);
    });

    $('#roleSelect2').trigger('change');

    // Save permissions
    $(document).on("click", "#savePermissions", function () {
        if (updatedPermissions.length === 0) {
            toastr.info("No changes to save.");
            return;
        }

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
            },
            error: function (xhr, status, error) {
                toastr.warning("Failed to update permissions.");
            }
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



