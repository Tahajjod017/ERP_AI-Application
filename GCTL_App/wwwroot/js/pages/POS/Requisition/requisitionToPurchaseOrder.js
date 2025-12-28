$(document).ready(function () {

    //#region Table Variables and Functions
    let page = 1;
    let pageSize = 10;
    let search = "";
    let sortColumn = "RequisitionId";
    let sortDirection = "desc";
    let productTypeId = "";
    let fromDate = "";
    let toDate = "";

    function loadApprovedRequisitions() {
        $.ajax({
            url: '/RequisitionToPurchaseOrder/GetApprovedRequisitions',
            data: {
                page: page,
                pageSize: pageSize,
                search: search,
                sortColumn: sortColumn,
                sortDirection: sortDirection,
                productTypeId: productTypeId,
                fromDate: fromDate,
                toDate: toDate
            },
            success: function (res) {
                showDev(res, 'Approved Requisitions');

                let rows = '';
                $.each(res.data, function (i, item) {
                    let statusBadge = item.hasPurchaseOrder
                        ? '<span class="badge badge-phoenix badge-phoenix-success">Converted</span>'
                        : '<span class="badge badge-phoenix badge-phoenix-info">Ready for PO</span>';

                    let convertBtn = item.hasPurchaseOrder
                        ? `<span class="text-muted" title="Already converted">
                              <i class="fas fa-check-circle"></i> Converted
                           </span>`
                        : `<a href="#" class="nav-item me-2 convertToPOBtn" 
                              data-id="${item.requisitionId}"
                              data-bs-toggle="modal" 
                              data-bs-target="#convertToPOModal"
                              title="Convert to PO">
                              <i class="fas fa-file-invoice text-black"></i>

                           </a>`;

                    rows += `
                        <tr class="position-static">
                            <td class="align-middle white-space-nowrap ps-0">#${item.requisitionId}</td>
                            <td class="align-middle white-space-nowrap ps-2">
                            <a href="#" class="nav-item me-2 viewDetailsBtn"
                                       data-id="${item.requisitionId}"
                                       data-bs-toggle="modal" 
                                       data-bs-target="#viewDetailsModal"
                                       title="View Details">
                                        ${item.requisitionCode}
                                       

                                    </a>
                           
                            
                            </td>
                            <td class="align-middle white-space-nowrap ps-2">${formatDate(item.requisitionDate)}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.requisitionBy}</td>
                            <td class="align-middle white-space-nowrap ps-2">${item.totalItems}</td>
                            <td class="align-middle white-space-nowrap ps-2">
                                <span class="badge badge-phoenix ${getPriorityBadge(item.priority)}">${item.priority}</span>
                            </td>
                            <td class="align-middle white-space-nowrap ps-2">${statusBadge}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="d-flex justify-content-end">
                                    
                                    ${convertBtn}
                                </div>
                            </td>
                        </tr>`;
                });
                $("#approved-requisitions-table").html(rows);

                DynamicTableDrag.refreshTableSettings('ApprovedRequisitions');

                // Pagination
                renderPagination(res.totalRecords, page, pageSize);

                // Info
                $("[data-list-info='list-info']").text(
                    `Showing ${(page - 1) * pageSize + 1} to ${Math.min(page * pageSize, res.totalRecords)} of ${res.totalRecords}`
                );
            }
        });
    }

    // Search
    $("#searchInput").on("keyup", function () {
        search = $(this).val();
        page = 1;
        loadApprovedRequisitions();
    });

    // Page size
    $("#pageSizeSelect").on("change", function () {
        pageSize = $(this).val();
        page = 1;
        loadApprovedRequisitions();
    });

    // Filters
    $("#productTypeFilter").on("change", function () {
        productTypeId = $(this).val();
        page = 1;
        loadApprovedRequisitions();
    });

    $("#fromDate, #toDate").on("change", function () {
        fromDate = $("#fromDate").val();
        toDate = $("#toDate").val();
        page = 1;
        loadApprovedRequisitions();
    });

    // Pagination
    $(document).on("click", "#pagination .page-link", function (e) {
        e.preventDefault();
        page = parseInt($(this).text());
        loadApprovedRequisitions();
    });

    $("#prevBtn").on("click", function (e) {
        e.preventDefault();
        if (page > 1) {
            page--;
            loadApprovedRequisitions();
        }
    });

    $("#nextBtn").on("click", function (e) {
        e.preventDefault();
        page++;
        loadApprovedRequisitions();
    });

    // Sorting
    $("#ApprovedRequisitions th.sort").on("click", function () {
        sortColumn = $(this).data("sort");
        sortDirection = sortDirection === "asc" ? "desc" : "asc";
        loadApprovedRequisitions();
    });

    //#endregion

    //#region View Details Modal
    $(document).on("click", ".viewDetailsBtn", function () {
        const id = $(this).data('id');

        $.ajax({
            url: '/RequisitionToPurchaseOrder/GetRequisitionDetails',
            type: "GET",
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    populateViewModal(response.data);
                } else {
                    toastr.error("Failed to load details");
                }
            },
            error: function () {
                toastr.error("An error occurred");
            }
        });
    });

    function populateViewModal(data) {
        $("#view_RequisitionCode").text(data.requisitionCode);
        $("#view_RequisitionDate").text(formatDate(data.requisitionDate));
        $("#view_RequisitionBy").text(data.requisitionBy);
        $("#view_Priority").text(data.priority);
        $("#view_Organization").text(data.organization);
        $("#view_Branch").text(data.branch);
        $("#view_RequisitionNote").text(data.requisitionNote || 'N/A');

        if (data.hasPurchaseOrder) {
            $("#view_POCode").text(data.purchaseOrderCode);
            $('#view_POCodeLink').attr('href', '/PurchaseOrderDetails/index/' + data.purchaseOrderVerId);
            $("#view_POSection").show();
        } else {
            $("#view_POSection").hide();
        }

        let productsHtml = '';
        $.each(data.items, function (i, item) {
            let amount = item.approvedQuantity * item.unitPrice;
            productsHtml += `
                <tr>
                    <td>${item.productType}</td>
                    <td>${item.productName}</td>
                    <td>${item.brand}</td>
                    <td>${item.unit}</td>
                    <td>${item.requestedQuantity}</td>
                    <td class="fw-bold">${item.approvedQuantity}</td>
                    <td>${item.unitPrice.toFixed(2)}</td>
                    <td class="fw-bold">${amount.toFixed(2)}</td>
                </tr>`;
        });
        $("#view_ProductsTable").html(productsHtml);
    }
    //#endregion

    //#region Convert to PO Modal
    let currentRequisitionData = null;

    $(document).on("click", ".convertToPOBtn", function () {
        const id = $(this).data('id');

        $.ajax({
            url: '/RequisitionToPurchaseOrder/GetRequisitionDetails',
            type: "GET",
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    if (response.data.hasPurchaseOrder) {
                        toastr.warning("This requisition has already been converted to a purchase order");
                        return;
                    }
                    currentRequisitionData = response.data;
                    populateConvertModal(response.data);
                } else {
                    toastr.error("Failed to load details");
                }
            },
            error: function () {
                toastr.error("An error occurred");
            }
        });
    });

    function populateConvertModal(data) {
        $("#po_RequisitionId").val(data.requisitionId);
        $("#po_RequisitionCode").text(data.requisitionCode);
        $("#po_TotalItems").text(data.items.length);

        // Set hidden organization and branch IDs from requisition data
        // Create hidden inputs if they don't exist
        if ($("#po_OrganizationId").length === 0) {
            $("#convertToPOForm").append(`<input type="hidden" id="po_OrganizationId" name="OrganizationId" />`);
        }
        if ($("#po_OrganizationBranchId").length === 0) {
            $("#convertToPOForm").append(`<input type="hidden" id="po_OrganizationBranchId" name="OrganizationBranchId" />`);
        }

        // These values should come from the requisition details
        // You may need to add these fields to the RequisitionDetailsForPOViewModel
        $("#po_OrganizationId").val(currentRequisitionData.organizationId || '');
        $("#po_OrganizationBranchId").val(currentRequisitionData.organizationBranchId || '');

        // Load suppliers
        loadSuppliers();

        // Load addresses (both billing and shipping)
        loadAddresses();

        // Get next PO code
        getNextPOCode();

        // Set dates
        const today = new Date().toISOString().split('T')[0];
        const dueDate = new Date();
        dueDate.setDate(dueDate.getDate() + 30);
        $("#po_PurchaseDate").val(today);
        $("#po_DueDate").val(dueDate.toISOString().split('T')[0]);

        // Reset other fields
        $("#po_OtherReference").val('');
        $("#po_WorkorderNo").val('');
        $("#po_WorkOrderDate").val('');
        $("#po_TaxPercent").val(0);
        $("#po_Note").val(data.requisitionNote || '');
        $("#po_TermsAndConditions").val('');
        $("#po_IsDraft").prop('checked', false);
        $("#po_StatusId").val('');

        // Populate products table
        let productsHtml = '';
        $.each(data.items, function (i, item) {
            let amount = item.approvedQuantity * item.unitPrice;
            productsHtml += `
                <tr data-item-id="${item.itemId}">
                    <td>
                        ${item.productName}
                        <input type="hidden" name="Items[${i}].RequisitionItemId" value="${item.itemId}" />
                        <input type="hidden" name="Items[${i}].ProductId" value="${item.productId}" />
                    </td>
                    <td>${item.brand}</td>
                    <td>${item.approvedQuantity}</td>
                    <td>
                        <input type="number" 
                               name="Items[${i}].Quantity" 
                               class="form-control form-control-sm po-quantity" 
                               value="${item.approvedQuantity}" 
                               min="0.01" 
                               step="0.01" 
                               required />
                    </td>
                    <td>
                        <input type="number" 
                               name="Items[${i}].UnitPrice" 
                               class="form-control form-control-sm po-unitprice" 
                               value="${item.unitPrice}" 
                               min="0.01" 
                               step="0.01" 
                               required />
                    </td>
                    <td class="po-amount">${amount.toFixed(2)}</td>
                </tr>`;
        });
        $("#po_ProductsTable").html(productsHtml);

        // Calculate totals
        calculatePOTotals();

        // Attach calculation events
        $(document).on("input", ".po-quantity, .po-unitprice, #po_TaxPercent", calculatePOTotals);
    }

    function calculatePOTotals() {
        let subTotal = 0;

        $("#po_ProductsTable tr").each(function () {
            const qty = parseFloat($(this).find(".po-quantity").val()) || 0;
            const price = parseFloat($(this).find(".po-unitprice").val()) || 0;
            const amount = qty * price;

            $(this).find(".po-amount").text(amount.toFixed(2));
            subTotal += amount;
        });

        const taxPercent = parseFloat($("#po_TaxPercent").val()) || 0;
        const taxAmount = (subTotal * taxPercent) / 100;
        const grandTotal = subTotal + taxAmount;

        $("#po_SubTotal").text(subTotal.toFixed(2));
        $("#po_TaxAmount").text(taxAmount.toFixed(2));
        $("#po_GrandTotal").text(grandTotal.toFixed(2));
    }

    function getNextPOCode() {
        $.ajax({
            url: '/RequisitionToPurchaseOrder/GetNextPOCode',
            method: 'GET',
            success: function (code) {
                $("#po_POCode").val(code);
                $("#po_POCode2").text(code);
            },
            error: function () {
                console.error('Failed to get next PO code');
            }
        });
    }

    function loadSuppliers() {
        $.ajax({
            url: '/RequisitionToPurchaseOrder/GetSuppliers',
            method: 'GET',
            success: function (suppliers) {
                showDev(suppliers, 'irsi d');
                choiceManager.populateDropdown('po_SupplierId', suppliers);
                //const $dropdown = $("#po_SupplierId");
                //$dropdown.find('option:not(:first)').remove();

                //$.each(suppliers, function (i, supplier) {
                //    $dropdown.append(`<option value="${supplier.id}">${supplier.companyName}</option>`);
                //});

                //// Reinitialize if using choice.js
                //if (typeof choiceManager !== 'undefined') {
                //    choiceManager.refreshChoice('po_SupplierId');
                //}
            },
            error: function () {
                console.error('Failed to load suppliers');
            }
        });
    }

    function loadAddresses() {
        $.ajax({
            url: '/RequisitionToPurchaseOrder/GetAddresses',
            method: 'GET',
            success: function (addresses) {
                showDev(addresses, '444')
                choiceManager.populateDropdown('po_ShippingAddressId', addresses);

                //const $billingDropdown = $("#po_BillingAddressId");
                //const $shippingDropdown = $("#po_ShippingAddressId");

                //$billingDropdown.find('option:not(:first)').remove();
                //$shippingDropdown.find('option:not(:first)').remove();

                //$.each(addresses, function (i, address) {
                //    const displayText = address.fullName + ' - ' + address.fullAddress.substring(0, 30) + '...';
                //    $billingDropdown.append(`<option value="${address.id}">${displayText}</option>`);
                //    $shippingDropdown.append(`<option value="${address.id}">${displayText}</option>`);
                //});

                //// Reinitialize if using choice.js
                //if (typeof choiceManager !== 'undefined') {
                //    choiceManager.refreshChoice('po_BillingAddressId');
                //    choiceManager.refreshChoice('po_ShippingAddressId');
                //}
            },
            error: function () {
                console.error('Failed to load addresses');
            }
        });
    }

    window.savePurchaseOrder = function () {
        // Validate
        if (!$("#po_SupplierId").val()) {
            toastr.warning("Please select a supplier");
            return;
        }

        if (!$("#po_POCode").val()) {
            toastr.warning("PO Code is required");
            return;
        }

        // Check if all items have quantity and price
        let isValid = true;
        $(".po-quantity, .po-unitprice").each(function () {
            const val = parseFloat($(this).val());
            if (isNaN(val) || val <= 0) {
                isValid = false;
                return false;
            }
        });

        if (!isValid) {
            toastr.warning("Please enter valid quantity and unit price for all items");
            return;
        }

        //const formData = $("#convertToPOForm").serialize();

        //let formData = $("#convertToPOForm").serialize();
        //let isDraft = $("#po_IsDraft").prop("checked");
        //formData += "&IsDraft=" + encodeURIComponent(isDraft);

        let formData = $("#convertToPOForm").serialize();
        formData += "&IsDraft=" + ($("#po_IsDraft").prop("checked") ? "true" : "false");

        $.ajax({
            url: '/RequisitionToPurchaseOrder/ConvertToPurchaseOrder',
            type: 'POST',
            data: formData,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    hideModal('convertToPOModal');
                    loadApprovedRequisitions();

                    // Optionally redirect to PO details page
                    if (res.data) {
                        setTimeout(function () {
                            window.location.href = '/PurchaseOrderDetails/Index/' + res.data;
                        }, 1500);
                    }
                } else {
                    toastr.warning(res.message || "Conversion failed");
                }
            },
            error: function () {
                toastr.error("An error occurred while creating purchase order");
            }
        });
    };

    //#endregion

    //#region Add Supplier
    $("#saveNewSupplierBtn").on("click", function () {
        const supplier = {
            companyName: $("#newCompanyName").val().trim(),
            contactName: $("#newContactName").val().trim(),
            email: $("#newEmail").val().trim(),
            phone: $("#newPhone").val().trim(),
            addressLine1: $("#newAddress1").val().trim(),
            taxNumber: $("#newTaxNumber").val().trim()
        };

        if (!supplier.companyName) {
            toastr.warning("Company name is required");
            return;
        }

        $.ajax({
            url: '/RequisitionToPurchaseOrder/AddSupplier',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(supplier),
            success: function (newSupplier) {
                $("#po_SupplierId").append(
                    `<option value="${newSupplier.id}">${newSupplier.companyName}</option>`
                );
                $("#po_SupplierId").val(newSupplier.id);

                hideModal('addSupplierModal');
                $("#addSupplierModal input").val('');
                toastr.success("Supplier added successfully");
            },
            error: function () {
                toastr.error("Failed to add supplier");
            }
        });
    });
    //#endregion

    //#region Add Address
    $("#saveNewAddressBtn").on("click", function () {
        const address = {
            fullName: $("#newFullName").val().trim(),
            fullAddress: $("#newFullAddress").val().trim(),
            city: $("#newCity").val().trim(),
            state: $("#newState").val().trim(),
            postalCode: $("#newPostalCode").val().trim(),
            phone: $("#newAddressPhone").val().trim(),
            email: $("#newAddressEmail").val().trim()
        };

        if (!address.fullName || !address.fullAddress) {
            toastr.warning("Full name and address are required");
            return;
        }

        $.ajax({
            url: '/RequisitionToPurchaseOrder/AddAddress',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(address),
            success: function (newAddress) {
                const displayText = newAddress.fullName + ' - ' + newAddress.fullAddress.substring(0, 30) + '...';

                $("#po_BillingAddressId").append(
                    `<option value="${newAddress.id}">${displayText}</option>`
                );
                $("#po_ShippingAddressId").append(
                    `<option value="${newAddress.id}">${displayText}</option>`
                );

                $("#po_ShippingAddressId").val(newAddress.id);

                hideModal('addAddressModal');
                $("#addAddressModal input, #addAddressModal textarea").val('');
                toastr.success("Address added successfully");
            },
            error: function () {
                toastr.error("Failed to add address");
            }
        });
    });
    //#endregion

    //#region Export Functions
    $("#btnExportPDF").on("click", function () {
        customToaster.loading("Generating PDF...");

        fetch(`/RequisitionToPurchaseOrder/GeneratePDF?fromDate=${fromDate}&toDate=${toDate}`, { method: "POST" })
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

    $("#btnExportXL").on("click", function () {
        customToaster.loading("Generating Excel...");

        fetch(`/RequisitionToPurchaseOrder/DownloadExcel?fromDate=${fromDate}&toDate=${toDate}`)
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

    function hideModal(modalId) {
        $(`#${modalId}`).modal('hide');
    }
    //#endregion

    // Initial load
    loadApprovedRequisitions();
});
