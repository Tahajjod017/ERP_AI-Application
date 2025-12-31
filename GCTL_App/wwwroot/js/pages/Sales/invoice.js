$(function () {

    //#region Toggle button for VAT modes

    const $ev = $("#toggleCheckboxEV"); // VAT after subtotal
    const $incVat = $("#toggleCheckboxforIncludingVat"); // item price including VAT
    const $noVat = $("#toggleCheckboxForPriceWithoutVat"); // price without VAT
    const $ait = $("#toggleCheckboxForAIT"); // AIT 5%
    const $showTax = $("#showTaxColumn"); // Show Tax Column
    const $hiddenDiv = $("#hiddenDiv"); // wrapper div for showTax

    function reconcileConflicts() {
        // Only ONE VAT mode can be active at a time
        if ($incVat.is(":checked")) {
            $noVat.prop("checked", false);
            $ev.prop("checked", false);
        }
        if ($noVat.is(":checked")) {
            $incVat.prop("checked", false);
            $ev.prop("checked", false);
        }
        if ($ev.is(":checked")) {
            $incVat.prop("checked", false);
            $noVat.prop("checked", false);
        }

        // Show/hide Tax Column wrapper
        const anyTaxRelevant =
            $ev.is(":checked") ||
            $incVat.is(":checked") ||
            $noVat.is(":checked") ||
            $ait.is(":checked");

        $hiddenDiv.toggle(anyTaxRelevant);

        // Auto-check "Show Tax Column" when any tax mode is active
        if (anyTaxRelevant) {
            $showTax.prop("checked", true);
        } else {
            $showTax.prop("checked", false);
        }

        // Show/hide VAT per item column based on mode
        updateVatColumnVisibility();

        // Recalculate after mode change
        recalcTotals();
    }

    //function updateVatColumnVisibility() {
    //    const showVatColumn = $incVat.is(":checked") || $noVat.is(":checked");

    //    if (showVatColumn) {
    //        // Show VAT column header and cells
    //        $('#itemsTable thead th:nth-child(5)').show();
    //        $('#itemsTable tbody td.vatPerItem').parent().find('.vatPerItem').show();
    //    } else {
    //        // Hide VAT column
    //        $('#itemsTable thead th:nth-child(5)').hide();
    //        $('#itemsTable tbody td.vatPerItem').hide();
    //    }
    //}

    function updateVatColumnVisibility() {
        debugger
        const showVatColumn = $showTax.is(':checked');

        if (showVatColumn) {
            // Show header
            $('#itemsTable thead th.vat-column-header').show();
            // Show all VAT cells in rows (both % badge and amount)
            $('#itemsTable tbody td .vat-percent-badge').show();
            $('#itemsTable tbody td .vat-amount').show();
            // Also show the hidden input if needed (optional)
            $('#itemsTable tbody .item-vat-percent').closest('td').show();
        } else {
            // Hide header
            $('#itemsTable thead th.vat-column-header').hide();
            // Hide all VAT display elements
            $('#itemsTable tbody td .vat-percent-badge').hide();
            $('#itemsTable tbody td .vat-amount').hide();
            // Optional: hide entire column cells
            $('#itemsTable td:nth-child(5), #itemsTable th:nth-child(5)').hide();
        }
    }

    // Bind events
    //$("#toggleCheckboxEV, #toggleCheckboxforIncludingVat, #toggleCheckboxForPriceWithoutVat, #toggleCheckboxForAIT, #showTaxColumn")
    //    .on("change", reconcileConflicts);

    // Bind events - add this to your existing change handlers
    $("#toggleCheckboxEV, #toggleCheckboxforIncludingVat, #toggleCheckboxForPriceWithoutVat, #toggleCheckboxForAIT, #showTaxColumn")
        .on("change", function () {
            debugger
            reconcileConflicts();
            updateVatColumnVisibility();  // Add this line
        });

    // Also call it on page load
    $(document).ready(function () {
        updateVatColumnVisibility();
        reconcileConflicts();
    });

    // Initialize on load
    reconcileConflicts();

    //#endregion

    //#region Select2 initialization
    const initializeSelect = () => {
        $('.searchableSelect').select2({
            width: '100%',
            allowClear: true,
            placeholder: 'Select an option',
            language: { noResults: () => 'No results found' },
            escapeMarkup: markup => markup
        });
    };
    initializeSelect();
    //#endregion

    //#region Get Next Invoice Number
    GetNextCode();
    function GetNextCode() {
        $.ajax({
            url: '/Invoice/GetNextInvoiceNumber',
            method: 'GET',
            success: function (data) {
                $('#invoiceNumber').val(data);
            },
            error: function () {
                console.error('Failed to fetch invoice number');
            }
        });
    }
    //#endregion

    //#region Customer Dropdown Handling
    const $card = $('#userInfo');
    const $dropdownContainer = $('#userDropdownContainer');
    const $info = {
        company: $('.info-company'),
        addr1: $('.info-addr1'),
        addr2: $('.info-addr2'),
        tax: $('.info-tax'),
        phone: $('.info-phone'),
        email: $('.info-email')
    };

    let customersData = [];

    function loadCustomers() {
        $.ajax({
            url: '/Invoice/GetAllCustomers',
            method: 'GET',
            success: function (data) {
                customersData = data;
                populateDropdown();

                const preSelectedId = $('#preSelectedCustomerId').val();
                if (preSelectedId) {
                    $('#showUserDropdownBtn').hide();
                    $('#userDropdownContainer').show();
                    $('#customerDropdown').val(preSelectedId);
                    showCustomer(preSelectedId);
                    loadSalesOrders(preSelectedId);
                }
            },
            error: function () {
                console.error('Failed to load customers');
                toastr.error('Failed to load customers. Please refresh the page.');
            }
        });
    }

    function populateDropdown() {
        const $dropdown = $('#customerDropdown');
        $dropdown.find('option:not(:first)').remove();

        customersData.forEach(function (customer) {
            const $option = $('<option>')
                .val(customer.id)
                .text(customer.companyName + ' (' + customer.contactName + ')')
                .data({
                    company: customer.companyName,
                    contact: customer.contactName,
                    email: customer.email,
                    phone: customer.phone,
                    addr1: customer.addressLine1,
                    addr2: customer.addressLine2,
                    tax: customer.taxNumber
                });

            $dropdown.append($option);
        });
    }

    loadCustomers();

    function showCustomer(customerId) {
        if (!customerId) {
            $card.hide();
            $('#selectedCustomerId').val('');
            $('#salesOrderDropdownContainer').hide();
            return;
        }

        const $option = $('#customerDropdown option[value="' + customerId + '"]');
        if ($option.length === 0) {
            $card.hide();
            return;
        }

        const company = $option.data('company');
        const contact = $option.data('contact');
        const email = $option.data('email');
        const phone = $option.data('phone');
        const addr1 = $option.data('addr1');
        const addr2 = $option.data('addr2');
        const tax = $option.data('tax');

        $info.company.text(company || '');
        $info.addr1.text(addr1 || '');
        $info.addr2.text(addr2 || '');
        $info.tax.text('Tax Number: ' + (tax || ''));
        $info.phone.text(phone || '');
        $info.email.text(email || '');

        $('#selectedCustomerId').val(customerId);
        $dropdownContainer.hide();
        $card.show();

        $('#billingFirstName').val(contact);
        $('#billingPhone').val(phone);
        $('#billingEmail').val(email);
        $('#billingAddress').val(addr1);

        loadSalesOrders(customerId);
    }

    $('#showUserDropdownBtn').on('click', function () {
        $(this).hide();
        $('#userDropdownContainer').show();
        $('#customerDropdown').focus();
    });

    $('#customerDropdown').on('change', function () {
        const selectedId = $(this).val();
        showCustomer(selectedId);
    });
    //#endregion

    //#region Sales Order Dropdown
    function loadSalesOrders(customerId) {
        $.ajax({
            url: '/Invoice/GetSalesOrdersByCustomer',
            method: 'GET',
            data: { customerId: customerId },
            success: function (data) {
                const $dropdown = $('#salesOrderDropdown');
                $dropdown.find('option:not(:first)').remove();

                if (data && data.length > 0) {
                    data.forEach(function (so) {
                        const $option = $('<option>')
                            .val(so.id)
                            .text(so.number + ' - ' + new Date(so.date).toLocaleDateString());
                        $dropdown.append($option);
                    });

                    $('#salesOrderDropdownContainer').show();

                    const preSelectedSOId = $('#preSelectedSalesOrderId').val();
                    if (preSelectedSOId) {
                        $('#salesOrderDropdown').val(preSelectedSOId);
                        loadSalesOrderDetails(preSelectedSOId);
                    }
                } else {
                    $('#salesOrderDropdownContainer').hide();
                }
            },
            error: function () {
                console.error('Failed to load sales orders');
            }
        });
    }

    $('#salesOrderDropdown').on('change', function () {
        const soId = $(this).val();
        if (soId) {
            loadSalesOrderDetails(soId);
        }
    });

    function loadSalesOrderDetails(soId) {
        $.ajax({
            url: '/Invoice/GetSalesOrderDetails',
            method: 'GET',
            data: { salesOrderId: soId },
            success: function (response) {
                if (response.success) {
                    $('#vatPercent').val(response.vatPercent);
                    $('#invoiceNote').val(response.note);
                }
            },
            error: function () {
                toastr.error('Failed to load sales order details');
            }
        });
    }
    //#endregion

    //#region Line Items - Add/Remove
    $('#add-item-btn').on('click', function () {
        const $table = $('#itemsTable tbody');
        const rowCount = $table.find('tr[data-index]').length;
        const newIndex = rowCount;

        //const newRow = `
        //    <tr data-index="${newIndex}">
        //        <td class="fs-8 text-center align-middle">${newIndex + 1}</td>
        //        <td>
        //            <select name="Items[${newIndex}].ProductId" class="form-select searchableSelect productDD">
        //                <option value="">-- Select Product --</option>
        //            </select>
        //        </td>
        //        <td><input name="Items[${newIndex}].Quantity" class="form-control calc" type="number" step="any" /></td>
        //        <td><input name="Items[${newIndex}].UnitPrice" class="form-control calc" type="number" step="any" /></td>
        //        <td class="vatPerItem text-end align-middle">
        //            <input type="hidden" name="Items[${newIndex}].VatPercent" class="item-vat-percent" value="5" />
        //        0.00</td>
        //        <td class="amount text-center align-middle">0.00</td>
        //        <td class="text-center align-middle">
        //            <button type="button" class="btn btn-sm btn-outline-danger remove-item">
        //                <i class="far fa-trash-alt text-black"></i>
        //            </button>
        //        </td>
        //    </tr>`;

        const newRow = `
            <tr data-index="${newIndex}">
                <td class="fs-8 text-center align-middle">${newIndex + 1}</td>
                <td>
                    <select name="Items[${newIndex}].ProductId" class="form-select searchableSelect productDD">
                        <option value="">-- Select Product --</option>
                    </select>
                </td>
                <td><input name="Items[${newIndex}].Quantity" class="form-control calc" type="number" step="any" /></td>
                <td><input name="Items[${newIndex}].UnitPrice" class="form-control calc" type="number" step="any" /></td>
                <td class="text-end align-middle">
                    <input type="hidden" name="Items[${newIndex}].VatPercent" class="item-vat-percent" value="5" />
                    <span class="vat-percent-badge badge badge-phoenix badge-phoenix-warning clickable-vat" style="cursor:pointer;">0.00%</span>
                    <span class="vat-amount ms-2">0.00</span>
                </td>
                <td class="amount text-end align-middle">0.00</td>
                <td class="text-center align-middle">
                    <button type="button" class="btn btn-sm btn-outline-danger remove-item">
                        <i class="far fa-trash-alt text-black"></i>
                    </button>
                </td>
            </tr>`;

        $(this).closest('tr').before(newRow);

        const $newSelect = $table.find(`tr[data-index="${newIndex}"] .productDD`);
        loadProductOptions($newSelect);

        updateVatColumnVisibility();
        attachCalcEvents();
        renumberRows();
        recalcTotals();
    });

    $(document).on('click', '.remove-item', function () {
        const $rows = $('#itemsTable tbody tr[data-index]');

        if ($rows.length <= 1) {
            toastr.warning('At least one item is required');
            return;
        }

        $(this).closest('tr').remove();
        renumberRows();
        recalcTotals();
    });

    function loadProductOptions($select, selectedValue) {
        $.ajax({
            url: '/Invoice/GetProducts',
            method: 'GET',
            success: function (products) {
                initializeSelect();
                $select.empty().append('<option value="">-- Select Product --</option>');
                $.each(products, function (i, product) {
                    //$select.append(`<option value="${product.id}" data-price="${product.price}">${product.name}</option>`);

                    $select.append(`<option value="${product.id}" 
                        data-price="${product.price}" 
                        data-vat="${product.vatPercent}">
                        ${product.name}
                    </option>`);

                });

                if (selectedValue) {
                    $select.val(selectedValue);
                }

                //$select.on('change', function () {
                //    const selectedOption = $(this).find('option:selected');
                //    const price = selectedOption.data('price');
                //    const $row = $(this).closest('tr');
                //    $row.find('input[name$=".UnitPrice"]').val(price);
                //    recalcTotals();
                //});


                $select.on('change', function () {
                    const $option = $(this).find('option:selected');
                    const price = $option.data('price') || 0;
                    const vat = $option.data('vat') || 5;
                    const $row = $(this).closest('tr');
                    $row.find('input[name$=".UnitPrice"]').val(price);
                    $row.find('.item-vat-percent').val(vat);
                    $row.find('.vat-percent-badge').text(vat.toFixed(2) + '%');
                    recalcTotals();
                });


            },
            error: function () {
                console.error('Failed to load products');
            }
        });
    }
    //#endregion


    // Per-item VAT % edit
    $(document).on('click', '.clickable-vat', function () {
        const $row = $(this).closest('tr');
        const currentVat = parseFloat($row.find('.item-vat-percent').val()) || 5;
        const newVat = prompt("Enter VAT % for this item:", currentVat.toFixed(2));
        if (newVat !== null && !isNaN(newVat) && parseFloat(newVat) >= 0) {
            const vatVal = parseFloat(newVat);
            $row.find('.item-vat-percent').val(vatVal);
            $row.find('.vat-percent-badge').text(vatVal.toFixed(2) + '%');
            recalcTotals();
        }
    });

    // Global VAT % edit
    $(document).on('click', '.clickable-global-vat', function () {
        const current = parseFloat($('#vatPercent').val()) || 5;
        const newVal = prompt("Enter global VAT %:", current.toFixed(2));
        if (newVal !== null && !isNaN(newVal) && parseFloat(newVal) >= 0) {
            const val = parseFloat(newVal);
            $('#vatPercent').val(val);
            $('#globalVatPercentDisplay').text(val.toFixed(2) + '%');
            recalcTotals();
        }
    });

    // Global AIT % edit
    $(document).on('click', '.clickable-global-ait', function () {
        const current = parseFloat($('#aitPercent').val()) || 5;
        const newVal = prompt("Enter AIT %:", current.toFixed(2));
        if (newVal !== null && !isNaN(newVal) && parseFloat(newVal) >= 0) {
            const val = parseFloat(newVal);
            $('#aitPercent').val(val);
            $('#globalAitPercentDisplay').text(val.toFixed(2) + '%');
            recalcTotals();
        }
    });


    //#region Calculation Logic - OPTION A
    function attachCalcEvents() {
        $('.calc').off('input').on('input', recalcTotals);
    }

    $('#vatPercent, #aitPercent').on('input', recalcTotals);

    function renumberRows() {
        const $rows = $('#itemsTable tbody tr[data-index]');
        $rows.each(function (i) {
            $(this).attr('data-index', i);
            $(this).find('td:first').text(i + 1);
            $(this).find('input, textarea, select').each(function () {
                const name = $(this).attr('name');
                if (name) {
                    $(this).attr('name', name.replace(/\[\d+\]/, `[${i}]`));
                }
            });
        });
    }

    //function recalcTotals() {
    //    const vatPercent = parseFloat($('#vatPercent').val()) || 0;
    //    const aitPercent = parseFloat($('#aitPercent').val()) || 5;

    //    let netSubtotal = 0;
    //    let grossSubtotal = 0;
    //    let totalVAT = 0;

    //    // Calculate based on selected mode
    //    $('#itemsTable tbody tr[data-index]').each(function () {
    //        const $row = $(this);
    //        const quantity = parseFloat($row.find('input[name$=".Quantity"]').val()) || 0;
    //        const price = parseFloat($row.find('input[name$=".UnitPrice"]').val()) || 0;

    //        let netPrice = 0;
    //        let grossPrice = 0;
    //        let vatPerItem = 0;
    //        let itemTotal = 0;

    //        // MODE: Each item price including VAT
    //        if ($incVat.is(':checked')) {
    //            grossPrice = price;
    //            netPrice = grossPrice / (1 + vatPercent / 100);
    //            vatPerItem = (grossPrice - netPrice) * quantity;
    //            itemTotal = grossPrice * quantity;

    //            $row.find('.vatPerItem').text(vatPerItem.toFixed(2));
    //            $row.find('.amount').text(itemTotal.toFixed(2));

    //            netSubtotal += netPrice * quantity;
    //            grossSubtotal += itemTotal;
    //            totalVAT += vatPerItem;
    //        }
    //        // MODE: Price without VAT (per-item calculation)
    //        else if ($noVat.is(':checked')) {
    //            netPrice = price;
    //            vatPerItem = (netPrice * quantity * vatPercent) / 100;
    //            grossPrice = netPrice + (vatPerItem / quantity);
    //            itemTotal = (netPrice * quantity) + vatPerItem;

    //            $row.find('.vatPerItem').text(vatPerItem.toFixed(2));
    //            $row.find('.amount').text(itemTotal.toFixed(2));

    //            netSubtotal += netPrice * quantity;
    //            grossSubtotal += itemTotal;
    //            totalVAT += vatPerItem;
    //        }
    //        // MODE: VAT after subtotal (invoice-level)
    //        else if ($ev.is(':checked')) {
    //            netPrice = price;
    //            itemTotal = netPrice * quantity;

    //            $row.find('.vatPerItem').text('—');
    //            $row.find('.amount').text(itemTotal.toFixed(2));

    //            netSubtotal += itemTotal;
    //            grossSubtotal = netSubtotal; // Will add VAT later
    //        }
    //        // NO VAT MODE
    //        else {
    //            netPrice = price;
    //            itemTotal = netPrice * quantity;

    //            $row.find('.vatPerItem').text('—');
    //            $row.find('.amount').text(itemTotal.toFixed(2));

    //            netSubtotal += itemTotal;
    //            grossSubtotal = netSubtotal;
    //        }
    //    });

    //    // For "VAT after subtotal" mode, calculate VAT on entire subtotal
    //    if ($ev.is(':checked')) {
    //        totalVAT = (netSubtotal * vatPercent) / 100;
    //        grossSubtotal = netSubtotal + totalVAT;
    //    }

    //    // Calculate AIT if enabled
    //    let aitAmount = 0;
    //    if ($ait.is(':checked')) {
    //        aitAmount = (grossSubtotal * aitPercent) / 100;
    //    }

    //    // Grand Total
    //    const grandTotal = grossSubtotal + aitAmount;

    //    // Update display
    //    $('#subTotal').text(netSubtotal.toFixed(2));
    //    $('#vatAmount').text(totalVAT.toFixed(2));
    //    $('#grandTotal').text(grandTotal.toFixed(2));

    //    // Show/hide VAT row based on Show Tax Column checkbox
    //    if ($showTax.is(':checked')) {
    //        $('#vatAmount').closest('tr').show();
    //    } else {
    //        $('#vatAmount').closest('tr').hide();
    //    }

    //    // Show/hide AIT row
    //    if ($ait.is(':checked')) {
    //        $('#aitAmount').closest('tr').show();
    //        $('#aitAmount').text(aitAmount.toFixed(2));
    //    } else {
    //        $('#aitAmount').closest('tr').hide();
    //    }
    //}


    function recalcTotals() {
        const globalVatPercent = parseFloat($('#vatPercent').val()) || 15; // Global fallback
        const aitPercent = parseFloat($('#aitPercent').val()) || 5;

        let netSubtotal = 0;      // Amount excluding VAT
        let totalVAT = 0;         // Total VAT across all items
        let grossSubtotal = 0;    // Amount including VAT (before AIT)

        $('#itemsTable tbody tr[data-index]').each(function () {
            const $row = $(this);
            const quantity = parseFloat($row.find('input[name$=".Quantity"]').val()) || 0;
            const unitPrice = parseFloat($row.find('input[name$=".UnitPrice"]').val()) || 0;
            const itemVatPercent = parseFloat($row.find('.item-vat-percent').val()) || globalVatPercent;

            let itemNetAmount = 0;
            let itemVatAmount = 0;
            let itemTotalAmount = 0; // Line total (net + VAT)

            // MODE 1: Each item price INCLUDING VAT
            if ($incVat.is(':checked')) {
                // UnitPrice is gross → extract net
                const netPrice = unitPrice / (1 + itemVatPercent / 100);
                itemNetAmount = netPrice * quantity;
                itemVatAmount = (unitPrice - netPrice) * quantity;
                itemTotalAmount = unitPrice * quantity;

                $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
                $row.find('.amount').text(itemTotalAmount.toFixed(2));
            }
            // MODE 2: Price WITHOUT VAT (VAT added per item)
            else if ($noVat.is(':checked')) {
                itemNetAmount = unitPrice * quantity;
                itemVatAmount = (unitPrice * quantity * itemVatPercent) / 100;
                itemTotalAmount = itemNetAmount + itemVatAmount;

                $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
                $row.find('.amount').text(itemTotalAmount.toFixed(2));
            }
            // MODE 3: VAT AFTER SUBTOTAL (invoice-level VAT) OR No VAT mode
            else {
                itemNetAmount = unitPrice * quantity;
                itemTotalAmount = itemNetAmount;

                // In "VAT after subtotal" mode, per-item VAT is 0 (shown at bottom)
                if ($ev.is(':checked')) {
                    $row.find('.vat-amount').text('—');
                } else {
                    $row.find('.vat-amount').text('0.00');
                }
                $row.find('.amount').text(itemTotalAmount.toFixed(2));
            }

            // Accumulate totals
            netSubtotal += itemNetAmount;
            totalVAT += itemVatAmount;
            grossSubtotal += itemTotalAmount;
        });

        // Special case: VAT after subtotal → calculate VAT once on entire net subtotal using GLOBAL %
        if ($ev.is(':checked')) {
            totalVAT = (netSubtotal * globalVatPercent) / 100;
            grossSubtotal = netSubtotal + totalVAT;
        }

        // AIT Calculation (on gross subtotal if enabled)
        let aitAmount = 0;
        if ($ait.is(':checked')) {
            aitAmount = (grossSubtotal * aitPercent) / 100;
        }

        // Grand Total
        const grandTotal = grossSubtotal + aitAmount;

        // Update display fields
        $('#subTotal').text(netSubtotal.toFixed(2));
        $('#vatAmount').text(totalVAT.toFixed(2));
        $('#aitAmount').text(aitAmount.toFixed(2));
        $('#grandTotal').text(grandTotal.toFixed(2));

        // Show/hide VAT row based on Show Tax Column checkbox
        if ($showTax.is(':checked') && (totalVAT > 0 || $ev.is(':checked'))) {
            $('#vatAmount').closest('tr').show();
        } else {
            $('#vatAmount').closest('tr').hide();
        }

        // Show/hide AIT row
        if ($ait.is(':checked')) {
            $('#aitAmount').closest('tr').show();
        } else {
            $('#aitAmount').closest('tr').hide();
        }

        // Update VAT column visibility based on modes
        updateVatColumnVisibility();
    }


    attachCalcEvents();
    recalcTotals();
    //#endregion

    //#region Copy Billing to Shipping
    $('#copyBillingToShipping').on('click', function () {
        $('#shippingFirstName').val($('#billingFirstName').val());
        $('#shippingLastName').val($('#billingLastName').val());
        $('#shippingPhone').val($('#billingPhone').val());
        $('#shippingEmail').val($('#billingEmail').val());
        $('#shippingAddress').val($('#billingAddress').val());
        $('#shippingCity').val($('#billingCity').val());
        $('#shippingState').val($('#billingState').val());
        $('#shippingPostalCode').val($('#billingPostalCode').val());
    });
    //#endregion

    //#region Form Submission
    $('#invoiceForm').on('submit', function (e) {
        e.preventDefault();

        const customerId = $('#selectedCustomerId').val();
        if (!customerId) {
            toastr.warning('Please select a customer before saving the invoice');
            return false;
        }

        let hasValidItem = false;
        $('#itemsTable tbody tr[data-index]').each(function () {
            const quantity = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
            const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;
            if (quantity > 0 && price > 0) {
                hasValidItem = true;
                return false;
            }
        });

        if (!hasValidItem) {
            toastr.warning('Please add at least one item with Quantity and Price');
            return false;
        }

        const formData = $(this).serialize();

        $.ajax({
            url: '/Invoice/Save',
            method: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Invoice saved successfully!');
                    window.location.href = '/InvoiceDetails/Index/' + response.invoiceId;
                } else {
                    toastr.info(response.message || 'Invoice saved unsuccessfully!');
                }
            },
            error: function () {
                toastr.error('Failed to save invoice. Please try again.');
            }
        });
    });
    //#endregion

    //#region Clear Form
    $('#clearFormBtn').on('click', function () {
        if (confirm('Are you sure you want to clear the form?')) {
            location.reload();
        }
    });
    //#endregion
});









