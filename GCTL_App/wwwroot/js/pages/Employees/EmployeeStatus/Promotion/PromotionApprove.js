$(document).ready(function () {
    const developmentMode = true;

    if (developmentMode) {
        toastr.info("Welcome to the Promotion Approval Page!");
    }

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



    //new Choices("#promotionType", { removeItemButton: true, placeholder: true });
    //new Choices("#statusSelect", { removeItemButton: true, placeholder: true });
    //new Choices("#sortBy", { removeItemButton: true, placeholder: true });
    //new Choices("#approvedDepartment", { removeItemButton: true, placeholder: true });
    //new Choices("#approvedEmployee", { removeItemButton: true, placeholder: true });
    //new Choices("#approvedPromotionType", { removeItemButton: true, placeholder: true });
    //new Choices("#approvedSort", { removeItemButton: true, placeholder: true });

    // Variables for pagination
    let pendingPage = 1;
    let approvedPage = 1;
    const pageSize = 10;

    let pendingSortColumn = "employeeName"; // Default sort column for pending table
    let pendingSortDirection = "asc"; // Default sort direction for pending table
    let approvedSortColumn = "employeeName"; // Default sort column for approved table
    let approvedSortDirection = "asc"; // Default sort direction for approved table

    // Load promotion cards
    function loadPromotionCards() {
        $.ajax({
            url: "/PromotionApprove/GetPromotionCards",
            method: "GET",
            success: function (data) {
                const cardsContainer = $("#promotionCards");
                cardsContainer.empty();
                const cards = [
                    { title: "Senior Promotion", count: data.seniorCount, pending: data.seniorPending, bg: "bg-black-le", icon: "trending-up" },
                    { title: "Team Lead Promotion", count: data.teamLeadCount, pending: data.teamLeadPending, bg: "bg-blue-le", icon: "users" },
                    { title: "Manager Promotion", count: data.managerCount, pending: data.managerPending, bg: "bg-purple-le", icon: "crown" },
                    { title: "Department Head", count: data.deptHeadCount, pending: data.deptHeadPending, bg: "bg-pink-le", icon: "award" }
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
                console.error("Failed to load promotion cards");
            }
        });
    }

    // Load pending promotions
    function loadPendingPromotions(page = 1) {
        const formData = new FormData();
        formData.append("page", page);
        formData.append("pageSize", pageSize);
        formData.append("promotionType", $("#promotionType").val() || "");
        formData.append("status", $("#statusSelect").val() || "");
        formData.append("sortBy", $("#sortBy").val() || "");
        formData.append("dateRange", $("#timepicker2").val() || "");

        formData.append("sortColumn", pendingSortColumn);
        formData.append("sortDirection", pendingSortDirection);

        $.ajax({
            url: "/PromotionApprove/GetPendingPromotions",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {
                const tbody = $("#pending-promotion-body");
                tbody.empty();
                data.promotions.forEach(promotion => {
               
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-0">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${promotion.avatarUrl}" alt="" />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold">${promotion.employeeName}</h6>
                                        <span class="fs-12 fw-normal">${promotion.department}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="currentPosition align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <p class="fs-14 fw-medium d-flex align-items-center mb-0">${promotion.currentPosition}</p>
                            </td>
                            <td class="proposedPosition align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.proposedPosition}</td>
                            <td class="currentSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.currentSalary}</td>
                            <td class="proposedSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.proposedSalary}</td>
                            <td class="effectiveDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.effectiveDate}</td>
                            <td class="decision align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <a href="#" class="nav-item mx-2 review-promotion" data-promotion-id="${promotion.id}" data-bs-toggle="modal" data-bs-target="#promotion_approval_modal" title="Review Promotion">
                                    <i class="fas fa-eye text-primary"></i>
                                </a>
                            </td>
                        </tr>
                    `);
                });
                // updatePagination("#pending-promotion-pagination", data.totalPages, page, loadPendingPromotions);
                updatePagination("#promotionApprovalTable", data.totalPages, data.totalItems, page, loadPendingPromotions);
                pendingPage = page;
            },
            error: function () {
                console.error("Failed to load pending promotions");
            }
        });
    }

    // Load approved promotions
    function loadApprovedPromotions(page = 1) {
        const formData = new FormData();
        formData.append("page", page);
        formData.append("pageSize", pageSize);
        formData.append("department", $("#approvedDepartment").val() || "");
        formData.append("employee", $("#approvedEmployee").val() || "");
        formData.append("promotionType", $("#approvedPromotionType").val() || "");
        formData.append("sortBy", $("#approvedSort").val() || "");
        formData.append("dateRange", $("#approvedDateRange").val() || "");

        formData.append("sortColumn", approvedSortColumn);
        formData.append("sortDirection", approvedSortDirection);

        $.ajax({
            url: "/PromotionApprove/GetApprovedPromotions",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {
                const tbody = $("#approved-promotion-body");
                tbody.empty();
                data.promotions.forEach(promotion => {
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-0">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${promotion.avatarUrl}" alt="" />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold">${promotion.employeeName}</h6>
                                        <span class="fs-12 fw-normal">${promotion.department}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="currentPosition align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <p class="fs-14 fw-medium d-flex align-items-center mb-0">${promotion.currentPosition}</p>
                            </td>
                            <td class="proposedPosition align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.proposedPosition}</td>
                            <td class="currentSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.currentSalary}</td>
                            <td class="proposedSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.proposedSalary}</td>
                            <td class="effectiveDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${promotion.effectiveDate}</td>
                            <td class="decision align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <a href="#" class="nav-item mx-2 review-promotion" data-promotion-id="${promotion.id}" data-bs-toggle="modal" data-bs-target="#promotion_approval_modal" title="Review Promotion">
                                    <i class="fas fa-eye text-primary"></i>
                                </a>
                            </td>
                        </tr>
                    `);
                });
                //updatePagination("#approved-promotion-pagination", data.totalPages, page, loadApprovedPromotions);
                updatePagination("#approvedPromotionTable", data.totalPages, data.totalItems, page, loadApprovedPromotions);
                approvedPage = page;
            },
            error: function () {
                console.error("Failed to load approved promotions");
            }
        });
    }

    // Handle column sorting for pending promotions
    $("#promotionApprovalTable .sort").on("click", function () {
        const column = $(this).data("sort");
        if (pendingSortColumn === column) {
            pendingSortDirection = pendingSortDirection === "asc" ? "desc" : "asc";
        } else {
            pendingSortColumn = column;
            pendingSortDirection = "asc";
        }
        pendingPage = 1; // Reset to first page on sort
        loadPendingPromotions(pendingPage);
    });

    // Handle column sorting for approved promotions
    $("#approvedPromotionTable .sort").on("click", function () {
        const column = $(this).data("sort");
        if (approvedSortColumn === column) {
            approvedSortDirection = approvedSortDirection === "asc" ? "desc" : "asc";
        } else {
            approvedSortColumn = column;
            approvedSortDirection = "asc";
        }
        approvedPage = 1; // Reset to first page on sort
        loadApprovedPromotions(approvedPage);
    });

    function updatePagination(containerId, totalPages, totalItems, currentPage, loadFunction) {
        const pagination = $(containerId).find(".pagination");
        pagination.empty();
        // Add Previous button
        pagination.append(`
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <button class="page-link" data-list-pagination="prev">Previous</button>
        </li>
    `);
        // Add page numbers
        for (let i = 1; i <= totalPages; i++) {
            pagination.append(`
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <button class="page-link" data-page="${i}">${i}</button>
            </li>
        `);
        }
        // Add Next button
        pagination.append(`
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <button class="page-link" data-list-pagination="next">Next</button>
        </li>
    `);
        // Update showing info
        const startItem = (currentPage - 1) * pageSize + 1;
        const endItem = Math.min(currentPage * pageSize, totalItems);
        $(containerId).find("[data-list-info]").text(`Showing ${startItem} to ${endItem} of ${totalItems} entries`);
    }

    // Load promotion details for modal
    $(document).on("click", ".review-promotion", function () {
        const promotionId = $(this).data("promotion-id");
        $.ajax({
            url: `/PromotionApprove/GetPromotionDetails/${promotionId}`,
            method: "GET",
            success: function (data) {
                $("#promotion_approval_modal").find("input[value='Hasan Shikder']").val(data.employeeName);
                $("#promotion_approval_modal").find("input[value='Development']").val(data.department);
                $("#promotion_approval_modal").find("input[value='Junior Developer']").val(data.currentPosition);
                $("#promotion_approval_modal").find("input[value='Senior Developer']").val(data.proposedPosition);
                $("#promotion_approval_modal").find("input[value='01 Jan 2024']").val(data.effectiveDate);
                $("#promotion_approval_modal").find("input[value='3 Years']").val(data.yearsOfExperience);
                $("#promotion_approval_modal").find("input[value='৳45,000']").val(data.currentSalary);
                $("#promotion_approval_modal").find("input[value='৳55,000']").val(data.proposedSalary);
                $("#promotion_approval_modal").find("textarea[readonly]").val(data.justification);
                $("#promotion_approval_modal").find("textarea[placeholder='Add your comments here...']").val("");
                $("#promotion_approval_modal").find("[data-action='approve']").data("promotion-id", promotionId);
                $("#promotion_approval_modal").find("[data-action='decline']").data("promotion-id", promotionId);
            },
            error: function () {
                console.error("Failed to load promotion details");
            }
        });
    });

    // Handle approve/decline button clicks
    $(document).on("click", "[data-action='approve'], [data-action='decline']", function () {
        const action = $(this).data("action");
        const promotionId = $(this).data("promotion-id");
        $("#confirmation_action").text(action.charAt(0).toUpperCase() + action.slice(1));
        $("#confirm_action").data("action", action);
        $("#confirm_action").data("promotion-id", promotionId);
        $("#confirmation_modal").modal("show");
    });

    // Handle confirmation modal OK button
    $("#confirm_action").on("click", function () {
        const action = $(this).data("action");
        const promotionId = $(this).data("promotion-id");
        const comments = $("#promotion_approval_modal").find("textarea[placeholder='Add your comments here...']").val();

        const formData = new FormData();
        formData.append("promotionId", promotionId);
        formData.append("action", action);
        formData.append("comments", comments);

        $.ajax({
            url: "/PromotionApprove/PerformPromotionAction",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                $("#confirmation_modal").modal("hide");
                $("#promotion_approval_modal").modal("hide");
                loadPendingPromotions(pendingPage);
                loadApprovedPromotions(approvedPage);
                loadPromotionCards();
            },
            error: function () {
                console.error(`Failed to ${action} promotion`);
            }
        });
    });

    // Handle export buttons
    $(".export-pdf, .export-excel").on("click", function () {
        const format = $(this).hasClass("export-pdf") ? "pdf" : "excel";
        const formData = new FormData();
        formData.append("format", format);
        // Add filters if needed
        $.ajax({
            url: `/PromotionApprove/ExportPromotions`,
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
                link.download = `promotions.${format}`;
                link.click();
            },
            error: function () {
                console.error(`Failed to export as ${format}`);
            }
        });
    });

    //// Handle filter changes
    //$("#promotionType, #statusSelect, #sortBy, #timepicker2").on("change", function () {
    //    pendingPage = 1;
    //    loadPendingPromotions(pendingPage);
    //});

    //$("#approvedDepartment, #approvedEmployee, #approvedPromotionType, #approvedSort, #approvedDateRange").on("change", function () {
    //    approvedPage = 1;
    //    loadApprovedPromotions(approvedPage);
    //});


    $("#promotionType, #statusSelect, #timepicker2").on("change", function () {
        pendingPage = 1;
        loadPendingPromotions(pendingPage);
    });

    $("#approvedDepartment, #approvedEmployee, #approvedPromotionType, #approvedDateRange").on("change", function () {
        approvedPage = 1;
        loadApprovedPromotions(approvedPage);
    });


    // Handle pagination clicks
    $(document).on("click", "#pending-promotion-pagination .page-link", function () {
        const page = $(this).data("page") || (this.innerText === "Previous" ? pendingPage - 1 : pendingPage + 1);
        loadPendingPromotions(page);
    });

    $(document).on("click", "#approved-promotion-pagination .page-link", function () {
        const page = $(this).data("page") || (this.innerText === "Previous" ? approvedPage - 1 : approvedPage + 1);
        loadApprovedPromotions(page);
    });

    // Initial load
    loadPromotionCards();
    loadPendingPromotions(pendingPage);
    loadApprovedPromotions(approvedPage);
});