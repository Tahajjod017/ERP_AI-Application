$(function () {

    //#region Toggle button for VAT modes
    const $ev = $("#toggleCheckboxEV"); // VAT after subtotal
    const $incVat = $("#toggleCheckboxforIncludingVat"); // Each item price including VAT
    const $noVat = $("#toggleCheckboxForPriceWithoutVat"); // Price without VAT
    const $ait = $("#toggleCheckboxForAIT"); // Add AIT 5%
    const $showTax = $("#showTaxColumn"); // Show VAT Column
    const $hiddenDiv = $("#hiddenDiv"); // wrapper div for showTax

    function updateUI() {
        // 1. Show/hide Show Tax Column wrapper - ONLY when AIT is checked
        //$hiddenDiv.toggle($ait.is(":checked"));
        $hiddenDiv.toggle($incVat.is(":checked"));

        // 2. Update VAT column visibility based on Show Tax Column checkbox
        updateVatColumnVisibility();

        // 3. Show/hide VAT after subtotal row in summary
        $('#vatRow').toggle($ev.is(':checked'));

        // 4. Show/hide AIT row in summary
        $('#aitRow').toggle($ait.is(':checked'));

        // 5. Update AIT display in product price column
        updateAitDisplay();

        // 6. Update VAT % badges visibility
        updateVatBadgeVisibility();

        // Recalculate totals
        recalcTotals();
    }

    function updateVatColumnVisibility() {
        const showVatColumn = $showTax.is(':checked');

        // Show/hide VAT column header
        $('#headerRow .vat-col').toggle(showVatColumn);

        // Show/hide VAT column in each item row
        $('.item-row .vat-cell').toggle(showVatColumn);
    }

    function updateAitDisplay() {
        const showAit = $ait.is(':checked');

        $('.item-row').each(function () {
            const $row = $(this);
            const $aitDisplay = $row.find('.ait-display');

            if (showAit) {
                const unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
                const aitPercent = 5; // Assuming 5% AIT
                const aitAmount = (unitPrice * aitPercent) / 100;

                $aitDisplay.text(`+${aitAmount.toFixed(2)} AIT`).show();
            } else {
                $aitDisplay.hide();
            }
        });
    }

    function updateVatBadgeVisibility() {
        // Show VAT % badge only when NOT in "VAT after subtotal" mode
        const showVatBadge = !$ev.is(':checked') && ($incVat.is(':checked') || $noVat.is(':checked'));

        $('.vat-percent-badge').toggle(showVatBadge);
    }

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

        updateUI();
    }

    // Event bindings
    $("#toggleCheckboxEV, #toggleCheckboxforIncludingVat, #toggleCheckboxForPriceWithoutVat, #toggleCheckboxForAIT").on("change", reconcileConflicts);
    $("#showTaxColumn").on("change", updateVatColumnVisibility);

    // Initialize on page load
    $(document).ready(function () {
        updateUI();
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
        showSearchBox();
    });

    function showSearchBox() {
        // Remove existing search if any
        $('.search-wrapper').remove();

        const searchWrapper = $('<div class="search-wrapper position-relative mb-3"></div>');
        const input = $('<input type="text" class="form-control" placeholder="Type product name..." autocomplete="off">');
        const dropdown = $('<div class="search-dropdown border mt-1" style="display:none; position:absolute; width:100%; max-height:200px; overflow-y:auto; background:white; z-index:1000;"></div>');

        function renderDropdown(keyword = "") {
            $.ajax({
                url: '/Invoice/GetProducts',
                method: 'GET',
                success: function (products) {
                    dropdown.empty();
                    const filtered = products.filter(p =>
                        p.name.toLowerCase().includes(keyword.toLowerCase())
                    );

                    if (filtered.length === 0) {
                        dropdown.hide();
                        return;
                    }

                    filtered.forEach(product => {
                        const item = $(`<div class="px-3 py-2 hover-bg" style="cursor:pointer;">${product.name} - ${product.price.toFixed(2)} BDT</div>`);
                        item.on('click', () => {
                            searchWrapper.remove();
                            addItemRow(product);
                        });
                        dropdown.append(item);
                    });
                    dropdown.show();
                }
            });
        }

        input.on('input', function () {
            renderDropdown($(this).val());
        });

        input.on('blur', () => setTimeout(() => dropdown.hide(), 200));

        searchWrapper.append(input).append(dropdown);

        // FIX: target a real element instead of `this`
        $('#add-item-btn').before(searchWrapper);

        input.focus();
    }


    function addItemRow(product) {
        const rowCount = $('.item-row').length;
        const newIndex = rowCount;
        const aitPercent = product.aitPercent || 5;
        const aitAmount = (product.price * aitPercent) / 100;

        const row = $(`
    <div class="row align-items-center mb-2 item-row" data-index="${newIndex}">
        <div class="col-1 fs-8 align-middle text-center">${newIndex + 1}</div>
        <div class="col-3 col-md-3">
            <input name="Items[${newIndex}].ProductId" type="hidden" value="${product.id}" />
            <div>${product.name}</div>
        </div>
        <div class="col-2 col-md-2">
            <input name="Items[${newIndex}].Quantity" class="form-control text-center calc quantity" type="number" step="any" value="1" />
        </div>
        <div class="col text-end product-price-cell">
            <input name="Items[${newIndex}].UnitPrice" type="hidden" class="unit-price" value="${product.price}" />
            <input type="hidden" name="Items[${newIndex}].VatPercent" class="item-vat-percent" value="${product.vatPercent || 15}" />
            <input type="hidden" name="Items[${newIndex}].AitPercent" class="item-ait-percent" value="${aitPercent}" />
            <span class="price-display">${product.price.toFixed(2)}</span>
            <span class="ait-display" style="display: none;">+${aitAmount.toFixed(2)} AIT</span>
            <span class="vat-percent-badge badge badge-phoenix badge-phoenix-warning clickable-vat ms-1" style="cursor:pointer; display: none;">
                ${(product.vatPercent || 15).toFixed(2)}%
            </span>
        </div>
        <div class="col text-end vat-cell" style="display: none;">
            <span class="vat-amount">0.00</span>
        </div>
        <div class="col text-end amount">${product.price.toFixed(2)}</div>
        <div class="col-1 col-md-1 text-center">
            <button type="button" class="btn btn-sm btn-outline-danger delete-item">
                <i class="far fa-trash-alt"></i>
            </button>
        </div>
    </div>
    `);

        $('#add-item-btn').before(row);
        renumberRows();
        attachCalcEvents();
        updateUI(); // This will set proper visibility
        recalcTotals();
    }


    //function addItemRow(product) {
    //    const rowCount = $('.item-row').length;
    //    const newIndex = rowCount;

    //    //<select name="Items[${newIndex}].ProductId" class="form-select productDD searchableSelect">
    //    //    <option value="${product.id}" selected>${product.name}</option>
    //    //</select>

    //    const row = $(`
    //    <div class="row align-items-center mb-2 item-row " data-index="${newIndex}">
    //        <div class="col-md-1 fs-9 align-middle">${newIndex + 1}</div>
    //        <div class="col-md-3">

    //            <input name="Items[${newIndex}].ProductId" class="form-control" type="hidden"  />
    //            <div>${product.name}</div>

               
    //        </div>
    //        <div class="col-md-1">
    //            <input name="Items[${newIndex}].Quantity" class="form-control text-center calc quantity" type="number" step="any" value="1" />
    //        </div>
    //        <div class="col-md-2 text-end">
    //            <input name="Items[${newIndex}].UnitPrice" type="hidden" class="unit-price" value="${product.price}" />
    //            <input type="hidden" name="Items[${newIndex}].VatPercent" class="item-vat-percent" value="${product.vatPercent || 15}" />
    //            <span class="price-display">${product.price.toFixed(2)}</span>

    //             <span class="vat-percent-badge badge badge-phoenix badge-phoenix-warning clickable-vat me-2" style="cursor:pointer;">
    //                ${(product.vatPercent || 15).toFixed(2)}%
    //            </span>

    //        </div>
    //        <div class="col-md-2 text-end vat-cell">
               
    //            <span class="vat-amount">0.00</span>
    //        </div>
    //        <div class="col-md-2 text-end amount">0.00</div>
    //        <div class="col-md-1 text-center">
    //            <button type="button" class="btn btn-sm btn-outline-danger delete-item">
    //                <i class="far fa-trash-alt"></i>
    //            </button>
    //        </div>
    //    </div>
    //`);

    //    $('#add-item-btn').before(row);
    //    renumberRows();
    //    attachCalcEvents();
    //    recalcTotals();
    //}



    // Delete Item
    $(document).on('click', '.delete-item', function () {
        if ($('.item-row').length <= 1) {
            toastr.warning('At least one item required');
            return;
        }
        $(this).closest('.item-row').remove();
        renumberRows();
        recalcTotals();
    });

    // VAT Column Show/Hide
    $('#showTaxColumn').on('change', function () {
        if ($(this).is(':checked')) {
            $('.vat-cell').show();
        } else {
            $('.vat-cell').hide();
        }
    });

    // Initial hide if unchecked
    if (!$('#showTaxColumn').is(':checked')) {
        $('.vat-cell').hide();
    }


    //$('#add-item-btn').on('click', function () {
    //    const $table = $('#itemsTable tbody');
    //    const rowCount = $table.find('tr[data-index]').length;
    //    const newIndex = rowCount;

    //    //const newRow = `
    //    //    <tr data-index="${newIndex}">
    //    //        <td class="fs-8 text-center align-middle">${newIndex + 1}</td>
    //    //        <td>
    //    //            <select name="Items[${newIndex}].ProductId" class="form-select searchableSelect productDD">
    //    //                <option value="">-- Select Product --</option>
    //    //            </select>
    //    //        </td>
    //    //        <td><input name="Items[${newIndex}].Quantity" class="form-control calc" type="number" step="any" /></td>
    //    //        <td><input name="Items[${newIndex}].UnitPrice" class="form-control calc" type="number" step="any" /></td>
    //    //        <td class="vatPerItem text-end align-middle">
    //    //            <input type="hidden" name="Items[${newIndex}].VatPercent" class="item-vat-percent" value="5" />
    //    //        0.00</td>
    //    //        <td class="amount text-center align-middle">0.00</td>
    //    //        <td class="text-center align-middle">
    //    //            <button type="button" class="btn btn-sm btn-outline-danger remove-item">
    //    //                <i class="far fa-trash-alt text-black"></i>
    //    //            </button>
    //    //        </td>
    //    //    </tr>`;

    //    const newRow = `
    //        <tr data-index="${newIndex}">
    //            <td class="fs-8 text-center align-middle">${newIndex + 1}</td>
    //            <td>
    //                <select name="Items[${newIndex}].ProductId" class="form-select searchableSelect productDD">
    //                    <option value="">-- Select Product --</option>
    //                </select>
    //            </td>
    //            <td><input name="Items[${newIndex}].Quantity" class="form-control calc" type="number" step="any" /></td>
    //            <td>
    //                <input name="Items[${newIndex}].UnitPrice" class="form-control calc" type="hidden" step="any" />
    //                <input name="Items[${newIndex}].VatPercent" class="item-vat-percent" type="hidden" value="5" />

    //                <span class="price-per-amount">0.00</span>
    //                <span class="vat-percent-badge badge badge-phoenix badge-phoenix-warning clickable-vat" style="cursor:pointer;">0.00%</span>
    //                <span class="vat-per-amount ms-2">0.00</span>
    //            </td>
    //            <td class="text-end align-middle">
                   
                   
    //                <span class="vat-amount ms-2">0.00</span>
    //            </td>
    //            <td class="amount text-end align-middle">0.00</td>
    //            <td class="text-center align-middle">
    //                <button type="button" class="btn btn-sm btn-outline-danger remove-item">
    //                    <i class="far fa-trash-alt text-black"></i>
    //                </button>
    //            </td>
    //        </tr>`;

    //    $(this).closest('tr').before(newRow);

    //    const $newSelect = $table.find(`tr[data-index="${newIndex}"] .productDD`);
    //    loadProductOptions($newSelect);

    //    updateVatColumnVisibility();
    //    attachCalcEvents();
    //    renumberRows();
    //    recalcTotals();
    //});

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
                    $row.find('.price-per-amount').text(price);
                    $row.find('.vat-per-amount').text((price * vat)/100);
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

    function recalcTotals() {
        const globalVatPercent = parseFloat($('#vatPercent').val()) || 15;
        const aitPercent = parseFloat($('#aitPercent').val()) || 5;
        const showAit = $ait.is(':checked');
        const vatAfterSubtotal = $ev.is(':checked');

        let netSubtotal = 0;
        let totalVAT = 0;
        let totalAIT = 0;
        let grossSubtotal = 0;

        $('.item-row').each(function () {
            const $row = $(this);
            const quantity = parseFloat($row.find('.quantity').val()) || 0;
            const unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
            const itemVatPercent = parseFloat($row.find('.item-vat-percent').val()) || globalVatPercent;
            const itemAitPercent = parseFloat($row.find('.item-ait-percent').val()) || aitPercent;

            let itemNetAmount = unitPrice * quantity;
            let itemVatAmount = 0;
            let itemAitAmount = 0;
            let itemTotalAmount = itemNetAmount;

            // Calculate AIT
            if (showAit) {
                itemAitAmount = (itemNetAmount * itemAitPercent) / 100;
            }

            // MODE 1: Each item price INCLUDING VAT
            if ($incVat.is(':checked')) {
                // Extract VAT from price
                const netPrice = unitPrice / (1 + itemVatPercent / 100);
                itemNetAmount = netPrice * quantity;
                itemVatAmount = (unitPrice - netPrice) * quantity;
                itemTotalAmount = itemNetAmount + itemVatAmount + itemAitAmount;

                // Show VAT amount in VAT column
                $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
            }
            // MODE 2: Price WITHOUT VAT
            else if ($noVat.is(':checked')) {
                itemVatAmount = (itemNetAmount * itemVatPercent) / 100;
                itemTotalAmount = itemNetAmount + itemVatAmount + itemAitAmount;

                // Show VAT amount in VAT column
                $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
            }
            // MODE 3: VAT AFTER SUBTOTAL
            else if (vatAfterSubtotal) {
                // VAT will be calculated at invoice level
                itemTotalAmount = itemNetAmount + itemAitAmount;
                $row.find('.vat-amount').text('—');
            }
            // No VAT mode
            else {
                itemTotalAmount = itemNetAmount + itemAitAmount;
                $row.find('.vat-amount').text('0.00');
            }

            // Update item amount display
            $row.find('.amount').text(itemTotalAmount.toFixed(2));

            // Accumulate totals
            netSubtotal += itemNetAmount;
            totalVAT += itemVatAmount;
            totalAIT += itemAitAmount;
            grossSubtotal += itemTotalAmount;
        });

        // Calculate VAT after subtotal if that mode is active
        if (vatAfterSubtotal) {
            totalVAT = (netSubtotal * globalVatPercent) / 100;
            grossSubtotal = netSubtotal + totalVAT + totalAIT;
        }

        // Update summary display
        $('#subTotal').text(netSubtotal.toFixed(2));
        $('#vatAmount').text(totalVAT.toFixed(2));
        $('#aitAmount').text(totalAIT.toFixed(2));
        $('#grandTotal').text(grossSubtotal.toFixed(2));

        // Update global VAT and AIT displays
        $('#globalVatPercentDisplay').text(globalVatPercent.toFixed(2) + '%');
        $('#globalAitPercentDisplay').text(aitPercent.toFixed(2) + '%');
    }

    //function recalcTotals() {
    //    const globalVatPercent = parseFloat($('#vatPercent').val()) || 15; // Global fallback
    //    const aitPercent = parseFloat($('#aitPercent').val()) || 5;

    //    let netSubtotal = 0;      // Amount excluding VAT
    //    let totalVAT = 0;         // Total VAT across all items
    //    let grossSubtotal = 0;    // Amount including VAT (before AIT)

    //    $('#itemsTable tbody tr[data-index]').each(function () {
    //        const $row = $(this);
    //        const quantity = parseFloat($row.find('input[name$=".Quantity"]').val()) || 0;
    //        const unitPrice = parseFloat($row.find('input[name$=".UnitPrice"]').val()) || 0;
    //        const itemVatPercent = parseFloat($row.find('.item-vat-percent').val()) || globalVatPercent;

    //        let itemNetAmount = 0;
    //        let itemVatAmount = 0;
    //        let itemTotalAmount = 0; // Line total (net + VAT)

    //        // MODE 1: Each item price INCLUDING VAT
    //        if ($incVat.is(':checked')) {
    //            // UnitPrice is gross → extract net
    //            const netPrice = unitPrice / (1 + itemVatPercent / 100);
    //            itemNetAmount = netPrice * quantity;
    //            itemVatAmount = (unitPrice - netPrice) * quantity;
    //            itemTotalAmount = unitPrice * quantity;

    //            $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
    //            $row.find('.amount').text(itemTotalAmount.toFixed(2));
    //        }
    //        // MODE 2: Price WITHOUT VAT (VAT added per item)
    //        else if ($noVat.is(':checked')) {
    //            itemNetAmount = unitPrice * quantity;
    //            itemVatAmount = (unitPrice * quantity * itemVatPercent) / 100;
    //            itemTotalAmount = itemNetAmount + itemVatAmount;

    //            $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
    //            $row.find('.amount').text(itemTotalAmount.toFixed(2));
    //        }
    //        // MODE 3: VAT AFTER SUBTOTAL (invoice-level VAT) OR No VAT mode
    //        else {
    //            itemNetAmount = unitPrice * quantity;
    //            itemTotalAmount = itemNetAmount;

    //            // In "VAT after subtotal" mode, per-item VAT is 0 (shown at bottom)
    //            if ($ev.is(':checked')) {
    //                $row.find('.vat-amount').text('—');
    //            } else {
    //                $row.find('.vat-amount').text('0.00');
    //            }
    //            $row.find('.amount').text(itemTotalAmount.toFixed(2));
    //        }

    //        // Accumulate totals
    //        netSubtotal += itemNetAmount;
    //        totalVAT += itemVatAmount;
    //        grossSubtotal += itemTotalAmount;
    //    });

    //    // Special case: VAT after subtotal → calculate VAT once on entire net subtotal using GLOBAL %
    //    if ($ev.is(':checked')) {
    //        totalVAT = (netSubtotal * globalVatPercent) / 100;
    //        grossSubtotal = netSubtotal + totalVAT;
    //    }

    //    // AIT Calculation (on gross subtotal if enabled)
    //    let aitAmount = 0;
    //    if ($ait.is(':checked')) {
    //        aitAmount = (grossSubtotal * aitPercent) / 100;
    //    }

    //    // Grand Total
    //    const grandTotal = grossSubtotal + aitAmount;

    //    // Update display fields
    //    $('#subTotal').text(netSubtotal.toFixed(2));
    //    $('#vatAmount').text(totalVAT.toFixed(2));
    //    $('#aitAmount').text(aitAmount.toFixed(2));
    //    $('#grandTotal').text(grandTotal.toFixed(2));

    //    // Show/hide VAT row based on Show Tax Column checkbox
    //    if ($showTax.is(':checked') && (totalVAT > 0 || $ev.is(':checked'))) {
    //        $('#vatAmount').closest('tr').show();
    //    } else {
    //        $('#vatAmount').closest('tr').hide();
    //    }

    //    // Show/hide AIT row
    //    if ($ait.is(':checked')) {
    //        $('#aitAmount').closest('tr').show();
    //    } else {
    //        $('#aitAmount').closest('tr').hide();
    //    }

    //    // Update VAT column visibility based on modes
    //    updateVatColumnVisibility();
    //}


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