//$(function () {

//    //#region toggole button for vat

//    const $ev = $("#toggleCheckboxEV"); // VAT after subtotal
//    const $incVat = $("#toggleCheckboxforIncludingVat"); // item price including VAT
//    const $noVat = $("#toggleCheckboxForPriceWithoutVat"); // price without VAT
//    const $ait = $("#toggleCheckboxForAIT"); // AIT 5%
//    const $showTax = $("#showTaxColumn"); // Show Tax Column
//    const $hiddenDiv = $("#hiddenDiv"); // wrapper div for showTax

//    function reconcileConflicts() {
//        // Mutual exclusivity: including VAT vs price without VAT
//        if ($incVat.is(":checked")) {
//            $noVat.prop("checked", false);
//            // Prevent double VAT: including VAT vs VAT after subtotal
//            $ev.prop("checked", false);
//        }
//        if ($noVat.is(":checked")) {
//            $incVat.prop("checked", false);
//        }

//        // Show/hide Tax Column block automatically based on active tax flags
//        const anyTaxRelevant =
//            $ev.is(":checked") ||
//            $incVat.is(":checked") ||
//            $noVat.is(":checked") ||
//            $ait.is(":checked");

//        $hiddenDiv.toggle(anyTaxRelevant);

//        // Auto-toggle Show Tax Column ON when any tax relevant is on; allow manual off
//        if (anyTaxRelevant && !$showTax.is(":checked")) {
//            $showTax.prop("checked", true);
//        }
//        if (!anyTaxRelevant) {
//            $showTax.prop("checked", false);
//        }
//    }

