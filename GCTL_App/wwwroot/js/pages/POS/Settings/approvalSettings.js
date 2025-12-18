$(document).ready(function () {
    // Debug log
    showDev("approvalSettings.js: document ready");

    let page = 1;
    let pageSize = 10;
    let search = "";
    let sortColumn = "ApprovalSettingID";
    let sortDirection = "asc";
    let organizationId = "";
    let approvalTypeId = "";


 



    //#region Load approval settings
    function loadApprovalSettings() {
        $.ajax({
            url: '/ApprovalMatrix/GetApprovalSettings',
            data: {
                page: page,
                pageSize: pageSize,
                search: search,
                sortColumn: sortColumn,
                sortDirection: sortDirection,
                organizationId: organizationId,
                approvalTypeId: approvalTypeId
            },
            success: function (res) {
                let rows = '';
                $.each(res.data, function (i, item) {
                    rows += `
                        <tr class="position-static">
                            <td class="align-middle white-space-nowrap ps-0">${item.approvalSettingID}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.approvalTypeName}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.organizationName}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.branchName}</td>
                            
                            <td class="align-middle white-space-nowrap ps-2">${item.allowSelfApproval ? 'Yes' : 'No'}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.selfExceptionApprovalName || ''}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2 action">
                                <div class="d-flex g-3">
                                    <button class="btn btn-phoenix-secondary btn-icon me-2 fs-10 text-body px-0" 
                                            type="button" data-bs-toggle="modal" 
                                            data-bs-target="#editApprovalSettingModal" 
                                            data-id="${item.approvalSettingID}">
                                        <span class="fas fa-edit"></span>
                                    </button>
                                    <button class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0" 
                                            data-id="${item.approvalSettingID}">
                                        <span class="fas fa-trash"></span>
                                    </button>
                                </div>
                            </td>
                        </tr>`;
                });
                $("#approval-settings-table").html(rows);

                // Handle DynamicTableDrag if present
                if (typeof DynamicTableDrag !== 'undefined') {
                    DynamicTableDrag.refreshTableSettings('ApprovalSettings');
                }

                //<td class="align-middle white-space-nowrap ps-2">${formatDate(item.startDate)}</td>
                //<td class="align-middle white-space-nowrap ps-2">${item.endDate ? formatDate(item.endDate) : ''}</td>

                // Pagination
                let totalPages = Math.ceil(res.totalRecords / pageSize);
                let paginationHtml = '';
                for (let i = 1; i <= totalPages; i++) {
                    paginationHtml += `<li class="page-item ${i === page ? 'active' : ''}">
                        <a class="page-link" href="#">${i}</a>
                    </li>`;
                }
                $(".pagination").html(paginationHtml);

                // Show total info
                $("[data-list-info]").text(`Showing ${(page - 1) * pageSize + 1} to ${Math.min(page * pageSize, res.totalRecords)} of ${res.totalRecords}`);
            },
            error: function (xhr, status, error) {
                console.error("Error loading approval settings:", error);
                toastr.error("Failed to load approval settings.");
            }
        });
    }

    // Date formatting helper
    //function formatDate(dateStr) {
    //    if (!dateStr) return '';
    //    let d = new Date(dateStr);
    //    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    //}

    // Initial load
    loadApprovalSettings();

    // Search
    $("#searchInput").on("keyup", function () {
        search = $(this).val();
        page = 1;
        loadApprovalSettings();
    });

    // Page Size
    $("#pageSizeSelect").on("change", function () {
        pageSize = parseInt($(this).val());
        page = 1;
        loadApprovalSettings();
    });

    // Pagination click
    $(document).on("click", ".pagination .page-link", function (e) {
        e.preventDefault();
        page = parseInt($(this).text());
        loadApprovalSettings();
    });

    // Filters
    $("#organizationFilter, #approvalTypeFilter").on("change", function () {
        organizationId = $("#organizationFilter").val();
        approvalTypeId = $("#approvalTypeFilter").val();
        page = 1;
        loadApprovalSettings();
    });

    // Sorting
    $("th.sort").on("click", function () {
        sortColumn = $(this).data("sort");
        sortDirection = sortDirection === "asc" ? "desc" : "asc";
        loadApprovalSettings();
    });

    //#endregion



    // Utility: refresh Select2 options to prevent duplicates
    function refreshSelectOptions(tableBodyId) {
        // Collect all selected values
        let selectedValues = [];
        $(`#${tableBodyId} select`).each(function () {
            let val = $(this).val();
            if (val) selectedValues.push(val);
        });

        // Disable duplicates, allow current selection
        $(`#${tableBodyId} select`).each(function () {
            let currentVal = $(this).val();
            $(this).find('option').each(function () {
                let optionVal = $(this).attr('value');
                if (selectedValues.includes(optionVal) && optionVal !== currentVal) {
                    $(this).prop('disabled', true);
                } else {
                    $(this).prop('disabled', false);
                }
            });
            // Refresh Select2 UI
            $(this).trigger('change.select2');
        });
    }

    // Add new approval level row
    window.addNewLevelRow = function (tableBodyId = 'approvalLevelTableBody') {
        const index = $(`#${tableBodyId} tr`).length;
        let template = $("#approvalLevelRowTemplate").prop("outerHTML");
        template = template.replace(/INDEX/g, index);
        template = template.replace('id="approvalLevelRowTemplate"', "");
        template = template.replace('name="ApprovalLevels.Index" value=""', `name="ApprovalLevels.Index" value="${index}"`);
        $(`#${tableBodyId}`).append(template);

        // Reindex rows
        $(`#${tableBodyId} tr`).each(function (i) {
            $(this).find('input, select').each(function () {
                let name = $(this).attr('name');
                if (name) {
                    let newName = name.replace(/\[\d+\]/, `[${i}]`);
                    $(this).attr('name', newName);
                }
            });
            $(this).find('input[name="ApprovalLevels.Index"]').val(i);
        });

        // Initialize Select2 for new row
        $(`#${tableBodyId} tr:last select`).select2({ width: '100%' , placeholder: 'Select an option', allowClear: true });

        // Refresh options to ignore previous selections
        refreshSelectOptions(tableBodyId);
    };

    // Remove approval level row
    window.removeLevelRow = function (button) {
        $(button).closest("tr").remove();

        // Reindex rows
        $("#approvalLevelTableBody tr, #editApprovalLevelTableBody tr").each(function (index) {
            $(this).find('input, select').each(function () {
                let name = $(this).attr('name');
                if (name) {
                    let newName = name.replace(/\[\d+\]/, `[${index}]`);
                    $(this).attr('name', newName);
                }
            });
            $(this).find('input[name="ApprovalLevels.Index"]').val(index);
        });

        // Refresh Select2 options so removed employee becomes available again
        refreshSelectOptions("approvalLevelTableBody");
        refreshSelectOptions("editApprovalLevelTableBody");
    };

    // Refresh whenever a selection changes
    $(document).on('change', '#approvalLevelTableBody select, #editApprovalLevelTableBody select', function () {
        refreshSelectOptions('approvalLevelTableBody');
        refreshSelectOptions('editApprovalLevelTableBody');
    });


    //// Add new approval level row
    //window.addNewLevelRow = function (tableBodyId = 'approvalLevelTableBody') {
    //    const index = $(`#${tableBodyId} tr`).length;
    //    let template = $("#approvalLevelRowTemplate").prop("outerHTML");
    //    template = template.replace(/INDEX/g, index);
    //    template = template.replace('id="approvalLevelRowTemplate"', "");
    //    template = template.replace('name="ApprovalLevels.Index" value=""', `name="ApprovalLevels.Index" value="${index}"`);
    //    $(`#${tableBodyId}`).append(template);

    //    // Reindex rows
    //    $(`#${tableBodyId} tr`).each(function (i) {
    //        $(this).find('input, select').each(function () {
    //            let name = $(this).attr('name');
    //            if (name) {
    //                let newName = name.replace(/\[\d+\]/, `[${i}]`);
    //                $(this).attr('name', newName);
    //            }
    //        });
    //        $(this).find('input[name="ApprovalLevels.Index"]').val(i);
    //    });

    //    $(`#${tableBodyId} tr:last select`).select2({
    //        width: '100%',   // optional: makes it fit nicely
    //        placeholder: 'Select an option', // optional: adds placeholder
    //        allowClear: true // optional: allows clearing the selection
    //    });


    //};

    //// Remove approval level row
    //window.removeLevelRow = function (button) {
    //    $(button).closest("tr").remove();
    //    $("#approvalLevelTableBody tr, #editApprovalLevelTableBody tr").each(function (index) {
    //        $(this).find('input, select').each(function () {
    //            let name = $(this).attr('name');
    //            if (name) {
    //                let newName = name.replace(/\[\d+\]/, `[${index}]`);
    //                $(this).attr('name', newName);
    //            }
    //        });
    //        $(this).find('input[name="ApprovalLevels.Index"]').val(index);
    //    });
    //};

    //#region Create form submission
    $("#createApprovalSettingForm").on("submit", function (e) {
        e.preventDefault();
        let isValid = true;
        $("select[name^='ApprovalLevels'][name$='.ApproverEmployeeID']").each(function () {
            if ($(this).closest('tr').is(':visible') && !$(this).val()) {
                isValid = false;
                toastr.warning("Please select an approver for all levels.");
                return false;
            }
        });

        if (!isValid) return;

        $.ajax({
            url: '/ApprovalMatrix/Create',
            type: "POST",
            data: $(this).serialize(),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    $("#createApprovalSettingForm")[0].reset();
                    $("#approvalLevelTableBody").empty();
                    addNewLevelRow();
                    loadApprovalSettings();
                    choiceManager.resetAllChoices();
                } else {
                    toastr.warning(res.message || "Validation failed.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error creating approval setting:", error);
                toastr.error("Something went wrong!");
            }
        });
    });

    //#endregion

    // Edit button click
    $(document).on("click", "button[data-bs-target='#editApprovalSettingModal']", function () {
        const id = $(this).data('id');
        showDev("Edit button clicked for ID: " + id);
        $.ajax({
            url: '/ApprovalMatrix/GetApprovalSettingById',
            data: { id: id },
            success: function (res) {
                showDev(res, 'ddss')
                if (!res.success) {
                    toastr.error(res.message || "Failed to load approval setting.");
                    return;
                }



                let data = res.data;
                choiceManager.setChoiceValue('edit_ApprovalSettingID', data.approvalSettingID);
                choiceManager.setChoiceValue('edit_OrganizationID', data.organizationID);
                choiceManager.setChoiceValue('edit_OrganizationBranchID', data.organizationBranchID);
                choiceManager.setChoiceValue('edit_ApprovalTypeID', data.approvalTypeID);
                choiceManager.setChoiceValue('edit_SelfExceptionApprovalID', data.selfExceptionApprovalID);

                //$('#edit_StartDate').val(data.startDate.split('T')[0]);
                //$('#edit_EndDate').val(data.endDate ? data.endDate.split('T')[0] : '');

                $('#edit_AllowSelfApproval').prop('checked', data.allowSelfApproval);

                // Populate approval levels
                $('#editApprovalLevelTableBody').empty();
                $.each(data.approvalLevels, function (i, level) {
                    let template = $("#approvalLevelRowTemplate").prop("outerHTML");
                    template = template.replace(/INDEX/g, i);
                    template = template.replace('id="approvalLevelRowTemplate"', "");
                    template = template.replace('name="ApprovalLevels.Index" value=""', `name="ApprovalLevels.Index" value="${i}"`);
                    template = template.replace('name="ApprovalLevels[INDEX].LevelNumber"', `name="ApprovalLevels[${i}].LevelNumber" value="${level.levelNumber}"`);
                    template = template.replace('name="ApprovalLevels[INDEX].ApproverEmployeeID"', `name="ApprovalLevels[${i}].ApproverEmployeeID"`);
                    template = template.replace('name="ApprovalLevels[INDEX].IsEnabled"', `name="ApprovalLevels[${i}].IsEnabled" ${level.isEnabled ? 'checked' : ''}`);
                    $('#editApprovalLevelTableBody').append(template);
                    $(`#editApprovalLevelTableBody select[name="ApprovalLevels[${i}].ApproverEmployeeID"]`).val(level.approverEmployeeID);
                });



                try {
                    $('#editApprovalSettingModal').modal('show');
                    showDev("Edit modal opened for ID: " + id);
                } catch (error) {
                    console.error("Error opening edit modal:", error);
                    toastr.error("Failed to open edit modal.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error loading approval setting:", error);
                toastr.error("Failed to load approval setting details.");
            }
        });
    });

    // Save edit
    window.saveEditApprovalSetting = function () {
        let isValid = true;
        $("#editApprovalLevelTableBody select[name^='ApprovalLevels'][name$='.ApproverEmployeeID']").each(function () {
            if ($(this).closest('tr').is(':visible') && !$(this).val()) {
                isValid = false;
                toastr.warning("Please select an approver for all levels.");
                return false;
            }
        });

        if (!isValid) return;

        $.ajax({
            url: '/ApprovalMatrix/Edit',
            type: 'POST',
            data: $('#editApprovalSettingForm').serialize(),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    hideModal('editApprovalSettingModal');

                    $('.rmvModal').trigger('click');

                    loadApprovalSettings();
                    choiceManager.resetAllChoices();
                } else {
                    toastr.warning(res.message || "Update failed.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error updating approval setting:", error);
                toastr.error("Something went wrong!");
            }
        });
    };

    // Delete button click
    $(document).on("click", "button.btn-phoenix-secondary.btn-icon.text-danger", function (e) {
        e.preventDefault();
        const id = $(this).data('id');
        showDev("Delete button clicked for ID: " + id);
        if (id) {
            $('#deleteApprovalSettingId').val(id);
            try {
                $('#deleteConfirmModal').modal('show');
                showDev("Delete modal opened for ID: " + id);
            } catch (error) {
                console.error("Error showing delete modal:", error);
                toastr.error("Failed to open delete modal.");
            }
        } else {
            toastr.warning("Invalid approval setting ID.");
        }
    });

    // Confirm delete
    window.confirmDelete = function () {
        const id = $('#deleteApprovalSettingId').val();
        if (!id) {
            toastr.warning("Please select an approval setting to delete.");
            return;
        }

        $.ajax({
            url: '/ApprovalMatrix/Delete',
            type: 'POST',
            data: {
                id: id,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    $('#deleteConfirmModal').modal('hide');
                    loadApprovalSettings();
                } else {
                    toastr.warning(res.message || "Delete failed.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error deleting approval setting:", error);
                toastr.error("Something went wrong!");
            }
        });
    };

    // Close delete modal
    $('.closeDltModal').on('click', function () {
        try {
            $('#deleteConfirmModal').modal('hide');
            showDev("Delete modal closed via closeDltModal");
        } catch (error) {
            console.error("Error closing delete modal:", error);
            toastr.error("Failed to close delete modal.");
        }
    });

    // Reinitialize modals after AJAX
    $(document).ajaxComplete(function () {
        showDev("AJAX complete, reinitializing modals");
        try {
            $('#editApprovalSettingModal, #deleteConfirmModal').modal({ show: false });
        } catch (error) {
            console.error("Error reinitializing modals:", error);
        }
    });

    // Initialize with one row
    addNewLevelRow();
});