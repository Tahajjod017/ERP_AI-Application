$(document).ready(function () {

    showDev('im loaded')

    const initializeSelect = () => {
        $('.searchableSelect ').select2({
            width: '100%',
            allowClear: true,
            placeholder: 'Select an option',
            language: { noResults: () => 'No results found' },
            escapeMarkup: markup => markup
        });
    };
    initializeSelect();


    let isEditMode = false;
    let currentPOItems = [];

    //#region Initialize
    function init() {
        loadOpenPOs();
        getNextPRNumber();
        setPRDate();
        loadPurchaseReceives();
    }

    function setPRDate() {
        $("#pr_PRDate").val(new Date().toISOString().split('T')[0]);
    }

    function getNextPRNumber() {
        $.ajax({
            url: '/PurchaseReceive/GetNextPRNumber',
            method: 'GET',
            success: function (prNumber) {
                $("#pr_PRNumber").val(prNumber);
            }
        });
    }

    function loadOpenPOs() {
        $.ajax({
            url: '/PurchaseReceive/GetOpenPurchaseOrders',
            method: 'GET',
            success: function (pos) {
                const $dropdown = $("#pr_PurchaseOrderVersionID");
                $dropdown.find('option:not(:first)').remove();

                $.each(pos, function (i, po) {
                    $dropdown.append(`<option value="${po.purchaseOrderVersionID}">${po.poNumber} - ${po.supplierName}</option>`);
                });

                if (typeof choiceManager !== 'undefined') {
                    choiceManager.refreshChoice('pr_PurchaseOrderVersionID');
                }
            }
        });
    }
    //#endregion

    //#region PO Selection
    $("#pr_PurchaseOrderVersionID").on("change", function () {
        const poVersionId = $(this).val();

        if (!poVersionId) {
            $("#poInfoSection").hide();
            $("#productsSection").hide();
            return;
        }

        $.ajax({
            url: '/PurchaseReceive/GetPODetailsForReceive',
            method: 'GET',
            data: { poId: poVersionId },
            success: function (response) {
                if (response.success) {
                    populatePOInfo(response.data);
                    populateProducts(response.data.items);
                } else {
                    toastr.error("Failed to load PO details");
                }
            }
        });
    });

    function populatePOInfo(data) {
        $("#po_Number").text(data.poNumber);
        $("#po_Supplier").text(data.supplierName);
        $("#po_Date").text(formatDate(data.purchaseDate));
        $("#po_TotalItems").text(data.items.length);
        $("#poInfoSection").show();
    }

    function populateProducts(items) {
        currentPOItems = items;
        let html = '';

        $.each(items, function (i, item) {
            if (item.isFullyReceived) return; // Skip fully received items

            html += `
                <tr data-item-id="${item.poItemVersionID}">
                    <td class="text-center">
                        <input type="checkbox" class="item-checkbox" checked />
                        <input type="hidden" name="Items[${i}].POItemVersionID" value="${item.poItemVersionID}" />
                        <input type="hidden" name="Items[${i}].ProductID" value="${item.productID}" />
                        <input type="hidden" name="Items[${i}].POQuantity" value="${item.orderedQuantity}" />
                    </td>
                    <td>${item.productName}</td>
                    <td>${item.brand}</td>
                    <td>${item.unit}</td>
                    <td>${item.orderedQuantity}</td>
                    <td class="text-success">${item.receivedQuantity}</td>
                    <td class="text-warning fw-bold">${item.remainingQuantity}</td>
                    <td>
                        <input type="number" 
                               name="Items[${i}].ReceiveQuantity" 
                               class="form-control form-control-sm receive-qty" 
                               value="${item.remainingQuantity}" 
                               min="0" 
                               step="0.01" 
                               data-max="${item.remainingQuantity}"
                               required />
                    </td>
                    <td>
                        <input type="number" 
                               name="Items[${i}].AcceptedQuantity" 
                               class="form-control form-control-sm accepted-qty" 
                               value="${item.remainingQuantity}" 
                               min="0" 
                               step="0.01" 
                               required />
                    </td>
                    <td>
                        <input type="number" 
                               name="Items[${i}].RejectedQuantity" 
                               class="form-control form-control-sm rejected-qty" 
                               value="0" 
                               min="0" 
                               step="0.01" />
                    </td>
                </tr>`;
        });

        $("#receiveItemsBody").html(html);
        $("#productsSection").show();
        calculateTotals();
    }
    //#endregion

    //#region Calculations
    function calculateTotals() {
        let totalReceive = 0;
        let totalAccepted = 0;
        let totalRejected = 0;

        $("#receiveItemsBody tr").each(function () {
            if (!$(this).find(".item-checkbox").is(":checked")) return;

            const receive = parseFloat($(this).find(".receive-qty").val()) || 0;
            const accepted = parseFloat($(this).find(".accepted-qty").val()) || 0;
            const rejected = parseFloat($(this).find(".rejected-qty").val()) || 0;

            totalReceive += receive;
            totalAccepted += accepted;
            totalRejected += rejected;
        });

        $("#total_Receive").text(totalReceive.toFixed(2));
        $("#total_Accepted").text(totalAccepted.toFixed(2));
        $("#total_Rejected").text(totalRejected.toFixed(2));
    }

    $(document).on("input", ".receive-qty, .accepted-qty, .rejected-qty", calculateTotals);

    // Auto-calculate accepted/rejected from receive qty
    $(document).on("input", ".receive-qty", function () {
        const $row = $(this).closest("tr");
        const receiveQty = parseFloat($(this).val()) || 0;
        const max = parseFloat($(this).data("max")) || 0;

        if (receiveQty > max) {
            toastr.warning(`Warning: Receiving more than remaining quantity (${max})`);
        }

        // Auto-fill accepted = receive, rejected = 0
        $row.find(".accepted-qty").val(receiveQty);
        $row.find(".rejected-qty").val(0);
        calculateTotals();
    });

    // Validate accepted + rejected = receive
    $(document).on("input", ".accepted-qty, .rejected-qty", function () {
        const $row = $(this).closest("tr");
        const receive = parseFloat($row.find(".receive-qty").val()) || 0;
        const accepted = parseFloat($row.find(".accepted-qty").val()) || 0;
        const rejected = parseFloat($row.find(".rejected-qty").val()) || 0;

        if (Math.abs((accepted + rejected) - receive) > 0.01) {
            $row.addClass("table-danger");
        } else {
            $row.removeClass("table-danger");
        }
    });

    // Select All
    $("#selectAll").on("change", function () {
        $(".item-checkbox").prop("checked", $(this).is(":checked"));
        calculateTotals();
    });

    $(document).on("change", ".item-checkbox", function () {
        calculateTotals();
    });
    //#endregion

    //#region Form Submit
    $("#purchaseReceiveForm").on("submit", function (e) {
        e.preventDefault();

        // Validate at least one item selected
        if ($(".item-checkbox:checked").length === 0) {
            toastr.warning("Please select at least one item to receive");
            return;
        }

        // Validate accepted + rejected = receive
        let isValid = true;
        $("#receiveItemsBody tr").each(function () {
            if (!$(this).find(".item-checkbox").is(":checked")) return;

            const receive = parseFloat($(this).find(".receive-qty").val()) || 0;
            const accepted = parseFloat($(this).find(".accepted-qty").val()) || 0;
            const rejected = parseFloat($(this).find(".rejected-qty").val()) || 0;

            if (Math.abs((accepted + rejected) - receive) > 0.01) {
                toastr.error("Accepted + Rejected must equal Receive Quantity for all items");
                isValid = false;
                return false;
            }
        });

        if (!isValid) return;

        // Remove unchecked items from form data
        $("#receiveItemsBody tr").each(function () {
            if (!$(this).find(".item-checkbox").is(":checked")) {
                $(this).find("input").prop("disabled", true);
            }
        });

        const formData = $(this).serialize();
        const url = isEditMode ? '/PurchaseReceive/Edit' : '/PurchaseReceive/Create';

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    clearForm();
                    loadPurchaseReceives();
                } else {
                    toastr.warning(res.message || "Operation failed");
                }
            },
            error: function () {
                toastr.error("An error occurred");
            },
            complete: function () {
                // Re-enable all inputs
                $("#receiveItemsBody tr input").prop("disabled", false);
            }
        });
    });
    //#endregion

    //#region Clear Form
    $("#clearFormBtn").on("click", function () {
        clearForm();
    });

    function clearForm() {
        $("#purchaseReceiveForm")[0].reset();
        $("#pr_PurchaseReceiveID").val('');
        $("#poInfoSection").hide();
        $("#productsSection").hide();
        $("#receiveItemsBody").empty();
        isEditMode = false;
        $("#saveBtn").text("Save Purchase Receive");
        getNextPRNumber();
        setPRDate();
        loadOpenPOs();
    }
    //#endregion

    //#region Purchase Receives List
    let page = 1;
    let pageSize = 10;
    let search = "";
    let sortColumn = "PurchaseReceiveID";
    let sortDirection = "desc";
    let statusId = "";
    let supplierId = "";
    let fromDate = "";
    let toDate = "";

    function loadPurchaseReceives() {
        $.ajax({
            url: '/PurchaseReceive/GetPurchaseReceives',
            data: {
                page, pageSize, search, sortColumn, sortDirection,
                statusId, supplierId, fromDate, toDate
            },
            success: function (res) {
                let rows = '';
                $.each(res.data, function (i, item) {
                    const editBtn = item.canEdit
                        ? `<a href="#" class="nav-item me-2 editBtn" data-id="${item.purchaseReceiveID}" title="Edit">
                              <i class="fas fa-edit text-black"></i>
                           </a>`
                        : '';

                    const deleteBtn = item.canDelete
                        ? `<a href="#" class="nav-item me-2 deleteBtn" data-id="${item.purchaseReceiveID}" 
                              data-bs-toggle="modal" data-bs-target="#deleteModal" title="Delete">
                              <i class="far fa-trash-alt text-black"></i>
                           </a>`
                        : '';

                    rows += `
                        <tr>
                            <td class="ps-0">#${item.purchaseReceiveID}</td>
                            <td class="ps-2">${item.prNumber}</td>
                            <td class="ps-2">${formatDate(item.prDate)}</td>
                            <td class="ps-2">${item.poNumber}</td>
                            <td class="ps-2">${item.supplierName}</td>
                            <td class="ps-2">${item.totalItems}</td>
                            <td class="ps-2">${item.totalReceivedQty}</td>
                            <td class="ps-2"><span class="badge badge-phoenix-primary">${item.status}</span></td>
                            <td class="text-end">
                                <a href="#" class="nav-item me-2 viewBtn" data-id="${item.purchaseReceiveID}" 
                                   data-bs-toggle="modal" data-bs-target="#viewDetailsModal" title="View">
                                    <i class="fas fa-eye text-black"></i>
                                </a>
                                ${editBtn}
                                ${deleteBtn}
                            </td>
                        </tr>`;
                });
                $("#purchase-receives-table").html(rows);

                DynamicTableDrag.refreshTableSettings('PurchaseReceives');
                renderPagination(res.totalRecords, page, pageSize);
                $("[data-list-info='list-info']").text(
                    `Showing ${(page - 1) * pageSize + 1} to ${Math.min(page * pageSize, res.totalRecords)} of ${res.totalRecords}`
                );
            }
        });
    }

    // Filters and Search
    $("#searchInput").on("keyup", function () {
        search = $(this).val();
        page = 1;
        loadPurchaseReceives();
    });

    $("#pageSizeSelect, #statusFilter, #supplierFilter").on("change", function () {
        pageSize = $("#pageSizeSelect").val();
        statusId = $("#statusFilter").val();
        supplierId = $("#supplierFilter").val();
        page = 1;
        loadPurchaseReceives();
    });

    $("#fromDate, #toDate").on("change", function () {
        fromDate = $("#fromDate").val();
        toDate = $("#toDate").val();
        page = 1;
        loadPurchaseReceives();
    });

    // Pagination
    $(document).on("click", "#pagination .page-link", function (e) {
        e.preventDefault();
        page = parseInt($(this).text());
        loadPurchaseReceives();
    });

    $("#prevBtn").on("click", function (e) {
        e.preventDefault();
        if (page > 1) { page--; loadPurchaseReceives(); }
    });

    $("#nextBtn").on("click", function (e) {
        e.preventDefault();
        page++; loadPurchaseReceives();
    });

    // Sorting
    $("#PurchaseReceives th.sort").on("click", function () {
        sortColumn = $(this).data("sort");
        sortDirection = sortDirection === "asc" ? "desc" : "asc";
        loadPurchaseReceives();
    });
    //#endregion

    //#region View Details
    $(document).on("click", ".viewBtn", function () {
        const id = $(this).data('id');

        $.ajax({
            url: '/PurchaseReceive/GetReceiveDetails',
            data: { id },
            success: function (response) {
                if (response.success) {
                    populateViewModal(response.data);
                }
            }
        });
    });

    function populateViewModal(data) {
        $("#view_PRNumber").text(data.prNumber);
        $("#view_PRDate").text(formatDate(data.prDate));
        $("#view_PONumber").text(data.poNumber);
        $("#view_Supplier").text(data.supplierName);
        $("#view_VendorBill").text(data.vendorBillChalan || 'N/A');
        $("#view_ReceivedBy").text(data.receivedByName);
        $("#view_Status").html(`<span class="badge badge-phoenix-primary">${data.status}</span>`);
        $("#view_Note").text(data.prNote || 'N/A');

        let itemsHtml = '';
        $.each(data.items, function (i, item) {
            itemsHtml += `
                <tr>
                    <td>${item.productName}</td>
                    <td>${item.brand}</td>
                    <td>${item.unit}</td>
                    <td>${item.poQuantity}</td>
                    <td>${item.receiveQuantity}</td>
                    <td class="text-success">${item.acceptedQuantity}</td>
                    <td class="text-danger">${item.rejectedQuantity}</td>
                </tr>`;
        });
        $("#view_ItemsTable").html(itemsHtml);
    }
    //#endregion

    //#region Delete
    $(document).on("click", ".deleteBtn", function () {
        $("#deleteReceiveId").val($(this).data('id'));
    });

    window.confirmDelete = function () {
        const id = $("#deleteReceiveId").val();

        $.ajax({
            url: '/PurchaseReceive/Delete',
            type: 'POST',
            data: {
                id: id,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    $("#deleteModal").modal('hide');
                    loadPurchaseReceives();
                } else {
                    toastr.warning(res.message);
                }
            }
        });
    };
    //#endregion

    //#region Helper Functions
    function formatDate(dateStr) {
        if (!dateStr) return 'N/A';
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    }

    function renderPagination(totalRecords, currentPage, pageSize) {
        const totalPages = Math.ceil(totalRecords / pageSize);
        let html = '';

        for (let i = 1; i <= totalPages; i++) {
            html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#">${i}</a>
            </li>`;
        }

        $("#pagination").html(html);
    }
    //#endregion

    // Initialize on load
    init();
});