//    // Bind events
//    $("#toggleCheckboxEV, #toggleCheckboxforIncludingVat, #toggleCheckboxForPriceWithoutVat, #toggleCheckboxForAIT, #showTaxColumn")
//        .on("change", reconcileConflicts);

//    // Initialize on load
//    reconcileConflicts();



//    //#endregion

//    //#region Select 2

//    const initializeSelect = () => {
//        $('.searchableSelect').select2({
//            width: '100%',
//            allowClear: true,
//            placeholder: 'Select an option',
//            language: { noResults: () => 'No results found' },
//            escapeMarkup: markup => markup
//        });
//    };

//    initializeSelect();

//    //#endregion

//    //#region GetNextCode

//    GetNextCode();

//    function GetNextCode() {
//        $.ajax({
//            url: '/Invoice/GetNextInvoiceNumber',
//            method: 'GET',
//            success: function (data) {
//                $('#invoiceNumber').val(data);
//            },
//            error: function () {
//                console.error('Failed to fetch invoice number');
//            }
//        });
//    }

//    //#endregion

//    //#region CUSTOMER DROPDOWN HANDLING
//    // ==============================================================
//    const $card = $('#userInfo');
//    const $dropdownContainer = $('#userDropdownContainer');
//    const $info = {
//        company: $('.info-company'),
//        addr1: $('.info-addr1'),
//        addr2: $('.info-addr2'),
//        tax: $('.info-tax'),
//        phone: $('.info-phone'),
//        email: $('.info-email')
//    };

