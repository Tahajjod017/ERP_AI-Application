$(document).ready(function () {

    //#region Pending Approvals Table
    let pendingPage = 1;
    let pendingPageSize = 10;
    let pendingSearch = "";
    let pendingSortColumn = "RequisitionId";
    let pendingSortDirection = "asc";
    let pendingProductTypeId = "";
    let pendingFromDate = "";
    let pendingToDate = "";

    function loadPendingApprovals() {
        $.ajax({
            url: '/RequisitionApprover/GetPendingApprovals',
            data: {
                page: pendingPage,
                pageSize: pendingPageSize,
                search: pendingSearch,
                sortColumn: pendingSortColumn,
                sortDirection: pendingSortDirection,
                productTypeId: pendingProductTypeId,
                fromDate: pendingFromDate,
                toDate: pendingToDate
            },
            success: function (res) {
                showDev(res, 'Pending Approvals');

                let rows = '';
                $.each(res.data, function (i, item) {
                    rows += `
                        <tr class="position-static">
                            <td class="align-middle white-space-nowrap ps-0">#${item.requisitionId}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.requisitionCode}</td>
                            <td class="align-middle white-space-nowrap ps-2">${formatDate(item.requisitionDate)}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.requisitionBy}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.totalItems}</td>
                            <td class="align-middle white-space-nowrap ps-2">
                                <span class="badge badge-phoenix ${getPriorityBadge(item.priority)}">${item.priority}</span>
                            </td>
                            <td class="align-middle white-space-nowrap ps-2">Step ${item.currentStep + 1}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <a href="#" class="nav-item me-2 viewApprovalBtn" 
                                   data-id="${item.requisitionId}" 
                                   data-bs-toggle="modal" 
                                   data-bs-target="#approvalModal">
                                    <i class="fas fa-eye text-black"></i>
                                </a>
                            </td>
                        </tr>`;
                });
                $("#pending-approvals-table").html(rows);

                DynamicTableDrag.refreshTableSettings('PendingApprovals');

                // Pagination
                renderPagination(res.totalRecords, pendingPage, pendingPageSize, 'pendingPagination');

                // Info
                $("[data-list-info='pending-list-info']").text(
                    `Showing ${(pendingPage - 1) * pendingPageSize + 1} to ${Math.min(pendingPage * pendingPageSize, res.totalRecords)} of ${res.totalRecords}`
                );
            }
        });
    }

    // Pending search
    $("#pendingSearchInput").on("keyup", function () {
        pendingSearch = $(this).val();
        pendingPage = 1;
        loadPendingApprovals();
    });

    // Pending page size
    $("#pendingPageSizeSelect").on("change", function () {
        pendingPageSize = $(this).val();
        pendingPage = 1;
        loadPendingApprovals();
    });

    // Pending filters
    $("#pendingProductTypeFilter").on("change", function () {
        pendingProductTypeId = $(this).val();
        pendingPage = 1;
        loadPendingApprovals();
    });

    $("#pendingFromDate, #pendingToDate").on("change", function () {
        pendingFromDate = $("#pendingFromDate").val();
        pendingToDate = $("#pendingToDate").val();
        pendingPage = 1;
        loadPendingApprovals();
    });

    // Pending pagination
    $(document).on("click", "#pendingPagination .page-link", function (e) {
        e.preventDefault();
        pendingPage = parseInt($(this).text());
        loadPendingApprovals();
    });

    $("#pendingPrevBtn").on("click", function (e) {
        e.preventDefault();
        if (pendingPage > 1) {
            pendingPage--;
            loadPendingApprovals();
        }
    });

    $("#pendingNextBtn").on("click", function (e) {
        e.preventDefault();
        pendingPage++;
        loadPendingApprovals();
    });

    // Pending sorting
    $("#PendingApprovals th.sort").on("click", function () {
        pendingSortColumn = $(this).data("sort");
        pendingSortDirection = pendingSortDirection === "asc" ? "desc" : "asc";
        loadPendingApprovals();
    });

    //#endregion

    //#region Approved History Table
    let approvedPage = 1;
    let approvedPageSize = 10;
    let approvedSearch = "";
    let approvedSortColumn = "RequisitionId";
    let approvedSortDirection = "desc";
    let approvedProductTypeId = "";
    let approvedFromDate = "";
    let approvedToDate = "";

    function loadApprovedHistory() {
        $.ajax({
            url: '/RequisitionApprover/GetApprovedHistory',
            data: {
                page: approvedPage,
                pageSize: approvedPageSize,
                search: approvedSearch,
                sortColumn: approvedSortColumn,
                sortDirection: approvedSortDirection,
                productTypeId: approvedProductTypeId,
                fromDate: approvedFromDate,
                toDate: approvedToDate
            },
            success: function (res) {
                showDev(res, 'Approved History');

                let rows = '';
                $.each(res.data, function (i, item) {
                    let editBtn = item.canEdit
                        ? `<a href="#" class="nav-item me-2 editApprovalBtn" 
                              data-id="${item.requisitionId}" 
                              data-bs-toggle="modal" 
                              data-bs-target="#approvalModal">
                              <i class="fas fa-edit text-black"></i>
                           </a>`
                        : '';

                    rows += `
                        <tr class="position-static">
                            <td class="align-middle white-space-nowrap ps-0">#${item.requisitionId}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.requisitionCode}</td>
                            <td class="align-middle white-space-nowrap ps-2">${formatDate(item.requisitionDate)}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.requisitionBy}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.totalItems}</td>
                            <td class="align-middle white-space-nowrap ps-2">
                                <span class="badge badge-phoenix ${getPriorityBadge(item.priority)}">${item.priority}</span>
                            </td>
                            <td class="align-middle white-space-nowrap ps-2">
                                <span class="badge badge-phoenix ${getStatusBadge(item.status)}">${item.status}</span>
                            </td>
                            <td class="align-middle white-space-nowrap ps-2">${item.approvedAt ? formatDate(item.approvedAt) : 'N/A'}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="d-flex justify-content-end">
                                    <a href="#" class="nav-item me-2 viewApprovalBtn" 
                                       data-id="${item.requisitionId}" 
                                       data-bs-toggle="modal" 
                                       data-bs-target="#approvalModal">
                                        <i class="fas fa-eye text-black"></i>
                                    </a>
                                    ${editBtn}
                                </div>
                            </td>
                        </tr>`;
                });
                $("#approved-history-table").html(rows);

                DynamicTableDrag.refreshTableSettings('ApprovedHistory');

                // Pagination
                renderPagination(res.totalRecords, approvedPage, approvedPageSize, 'approvedPagination');

                // Info
                $("[data-list-info='approved-list-info']").text(
                    `Showing ${(approvedPage - 1) * approvedPageSize + 1} to ${Math.min(approvedPage * approvedPageSize, res.totalRecords)} of ${res.totalRecords}`
                );
            }
        });
    }

    // Approved search
    $("#approvedSearchInput").on("keyup", function () {
        approvedSearch = $(this).val();
        approvedPage = 1;
        loadApprovedHistory();
    });

    // Approved page size
    $("#approvedPageSizeSelect").on("change", function () {
        approvedPageSize = $(this).val();
        approvedPage = 1;
        loadApprovedHistory();
    });

    // Approved filters
    $("#approvedProductTypeFilter").on("change", function () {
        approvedProductTypeId = $(this).val();
        approvedPage = 1;
        loadApprovedHistory();
    });

    $("#approvedFromDate, #approvedToDate").on("change", function () {
        approvedFromDate = $("#approvedFromDate").val();
        approvedToDate = $("#approvedToDate").val();
        approvedPage = 1;
        loadApprovedHistory();
    });

    // Approved pagination
    $(document).on("click", "#approvedPagination .page-link", function (e) {
        e.preventDefault();
        approvedPage = parseInt($(this).text());
        loadApprovedHistory();
    });

    $("#approvedPrevBtn").on("click", function (e) {
        e.preventDefault();
        if (approvedPage > 1) {
            approvedPage--;
            loadApprovedHistory();
        }
    });

    $("#approvedNextBtn").on("click", function (e) {
        e.preventDefault();
        approvedPage++;
        loadApprovedHistory();
    });

    // Approved sorting
    $("#ApprovedHistory th.sort").on("click", function () {
        approvedSortColumn = $(this).data("sort");
        approvedSortDirection = approvedSortDirection === "asc" ? "desc" : "asc";
        loadApprovedHistory();
    });

    //#endregion

    //#region Modal - View/Approve/Edit
    let currentRequisitionDetails = null;
    let isEditMode = false;

    $(document).on("click", ".viewApprovalBtn, .editApprovalBtn", function () {
        const id = $(this).data('id');
        isEditMode = $(this).hasClass('editApprovalBtn');

        $.ajax({
            url: '/RequisitionApprover/GetRequisitionDetails',
            type: "GET",
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    currentRequisitionDetails = response.data;
                    populateModal(response.data, isEditMode);
                } else {
                    toastr.error("Failed to load requisition details");
                }
            },
            error: function () {
                toastr.error("An error occurred while loading details");
            }
        });
    });

    function populateModal(data, editMode) {
        // Set basic info
        $("#modal_RequisitionId").val(data.requisitionId);
        $("#modal_RequisitionCode").text(data.requisitionCode);
        $("#modal_RequisitionDate").text(formatDate(data.requisitionDate));
        $("#modal_RequisitionBy").text(data.requisitionBy);
        $("#modal_Priority").text(data.priority);
        $("#modal_Organization").text(data.organization);
        $("#modal_Branch").text(data.branch);
        $("#modal_RequisitionNote").text(data.requisitionNote || 'N/A');
        $("#modal_IsFirstApprover").val(data.isFirstApprover);
        $("#modal_CanEdit").val(data.canEdit);

        // Clear and populate products table
        let productsHtml = '';
        $.each(data.items, function (i, item) {
            let approvedQtyInput = '';

            if (data.isFirstApprover && (data.canApprove || (editMode && data.canEdit))) {
                // First approver can enter quantity
                approvedQtyInput = `
                    <input type="number" 
                           class="form-control form-control-sm approved-qty-input" 
                           name="Items[${i}].ApprovedQuantity" 
                           data-item-id="${item.itemId}"
                           value="${item.approvedQuantity || item.requestedQuantity}" 
                           min="0" 
                           max="${item.requestedQuantity}" 
                           step="0.01" 
                           required />
                    <input type="hidden" name="Items[${i}].ItemId" value="${item.itemId}" />`;
            } else {

                approvedQtyInput = `
                    <input type="hidden"
                           class="form-control form-control-sm approved-qty-input"
                           name="Items[${i}].ApprovedQuantity" 
                           data-item-id="${item.itemId}"
                           value="${item.approvedQuantity || item.requestedQuantity}" 
                           min="0" 
                           max="${item.requestedQuantity}" 
                           step="0.01" 
                           required />
                    <input type="hidden" name="Items[${i}].ItemId" value="${item.itemId}" />
                    <span class="fw-bold">${item.approvedQuantity || 'Not set'}</span>`;
                // Other approvers just see the quantity
               //  approvedQtyInput = `<span class="fw-bold">${item.approvedQuantity || 'Not set'}</span>`;
            }

            productsHtml += `
                <tr>
                    <td>${item.productType}</td>
                    <td>${item.productName}</td>
                    <td>${item.brand}</td>
                    <td>${item.unit}</td>
                    <td>${item.requestedQuantity}</td>
                    <td>${approvedQtyInput}</td>
                </tr>`;
        });
        $("#modal_ProductsTable").html(productsHtml);

        // Populate approval history
        let historyHtml = '';
        if (data.approvalHistory && data.approvalHistory.length > 0) {
            $.each(data.approvalHistory, function (i, hist) {
                historyHtml += `
                    <tr>
                        <td>Step ${hist.step}</td>
                        <td>${hist.approverName}</td>
                        <td><span class="badge badge-phoenix ${hist.status === 'Approved' ? 'badge-phoenix-success' : 'badge-phoenix-danger'}">${hist.status}</span></td>
                        <td>${hist.approvedAt ? formatDate(hist.approvedAt) : 'N/A'}</td>
                        <td>${hist.note || 'N/A'}</td>
                    </tr>`;
            });
        } else {
            historyHtml = '<tr><td colspan="5" class="text-center">No approval history yet</td></tr>';
        }
        $("#modal_ApprovalHistory").html(historyHtml);

        // Setup footer buttons
        let footerHtml = '';

        if (editMode && data.canEdit) {
            // Edit mode
            $("#approvalModalLabel").text("Edit Approval");
            $("#approverNoteSection").show();
            footerHtml = `
                <button class="btn btn-primary" type="button" onclick="saveEditApproval()">Update Approval</button>
                <button class="btn btn-outline-primary" type="button" data-bs-dismiss="modal">Cancel</button>`;
        } else if (data.canApprove) {
            // Approve mode
            $("#approvalModalLabel").text("Approve Requisition");
            $("#approverNoteSection").show();
            footerHtml = `
                <button class="btn btn-success" type="button" onclick="approveRequisition()">Approve</button>
                <button class="btn btn-danger" type="button" onclick="openDeclineModal()">Decline</button>
                <button class="btn btn-outline-primary" type="button" data-bs-dismiss="modal">Cancel</button>`;
        } else {
            // View only mode
            $("#approvalModalLabel").text("Requisition Details");
            $("#approverNoteSection").hide();
            footerHtml = `<button class="btn btn-outline-primary" type="button" data-bs-dismiss="modal">Close</button>`;
        }

        $("#modalFooterButtons").html(footerHtml);
    }

    //#endregion

    //#region Approve Requisition
    window.approveRequisition = function () {
        // Validate quantities if first approver
        const isFirstApprover = $("#modal_IsFirstApprover").val() === 'true';
        let valid = true;

        if (isFirstApprover) {
            $(".approved-qty-input").each(function () {
                const val = parseFloat($(this).val());
                const max = parseFloat($(this).attr('max'));

                if (isNaN(val) || val <= 0) {
                    toastr.warning("Please enter valid approved quantities");
                    valid = false;
                    return false;
                }

                if (val > max) {
                    toastr.warning("Approved quantity cannot exceed requested quantity");
                    valid = false;
                    return false;
                }
            });
        }

        if (!valid) return;

        const formData = $("#approvalForm").serialize();

        $.ajax({
            url: '/RequisitionApprover/ApproveRequisition',
            type: 'POST',
            data: formData,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    hideModal('approvalModal');
                    loadPendingApprovals();
                    loadApprovedHistory();
                } else {
                    toastr.warning(res.message || "Approval failed");
                }
            },
            error: function () {
                toastr.error("An error occurred while approving");
            }
        });
    };

    //#endregion

    //#region Decline Requisition
    window.openDeclineModal = function () {
        const reqId = $("#modal_RequisitionId").val();
        $("#decline_RequisitionId").val(reqId);
        $("#decline_Note").val('');

        hideModal('approvalModal');
        showModal('declineModal');
    };

    window.confirmDecline = function () {
        const note = $("#decline_Note").val().trim();

        if (!note) {
            toastr.warning("Please provide a reason for declining");
            return;
        }

        const formData = $("#declineForm").serialize();

        $.ajax({
            url: '/RequisitionApprover/DeclineRequisition',
            type: 'POST',
            data: formData,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    hideModal('declineModal');
                    loadPendingApprovals();
                    loadApprovedHistory();
                } else {
                    toastr.warning(res.message || "Decline failed");
                }
            },
            error: function () {
                toastr.error("An error occurred while declining");
            }
        });
    };

    //#endregion

    //#region Edit Approval
    window.saveEditApproval = function () {
        // Validate quantities if first approver
        const isFirstApprover = $("#modal_IsFirstApprover").val() === 'true';
        let valid = true;

        if (isFirstApprover) {
            $(".approved-qty-input").each(function () {
                const val = parseFloat($(this).val());
                const max = parseFloat($(this).attr('max'));

                if (isNaN(val) || val <= 0) {
                    toastr.warning("Please enter valid approved quantities");
                    valid = false;
                    return false;
                }

                if (val > max) {
                    toastr.warning("Approved quantity cannot exceed requested quantity");
                    valid = false;
                    return false;
                }
            });
        }

        if (!valid) return;

        const formData = $("#approvalForm").serialize();

        $.ajax({
            url: '/RequisitionApprover/EditApprovedRequisition',
            type: 'POST',
            data: formData,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    hideModal('approvalModal');
                    loadApprovedHistory();
                } else {
                    toastr.warning(res.message || "Update failed");
                }
            },
            error: function () {
                toastr.error("An error occurred while updating");
            }
        });
    };

    //#endregion

    //#region Export Functions
    $("#btnExportApprovedPDF").on("click", function () {
        customToaster.loading("Generating PDF...");

        fetch(`/RequisitionApprover/GeneratePDF?fromDate=${approvedFromDate}&toDate=${approvedToDate}&approved=true`,
            { method: "POST" })
            .then(res => res.blob())
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = `Approved_Requisitions_${new Date().getTime()}.pdf`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
                customToaster.success("PDF generated successfully!");
            })
            .catch(() => {
                customToaster.error("Failed to generate PDF");
            });
    });

    $("#btnExportApprovedXL").on("click", function () {
        customToaster.loading("Generating Excel...");

        fetch(`/RequisitionApprover/DownloadExcel?fromDate=${approvedFromDate}&toDate=${approvedToDate}&approved=true`)
            .then(res => res.blob())
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = `Approved_Requisitions_${new Date().getTime()}.xlsx`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
                customToaster.success("Excel generated successfully!");
            })
            .catch(() => {
                customToaster.error("Failed to generate Excel");
            });
    });

    //#endregion

    //#region Helper Functions
    function formatDate(dateStr) {
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    }

    function getPriorityBadge(priority) {
        const p = (priority || 'Normal').toLowerCase();
        switch (p) {
            case 'urgent': return 'badge-phoenix-danger';
            case 'advance': return 'badge-phoenix-warning';
            case 'normal':
            default: return 'badge-phoenix-primary';
        }
    }

    function getStatusBadge(status) {
        const s = (status || '').toLowerCase();
        if (s.includes('fully approved')) return 'badge-phoenix-success';
        if (s.includes('partially')) return 'badge-phoenix-warning';
        if (s.includes('declined')) return 'badge-phoenix-danger';
        return 'badge-phoenix-secondary';
    }

    function renderPagination(totalRecords, currentPage, pageSize, containerId) {
        const totalPages = Math.ceil(totalRecords / pageSize);
        let html = '';

        for (let i = 1; i <= totalPages; i++) {
            html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#">${i}</a>
            </li>`;
        }

        $(`#${containerId}`).html(html);
    }

    function showModal(modalId) {
        $(`#${modalId}`).modal('show');
    }

    function hideModal(modalId) {
        $(`#${modalId}`).modal('hide');
    }

    //#endregion

    // Initial load
    loadPendingApprovals();
    loadApprovedHistory();
});