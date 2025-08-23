$(document).ready(function () {
    showDev('js Loaded')

    //#region flatpick

    flatpickr("#timepicker2", {
        mode: "range",
        dateFormat: "d/m/y",
        disableMobile: true
    });

    flatpickr("#approvedDateRange", {
        mode: "range",
        dateFormat: "d/m/y",
        disableMobile: true
    });

    //#endregion

    let pendingPage = 1;
    let approvedPage = 1;
    const pageSize = 10;
    let pendingSortColumn = "employeeName";
    let pendingSortDirection = "asc";
    let approvedSortColumn = "employeeName";
    let approvedSortDirection = "asc";

    //#region LoadInc func Card/Pending/Approved
    function loadIncrementCards() {
        $.ajax({
            url: "/IncrementApprove/GetIncrementCards",
            method: "GET",
            success: function (data) {
                const cardsContainer = $("#incrementCards");
                cardsContainer.empty();
                const cards = [
                    { title: "Annual Increment", count: data.annualCount, pending: data.annualPending, bg: "bg-black-le", icon: "trending-up" },
                    { title: "Performance Bonus", count: data.performanceCount, pending: data.performancePending, bg: "bg-blue-le", icon: "trophy" },
                    { title: "Promotion Increment", count: data.promotionCount, pending: data.promotionPending, bg: "bg-purple-le", icon: "arrow-up-circle" },
                    { title: "Special Increment", count: data.specialCount, pending: data.specialPending, bg: "bg-pink-le", icon: "star" }
                ];
                cards.forEach(card => {
                    const badgeClass = card.pending > 0 ? "badge-phoenix-success" : "badge-phoenix-warning";
                    cardsContainer.append(`
                        <div class="col-xl-3 col-md-6">
                            <div class="card ${card.bg}">
                                <div class="card-body">
                                    <div class="d-flex align-items-center justify-content-between">
                                        <div class="text-start">
                                            <p class="mb-1">${card.title}</p>
                                            <h4>${card.count}</h4>
                                        </div>
                                        <div class="d-flex">
                                            <div class="flex-shrink-0 me-2">
                                                <span class="avatar avatar-md d-flex">
                                                    <i class="fas fa-${card.icon} fs-5 text-white"></i>
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                    <span class="badge badge-phoenix ${badgeClass}">Pending: ${card.pending}</span>
                                </div>
                            </div>
                        </div>
                    `);
                });
            },
            error: function () {
                console.error("Failed to load increment cards");
            }
        });
    }

    function loadPendingIncrements(page = 1) {
        const formData = new FormData();
        formData.append("page", page);
        formData.append("pageSize", pageSize);
        formData.append("incrementType", $("#incrementType").val() || "");
        formData.append("status", $("#statusSelect").val() || "");
        formData.append("searchInput", $("#searchInputPe").val() || "");
        formData.append("sortBy", $("#sortBy").val() || "");
        formData.append("dateRange", $("#timepicker2").val() || "");
        formData.append("sortColumn", pendingSortColumn);
        formData.append("sortDirection", pendingSortDirection);
        formData.append("sortDirection", pendingSortDirection);

        $.ajax({
            url: "/IncrementApprove/GetPendingIncrements",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {
                const tbody = $("#increment-approval-body");
                tbody.empty();
                data.increments.forEach(increment => {
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-0">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1" >
                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${increment.avatarUrl}" alt="" />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold">${increment.employeeName}</h6>
                                        <span class="fs-12 fw-normal">${increment.department}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="incrementType align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" >${increment.incrementType}</td>
                            <td class="currentSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" >${increment.currentSalary}</td>
                            <td class="proposedSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" >${increment.proposedSalary}</td>
                            <td class="incrementAmount align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" >${increment.incrementAmount}</td>
                            <td class="effectiveDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" >${increment.effectiveDate}</td>
                            <td class="decision align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" >
                                <a href="#" class="nav-item mx-2 review-increment" data-increment-id="${increment.id}" data-bs-toggle="modal" data-bs-target="#increment_approval_modal" title="Review Increment">
                                    <i class="fas fa-eye text-primary"></i>
                                </a>
                            </td>
                            <td class="align-middle text-end white-space-nowrap pe-0 ps-5"></td>
                        </tr>
                    `);
                });

                //DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('pendingTblDy'), 'pendingTblDy');
                DynamicTableDrag.refreshTableSettings('IncrementPendingTblDy');

                updatePagination("#incrementApprovalTable", data.totalPages, data.totalItems, page, loadPendingIncrements);
                pendingPage = page;
            },
            error: function () {
                console.error("Failed to load pending increments");
            }
        });
    }

    function loadApprovedIncrements(page = 1) {
        const formData = new FormData();
        formData.append("page", page);
        formData.append("pageSize", pageSize);
        formData.append("department", $("#approvedDepartment").val() || "");
        formData.append("employeeName", $("#approvedEmployee").val() || "");
        formData.append("incrementType", $("#approvedIncrementType").val() || "");
        formData.append("searchInput", $("#searchInputAp").val() || "");
        formData.append("sortBy", $("#approvedSort").val() || "");
        formData.append("dateRange", $("#approvedDateRange").val() || "");
        formData.append("sortColumn", approvedSortColumn);
        formData.append("sortDirection", approvedSortDirection);

        $.ajax({
            url: "/IncrementApprove/GetApprovedIncrements",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {
                showDev(data, "increment approve table")
                const tbody = $("#approved-increment-body");
                tbody.empty();
                data.increments.forEach(increment => {
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-0">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1" data-column="0">
                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${increment.avatarUrl}" alt="" />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold">${increment.employeeName}</h6>
                                        <span class="fs-12 fw-normal">${increment.department}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="incrementType align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="1">${increment.incrementType}</td>
                            <td class="currentSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="2">${increment.currentSalary}</td>
                            <td class="proposedSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="3">${increment.proposedSalary}</td>
                            <td class="incrementAmount align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="4">${increment.incrementAmount}</td>
                            <td class="effectiveDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="5">${increment.effectiveDate}</td>
                            <td class="approvedDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="6">${increment.approvedDate}</td>
                            <td class="status align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="7">${increment.status}
                                <div class="universal-tooltip-container position-relative d-inline-block">
                                    <i class="fa-solid fa-circle-info universal-tooltip-trigger"
                                        data-tooltip-url="/IncrementApprove/GetDetails"
                                        data-tooltip-id="${increment.id}"
                                        data-tooltip-data-key="id"
                                        data-tooltip-template="timelineApprovalTemplate"
                                        style="cursor: pointer; font-size: 12px; color: #007bff; margin-left: 5px;"></i>
                                </div>
                            </td>
                            <td class="align-middle text-end white-space-nowrap pe-0 ps-5"></td>
                        </tr>
                    `);
                });

               // DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('approvedTblDy'), 'approvedTblDy');
                DynamicTableDrag.refreshTableSettings('IncrementApproveTblDy');

                updatePagination("#approvedIncrementTable", data.totalPages, data.totalItems, page, loadApprovedIncrements);
                approvedPage = page;
            },
            error: function () {
                console.error("Failed to load approved increments");
            }
        });
    }




    //#endregion

    //#region Tooltip

    function timelineApprovalTemplate(data) {
        const steps = Array.isArray(data) ? data : [data];
        let html = '';


        if (steps.length > 0) {
            steps.forEach((item, index) => {
                const approverStep = item.approverStep ?? '';
                const statusName = item.statusName ?? '';
                const author = item.approvarPerson ?? '';
                const statusDescription = item.approvarNote ?? '';
                const approvedOrDeclineDate = item.approvedOrDeclineDate ?? '';

                // Determine status color
                let statusColor = 'text-body-secondary';
                if (statusName === 'APPROVED') statusColor = 'text-success';
                if (statusName === 'DECLINED') statusColor = 'text-danger';
                if (statusName === 'PENDING') statusColor = 'text-warning';

                html += `

            <style>
                .timeline-item-bar {
                    position: relative;
                }

                .timeline-bar {
                    position: absolute;
                    top: 100%;
                    left: 50%;
                    transform: translateX(-50%);
                    height: 20px;
                    border-right: 2px dashed #dee2e6 !important;
                }

                .timeline-item:last-child .timeline-bar {
                    display: none;
                }

                .icon-item-sm {
                    width: 24px;
                    height: 24px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }

                .bg-primary-subtle {
                    background-color: #e8f4ff !important;
                }

                .text-primary-dark {
                    color: #0d6efd !important;
                }
            </style>

                <div class="timeline-item" style="margin-bottom:1px">
                    <div class="timeline-item position-relative">
                        <div class="row g-md-3">
                            <div class="col-12 col-md-auto d-flex">
                                <!--<div class="timeline-item-date order-1 order-md-0 me-md-4">
                                    <p class="fs-10 fw-semibold text-body-tertiary text-opacity-85 text-end">
                                        ${approverStep} 
                                    </p>
                                </div> -->
                                <div class="timeline-item-bar position-md-relative me-3 me-md-0">
                                    <div class="icon-item icon-item-sm rounded-7 shadow-none bg-primary-subtle">
                                        <span class="fa-solid far fa-file-alt text-primary-dark fs-10"></span>
                                    </div>
                                    <span class="timeline-bar border-end border-dashed"></span>
                                </div>
                            </div>
                            <div class="col">
                                <div class="timeline-item-content ps-6 ps-md-3">
                                    <h5 class="fs-9 lh-sm ${statusColor}">${statusName}</h5>
                                    <p class="fs-9 mb-0">by <a class="fw-semibold" href="#!">${author}</a></p>
                                    <h5 class="fs-9 lh-sm">${approvedOrDeclineDate}</h5>
                                    <p class="fs-9 text-body-secondary">${statusDescription}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>`;
            });
        } else {
            html = '<div class="text-muted" style="color: #999;">No approval steps found</div>';
        }

        return html;
    }


    UniversalTooltipService.registerTemplate('timelineApprovalTemplate', timelineApprovalTemplate);

    

    //#endregion

    //#region Table sort and Pagination

    $("#incrementApprovalTable .sort").on("click", function () {
        const column = $(this).data("sort");
        if (pendingSortColumn === column) {
            pendingSortDirection = pendingSortDirection === "asc" ? "desc" : "asc";
        } else {
            pendingSortColumn = column;
            pendingSortDirection = "asc";
        }
        pendingPage = 1;
        loadPendingIncrements(pendingPage);
    });

    $("#approvedIncrementTable .sort").on("click", function () {
        const column = $(this).data("sort");
        if (approvedSortColumn === column) {
            approvedSortDirection = approvedSortDirection === "asc" ? "desc" : "asc";
        } else {
            approvedSortColumn = column;
            approvedSortDirection = "asc";
        }
        approvedPage = 1;
        loadApprovedIncrements(approvedPage);
    });


    $('#searchInputAp').on('input', debounce(function () {
        loadApprovedIncrements(1);
    }, 500));

    $('#searchInputPe').on('input', debounce(function () {
        loadPendingIncrements(1);
    }, 500));

    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    function updatePagination(containerId, totalPages, totalItems, currentPage, loadFunction) {
        const pagination = $(containerId).find(".pagination");
        pagination.empty();

        pagination.append(`
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <button class="page-link" data-list-pagination="prev">Previous</button>
            </li>
        `);

        for (let i = 1; i <= totalPages; i++) {
            pagination.append(`
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <button class="page-link" data-page="${i}">${i}</button>
                </li>
            `);
        }

        pagination.append(`
            <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <button class="page-link" data-list-pagination="next">Next</button>
            </li>
        `);

        const startItem = (currentPage - 1) * pageSize + 1;
        const endItem = Math.min(currentPage * pageSize, totalItems);
        $(containerId).find("[data-list-info]").text(`Showing ${startItem} to ${endItem} of ${totalItems} entries`);
    }

    //#endregion

    //#region get by id  and Populate

    $(document).on("click", ".review-increment", function () {
        const incrementId = $(this).data("increment-id");
        $.ajax({
            url: `/IncrementApprove/GetIncrementDetails/${incrementId}`,
            method: "GET",
            success: function (data) {
                $("#increment_approval_modal").find("input[value='Hasan Shikder']").val(data.employeeName);
                $("#increment_approval_modal").find("input[value='Development']").val(data.department);
                $("#increment_approval_modal").find("input[value='Annual Increment']").val(data.incrementType);
                $("#increment_approval_modal").find("input[value='01 Jan 2024']").val(data.effectiveDate);
                $("#increment_approval_modal").find("input[value='৳45,000']").val(data.currentSalary);
                $("#increment_approval_modal").find("input[value='৳50,000']").val(data.proposedSalary);
                $("#increment_approval_modal").find("input[value='৳5,000']").val(data.incrementAmount);
                $("#increment_approval_modal").find("textarea[readonly]").val(data.justification);
                $("#increment_approval_modal").find("textarea[placeholder='Add your comments here...']").val("");
                $("#increment_approval_modal").find("[data-action='approve']").data("increment-id", incrementId);
                $("#increment_approval_modal").find("[data-action='decline']").data("increment-id", incrementId);
            },
            error: function () {
                console.error("Failed to load increment details");
            }
        });
    });

    //#endregion

    //#region approve and decline button 

    $(document).on("click", "[data-action='approve'], [data-action='decline']", function () {
        const action = $(this).data("action");
        const incrementId = $(this).data("increment-id");
        $("#confirmation_action").text(action.charAt(0).toUpperCase() + action.slice(1));
        $("#confirm_action").data("action", action);
        $("#confirm_action").data("increment-id", incrementId);

        const approvalModalEl = document.getElementById('increment_approval_modal');
        const approvalModal = bootstrap.Modal.getInstance(approvalModalEl);
        if (approvalModal) {
            approvalModal.hide();
        }
        $("#confirmation_modal").modal("show");
    });

    $("#confirm_action").on("click", function () {
        const action = $(this).data("action");
        const incrementId = $(this).data("increment-id");
        const comments = $("#increment_approval_modal").find("textarea[placeholder='Add your comments here...']").val();

        const formData = new FormData();
        formData.append("incrementId", incrementId);
        formData.append("action", action);
        formData.append("comments", comments);

        $.ajax({
            url: "/IncrementApprove/PerformIncrementAction",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    loadPendingIncrements(pendingPage);
                    loadApprovedIncrements(approvedPage);
                    loadIncrementCards();
                    toastr.success(response.message || `Increment ${action}d successfully!`);
                } else {
                    toastr.warning(response.message || `Increment ${action}d failed!`);
                }
                $("#confirmation_modal").modal("hide");
            },
            error: function () {
                toastr.error(`Failed to ${action} increment`);
                $("#confirmation_modal").modal("hide");
                console.error(`Failed to ${action} increment`);
            }
        });
    });

    //#endregion

    //#region export

    $(".export-pdf, .export-excel").on("click", function () {
        const format = $(this).hasClass("export-pdf") ? "pdf" : "excel";
        const formData = new FormData();
        formData.append("format", format);

        $.ajax({
            url: `/IncrementApprove/ExportIncrements`,
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            xhrFields: {
                responseType: "blob"
            },
            success: function (data) {
                const blob = new Blob([data]);
                const link = document.createElement("a");
                link.href = window.URL.createObjectURL(blob);
                link.download = `increments.${format}`;
                link.click();
            },
            error: function () {
                console.error(`Failed to export as ${format}`);
            }
        });
    });

    //#endregion

    //#region On change load increment table

    $("#incrementType, #statusSelect, #timepicker2").on("change", function () {
        pendingPage = 1;
        loadPendingIncrements(pendingPage);
    });

    $("#approvedDepartment, #approvedEmployee, #approvedIncrementType, #approvedDateRange").on("change", function () {
        approvedPage = 1;
        loadApprovedIncrements(approvedPage);
    });

    $(document).on("click", "#incrementApprovalTable .page-link", function () {
        const page = $(this).data("page") || ($(this).text() === "Previous" ? pendingPage - 1 : pendingPage + 1);
        loadPendingIncrements(page);
    });

    $(document).on("click", "#approvedIncrementTable .page-link", function () {
        const page = $(this).data("page") || ($(this).text() === "Previous" ? approvedPage - 1 : approvedPage + 1);
        loadApprovedIncrements(page);
    });

    //#endregion

    loadIncrementCards();
    loadPendingIncrements(pendingPage);
    loadApprovedIncrements(approvedPage);
});