//    let customersData = [];

//    function loadCustomers() {
//        $.ajax({
//            url: '/Invoice/GetAllCustomers',
//            method: 'GET',
//            success: function (data) {
//                customersData = data;
//                populateDropdown();

//                const preSelectedId = $('#preSelectedCustomerId').val();
//                if (preSelectedId) {
//                    $('#showUserDropdownBtn').hide();
//                    $('#userDropdownContainer').show();
//                    $('#customerDropdown').val(preSelectedId);
//                    showCustomer(preSelectedId);
//                    loadSalesOrders(preSelectedId);
//                }
//            },
//            error: function () {
//                console.error('Failed to load customers');
//                toastr.error('Failed to load customers. Please refresh the page.');
//            }
//        });
//    }

//    function populateDropdown() {
//        const $dropdown = $('#customerDropdown');
//        $dropdown.find('option:not(:first)').remove();

//        customersData.forEach(function (customer) {
//            const $option = $('<option>')
//                .val(customer.id)
//                .text(customer.companyName + ' (' + customer.contactName + ')')
//                .data({
//                    company: customer.companyName,
//                    contact: customer.contactName,
//                    email: customer.email,
//                    phone: customer.phone,
//                    addr1: customer.addressLine1,
//                    addr2: customer.addressLine2,
//                    tax: customer.taxNumber
//                });

//            $dropdown.append($option);
//        });
//    }

//    loadCustomers();

//    function showCustomer(customerId) {
//        if (!customerId) {
//            $card.hide();
//            $('#selectedCustomerId').val('');
//            $('#salesOrderDropdownContainer').hide();
//            return;
//        }

//        const $option = $('#customerDropdown option[value="' + customerId + '"]');
//        if ($option.length === 0) {
//            $card.hide();
//            return;
//        }

//        const company = $option.data('company');
//        const contact = $option.data('contact');
//        const email = $option.data('email');
//        const phone = $option.data('phone');
//        const addr1 = $option.data('addr1');
//        const addr2 = $option.data('addr2');
//        const tax = $option.data('tax');

//        $info.company.text(company || '');
//        $info.addr1.text(addr1 || '');
//        $info.addr2.text(addr2 || '');
//        $info.tax.text('Tax Number: ' + (tax || ''));
//        $info.phone.text(phone || '');
//        $info.email.text(email || '');

//        $('#selectedCustomerId').val(customerId);
//        $dropdownContainer.hide();
//        $card.show();

//        // Auto-populate billing address
//        $('#billingFirstName').val(contact);
//        $('#billingPhone').val(phone);
//        $('#billingEmail').val(email);
//        $('#billingAddress').val(addr1);

//        loadSalesOrders(customerId);
//    }

//    $('#showUserDropdownBtn').on('click', function () {
//        $(this).hide();
//        $('#userDropdownContainer').show();
//        $('#customerDropdown').focus();
//    });

//    $('#customerDropdown').on('change', function () {
//        const selectedId = $(this).val();
//        showCustomer(selectedId);
//    });

//    //#endregion ==============================================================

//    //#region SALES ORDER DROPDOWN HANDLING
//    // ==============================================================
//    function loadSalesOrders(customerId) {
//        $.ajax({
//            url: '/Invoice/GetSalesOrdersByCustomer',
//            method: 'GET',
//            data: { customerId: customerId },
//            success: function (data) {
//                const $dropdown = $('#salesOrderDropdown');
//                $dropdown.find('option:not(:first)').remove();

//                if (data && data.length > 0) {
//                    data.forEach(function (so) {
//                        const $option = $('<option>')
//                            .val(so.id)
//                            .text(so.number + ' - ' + new Date(so.date).toLocaleDateString());
//                        $dropdown.append($option);
//                    });

//                    $('#salesOrderDropdownContainer').show();

//                    const preSelectedSOId = $('#preSelectedSalesOrderId').val();
//                    if (preSelectedSOId) {
//                        $('#salesOrderDropdown').val(preSelectedSOId);
//                        loadSalesOrderDetails(preSelectedSOId);
//                    }
//                } else {
//                    $('#salesOrderDropdownContainer').hide();
//                }
//            },
//            error: function () {
//                console.error('Failed to load sales orders');
//            }
//        });
//    }

//    $('#salesOrderDropdown').on('change', function () {
//        const soId = $(this).val();
//        if (soId) {
//            loadSalesOrderDetails(soId);
//        }
//    });

//    function loadSalesOrderDetails(soId) {
//        $.ajax({
//            url: '/Invoice/GetSalesOrderDetails',
//            method: 'GET',
//            data: { salesOrderId: soId },
//            success: function (response) {
//                if (response.success) {
//                    $('#vatPercent').val(response.vatPercent);
//                    $('#invoiceNote').val(response.note);
//                }
//            },
//            error: function () {
//                toastr.error('Failed to load sales order details');
//            }
//        });
//    }

//    //#endregion ==============================================================

//    //#region LINE ITEMS - ADD/REMOVE
//    // ==============================================================
//    $('#add-item-btn').on('click', function () {
//        const $table = $('#itemsTable tbody');
//        const rowCount = $table.find('tr[data-index]').length;
//        const newIndex = rowCount;

//        const newRow = `
//            <tr data-index="${newIndex}">
//                <td class="fs-8 text-center align-middle">${newIndex + 1}</td>
//                <td>
//                    <select name="Items[${newIndex}].ProductId" class="form-select searchableSelect productDD">
//                        <option value="">-- Select Product --</option>
//                    </select>
//                </td>
//                <td><input name="Items[${newIndex}].Quantity" class="form-control calc" type="number" step="any" /></td>
//                <td><input name="Items[${newIndex}].UnitPrice" class="form-control calc" type="number" step="any" /></td>

//                <td class="amount text-end align-middle">0.00</td>
//                <td class="text-center align-middle">
//                    <button type="button" class="btn btn-sm btn-outline-danger remove-item">
//                        <i class="far fa-trash-alt text-black"></i>
//                    </button>
//                </td>
//            </tr>`;

//        $(this).closest('tr').before(newRow);

//        const $newSelect = $table.find(`tr[data-index="${newIndex}"] .productDD`);
//        loadProductOptions($newSelect);

//        //-------------
//        if ($('#toggleCheckboxforIncludingVat').is(':checked')) {
//            const percentValue = parseFloat($('#vatPercent').val()) || 0;
//            $('#itemsTable tbody tr[data-index]').each(function () {
//                const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;
//                const quantity = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;

//                // Base price calculation
//                const basePrice = price / (1 + percentValue / 100);
//                const vatPerItem = price - basePrice;
//                const totalVat = vatPerItem * quantity;

//                // Show VAT amount per product (extra column needed if you want to display)
//                $(this).find('.vatPerItem').text(totalVat.toFixed(2));
//            });
//        }
//        //---------


//        attachCalcEvents();
//        renumberRows();
//    });

//    $(document).on('click', '.remove-item', function () {
//        const $rows = $('#itemsTable tbody tr[data-index]');

//        if ($rows.length <= 1) {
//            toastr.warning('At least one item is required');
//            return;
//        }

//        $(this).closest('tr').remove();
//        renumberRows();
//        recalcTotals();
//    });

//    function loadProductOptions($select, selectedValue) {
//        $.ajax({
//            url: '/Invoice/GetProducts',
//            method: 'GET',
//            success: function (products) {
//                initializeSelect();
//                $select.empty().append('<option value="">-- Select Product --</option>');
//                $.each(products, function (i, product) {
//                    $select.append(`<option value="${product.id}" data-price="${product.price}">${product.name}</option>`);
//                });

//                if (selectedValue) {
//                    $select.val(selectedValue);
//                }

//                // Auto-fill price when product is selected
//                $select.on('change', function () {
//                    const selectedOption = $(this).find('option:selected');
//                    const price = selectedOption.data('price');
//                    const $row = $(this).closest('tr');
//                    $row.find('input[name$=".UnitPrice"]').val(price);
//                    recalcTotals();
//                });
//            },
//            error: function () {
//                console.error('Failed to load products');
//            }
//        });
//    }

//    //#endregion ==============================================================

//    //#region CALCULATION LOGIC
//    // ==============================================================
//    function attachCalcEvents() {
//        $('.calc').off('input').on('input', recalcTotals);
//    }

//    $('#vatPercent').on('input', function () {
//        recalcTotals();
//    });

//    function renumberRows() {
//        const $rows = $('#itemsTable tbody tr[data-index]');
//        $rows.each(function (i) {
//            $(this).attr('data-index', i);
//            $(this).find('td:first').text(i + 1);
//            $(this).find('input, textarea, select').each(function () {
//                const name = $(this).attr('name');
//                if (name) {
//                    $(this).attr('name', name.replace(/\[\d+\]/, `[${i}]`));
//                }
//            });
//        });
//    }

//    //function recalcTotals() {
//    //    let sub = 0;
//    //    $('#itemsTable tbody tr[data-index]').each(function () {
//    //        const quantity = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
//    //        const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;

//    //        const amount = quantity * price;

//    //        $(this).find('.amount').text(amount.toFixed(2));
//    //        sub += amount;
//    //    });

//    //    const percentValue = parseFloat($('#vatPercent').val()) || 0;
//    //    const vat = (sub * percentValue) / 100;
//    //    const grand = sub + vat;

//    //    $('#subTotal').text(sub.toFixed(2));
//    //    $('#vatAmount').text(vat.toFixed(2));
//    //    $('#grandTotal').text(grand.toFixed(2));
//    //    $('#dueAmount').text(grand.toFixed(2));
//    //}

//    function recalcTotals() {
//        let sub = 0;

//        // Calculate subtotal
//        $('#itemsTable tbody tr[data-index]').each(function () {
//            const quantity = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
//            const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;

//            const amount = quantity * price;
//            $(this).find('.amount').text(amount.toFixed(2));
//            sub += amount;
//        });

//        // VAT percent input
//        const percentValue = parseFloat($('#vatPercent').val()) || 0;
//        let vat = 0;

//        // Logic: Each item price including VAT
//        if ($('#toggleCheckboxforIncludingVat').is(':checked')) {
//            // দাম already VAT সহ, তাই আলাদা করে VAT যোগ হবে না
//            vat = 0;
//        }
//        // Logic: Price without VAT
//        else if ($('#toggleCheckboxForPriceWithoutVat').is(':checked')) {
//            // প্রতিটি আইটেমের দাম VAT ছাড়া, তাই subtotal এর উপর VAT যোগ হবে
//            vat = (sub * percentValue) / 100;
//        }
//        // Logic: VAT after subtotal
//        else if ($('#toggleCheckboxEV').is(':checked')) {
//            // subtotal এর পরে VAT যোগ হবে
//            vat = (sub * percentValue) / 100;
//        }

//        // Add AIT 5%
//        let ait = 0;
//        if ($('#toggleCheckboxForAIT').is(':checked')) {
//            ait = (sub + vat) * 0.05;
//        }

//        // Grand total
//        const grand = sub + vat + ait;

//        // Update UI
//        $('#subTotal').text(sub.toFixed(2));
//        $('#vatAmount').text(vat.toFixed(2));
//        $('#grandTotal').text(grand.toFixed(2));
//        $('#dueAmount').text(grand.toFixed(2));

//        // Show Tax Column toggle
//        if ($('#showTaxColumn').is(':checked')) {
//            $('#vatAmount').closest('tr').show();
//        } else {
//            $('#vatAmount').closest('tr').hide();
//        }
//    }

//    $('#vatPercent, #toggleCheckboxEV, #toggleCheckboxforIncludingVat, #toggleCheckboxForPriceWithoutVat, #toggleCheckboxForAIT, #showTaxColumn')
//        .on('input change', recalcTotals);

//    attachCalcEvents();
//    recalcTotals();

//    //#endregion ==============================================================

//    //#region COPY BILLING TO SHIPPING
//    // ==============================================================
//    $('#copyBillingToShipping').on('click', function () {
//        $('#shippingFirstName').val($('#billingFirstName').val());
//        $('#shippingLastName').val($('#billingLastName').val());
//        $('#shippingPhone').val($('#billingPhone').val());
//        $('#shippingEmail').val($('#billingEmail').val());
//        $('#shippingAddress').val($('#billingAddress').val());
//        $('#shippingCity').val($('#billingCity').val());
//        $('#shippingState').val($('#billingState').val());
//        $('#shippingPostalCode').val($('#billingPostalCode').val());
//    });

//    //#endregion ==============================================================

//    //#region AJAX FORM SUBMISSION
//    // ==============================================================
//    $('#invoiceForm').on('submit', function (e) {
//        e.preventDefault();

//        const customerId = $('#selectedCustomerId').val();

//        if (!customerId) {
//            toastr.warning('Please select a customer before saving the invoice');
//            return false;
//        }

//        let hasValidItem = false;
//        $('#itemsTable tbody tr[data-index]').each(function () {
//            const quantity = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
//            const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;
//            if (quantity > 0 && price > 0) {
//                hasValidItem = true;
//                return false;
//            }
//        });

//        if (!hasValidItem) {
//            toastr.warning('Please add at least one item with Quantity and Price');
//            return false;
//        }

//        const formData = $(this).serialize();

//        $.ajax({
//            url: '/Invoice/Save',
//            method: 'POST',
//            data: formData,
//            success: function (response) {
//                if (response.success) {
//                    toastr.success(response.message || 'Invoice saved successfully!');
//                    window.location.href = '/InvoiceDetails/Index/' + response.invoiceId;
//                } else {
//                    toastr.info(response.message || 'Invoice saved unSuccessfull!');

//                }
                
//            },
//            error: function () {
//                toastr.error('Failed to save invoice. Please try again.');
//            }
//        });
//    });

//    //#endregion


//    //#region clr btn

//    $('#clearFormBtn').on('click', function () {
//        if (confirm('Are you sure you want to clear the form?')) {
//            location.reload();
//        }
//    });

//    //#endregion
//});
