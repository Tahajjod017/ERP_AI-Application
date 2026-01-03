$(function () {

    //#region Toggle button for VAT modes
    const $ev = $("#toggleCheckboxEV"); // VAT after subtotal
    const $incVat = $("#toggleCheckboxforIncludingVat"); // Each item price including VAT
    const $noVat = $("#toggleCheckboxForPriceWithoutVat"); // Price without VAT
    const $ait = $("#toggleCheckboxForAIT"); // Add AIT 5%
    const $showTax = $("#showTaxColumn"); // Show VAT Column
    const $hiddenDiv = $("#hiddenDiv"); // wrapper div for showTax
    const $directChallan = $("#toggleCheckboxForDirectChallan"); // Direct Challan

    function updateUI() {
        // 1. Show/hide Show Tax Column wrapper - ONLY when incVat is checked
        $hiddenDiv.toggle($incVat.is(":checked"));

        // 2. Update VAT column visibility based on Show Tax Column checkbox
        updateVatColumnVisibility();

        // 3. Show/hide VAT after subtotal row in summary
        $('#vatRow').toggle($ev.is(':checked'));

        // 4. Update AIT display in product price column
        updateAitDisplay();

        // 5. Update VAT % badges visibility
        updateVatBadgeVisibility();

        // 6. Update ItemSerial column visibility
        updateItemSerialColumnVisibility();

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

    function updateItemSerialColumnVisibility() {
        const showItemSerial = $directChallan.is(':checked');

        // Show/hide ItemSerial column header
        $('#headerRow .itemserial-col').toggle(showItemSerial);

        // Show/hide ItemSerial column in each item row
        $('.item-row .itemserial-cell').toggle(showItemSerial);
    }

    function updateAitDisplay() {
        const showAit = $ait.is(':checked');

        $('.item-row').each(function () {
            const $row = $(this);
            const $aitDisplay = $row.find('.ait-display');

            if (showAit) {
                const basePrice = parseFloat($row.find('.unit-price').data('base-price')) || 0;
                const aitPercent = parseFloat($row.find('.item-ait-percent').val()) || 5;
                const aitAmount = (basePrice * aitPercent) / 100;

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
    $("#toggleCheckboxEV, #toggleCheckboxforIncludingVat, #toggleCheckboxForPriceWithoutVat, #toggleCheckboxForAIT, #toggleCheckboxForDirectChallan").on("change", reconcileConflicts);
    $("#showTaxColumn").on("change", updateVatColumnVisibility);

    // Initialize on page load
    $(document).ready(function () {
        updateUI();
        initializeSortable();
        initializeBarcodeScanner();
    });

    // Initialize on load
    reconcileConflicts();

    //#endregion

    //#region Drag and Drop - Sortable
    function initializeSortable() {
        const container = document.getElementById('item-container');
        if (!container) return;

        // Get all item rows except the add button
        const itemRows = Array.from(container.querySelectorAll('.item-row'));

        itemRows.forEach(row => {
            row.draggable = true;

            row.addEventListener('dragstart', function (e) {
                e.dataTransfer.effectAllowed = 'move';
                this.classList.add('dragging');
                this.style.opacity = '0.5';
            });

            row.addEventListener('dragend', function (e) {
                this.classList.remove('dragging');
                this.style.opacity = '1';
                renumberRows();
            });

            row.addEventListener('dragover', function (e) {
                e.preventDefault();
                const dragging = container.querySelector('.dragging');
                if (!dragging) return;

                const afterElement = getDragAfterElement(container, e.clientY);

                if (afterElement == null) {
                    container.insertBefore(dragging, document.getElementById('add-item-btn'));
                } else {
                    container.insertBefore(dragging, afterElement);
                }
            });
        });
    }

    function getDragAfterElement(container, y) {
        const draggableElements = [...container.querySelectorAll('.item-row:not(.dragging)')];

        return draggableElements.reduce((closest, child) => {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;

            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: child };
            } else {
                return closest;
            }
        }, { offset: Number.NEGATIVE_INFINITY }).element;
    }
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

    //#region Line Items - Add/Remove/Edit

    $('#add-item-btn').on('click', function () {
        showSearchBox();
    });

    //// Add Item via Barcode Button
    //$('#add-item-barcode-btn').on('click', function () {
    //    // TODO: Implement barcode scanning logic
    //    // This will require:
    //    // 1. Barcode scanner integration (USB/Bluetooth device)
    //    // 2. Server-side endpoint to get product by barcode: /Invoice/GetProductByBarcode
    //    // 3. Call addItemRow with the product data returned from server

    //    // Example implementation when ready:
    //    /*
    //    const barcode = "SCANNED_BARCODE_VALUE"; // Get from scanner device
    //    $.ajax({
    //        url: '/Invoice/GetProductByBarcode',
    //        method: 'GET',
    //        data: { barcode: barcode },
    //        success: function (product) {
    //            if (product) {
    //                addItemRow(product);
    //            } else {
    //                toastr.error('Product not found for barcode: ' + barcode);
    //            }
    //        },
    //        error: function () {
    //            toastr.error('Failed to fetch product by barcode');
    //        }
    //    });
    //    */

    //    toastr.info('Barcode scanning feature - Coming soon!');
    //});





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
        $('#add-item-btn').before(searchWrapper);
        input.focus();
    }

    function addItemRow(product, existingQuantity = 1, existingItemSerial = '') {
        const rowCount = $('.item-row').length;
        const newIndex = rowCount;
        const aitPercent = product.aitPercent || 5;
        const basePrice = product.price;
        const showAit = $ait.is(':checked');

        // Calculate display price with AIT if enabled
        const displayPrice = showAit ? basePrice * (1 + aitPercent / 100) : basePrice;
        const aitAmount = (basePrice * aitPercent) / 100;

        const row = $(`
    <div class="row align-items-center mb-2 item-row" data-index="${newIndex}" draggable="true" style="cursor:move;">
        <div class="col-1 fs-8 align-middle d-flex align-items-center justify-content-center text-center">
            <i class="fas fa-grip-vertical text-muted"></i>
            <span class="row-number ms-2 d-block">${newIndex + 1}</span>
        </div>
        <div class="col-3 col-md-2">
            <input name="Items[${newIndex}].ProductId" type="hidden" value="${product.id}" />
            <div class="product-name-display">${product.name}</div>
        </div>
        <div class="col-2 col-md-1">
            <input name="Items[${newIndex}].Quantity" class="form-control text-center calc quantity" type="number" step="any" value="${existingQuantity}" />
        </div>
        <div class="col itemserial-cell" style="display: none;">
            <input name="Items[${newIndex}].ItemSerial" class="form-control item-serial" type="text" value="${existingItemSerial}" placeholder="Serial" />
        </div>
        <div class="col text-end product-price-cell">
            <input name="Items[${newIndex}].UnitPrice" type="hidden" class="unit-price" value="${displayPrice}" data-base-price="${basePrice}" />
            <input type="hidden" name="Items[${newIndex}].VatPercent" class="item-vat-percent" value="${product.vatPercent || 15}" />
            <input type="hidden" name="Items[${newIndex}].AitPercent" class="item-ait-percent" value="${aitPercent}" />
            <span class="price-display">${displayPrice.toFixed(2)}</span>
            <span class="ait-display" style="display: ${showAit ? 'inline' : 'none'};">+${aitAmount.toFixed(2)} AIT</span>
            <span class="vat-percent-badge badge badge-phoenix badge-phoenix-warning clickable-vat ms-1" style="cursor:pointer; display: none;">
                ${(product.vatPercent || 15).toFixed(2)}%
            </span>
        </div>
        <div class="col text-end vat-cell" style="display: none;">
            <span class="vat-amount">0.00</span>
        </div>
        <div class="col text-end amount">${(displayPrice * existingQuantity).toFixed(2)}</div>
        <div class="col-1 text-center">

             <div class="btn-reveal-trigger position-static g-3">
                    <a href="#" class="nav-item me-2 edit-item " title="Edit">
                        <i class="fas fa-edit text-black"></i>
                    </a>
                               
                    <a href="#" class="nav-item me-2 delete-item" title="Delete">
                        <i class="far fa-trash-alt text-black "></i>
                    </a>


                </div>

            
        </div>
    </div>
    `);

    //<button type="button" class="btn btn-sm btn-outline-primary edit-item me-1" title="Edit">
    //            <i class="fas fa-edit"></i>
    //        </button>
    //        <button type="button" class="btn btn-sm btn-outline-danger delete-item" title="Delete">
    //            <i class="far fa-trash-alt"></i>
    //        </button>


        $('#add-item-btn').before(row);
        renumberRows();
        attachCalcEvents();
        updateUI();
        recalcTotals();
        initializeSortable();
    }

    // Edit Item
    $(document).on('click', '.edit-item', function () {
        const $row = $(this).closest('.item-row');
        const currentQuantity = parseFloat($row.find('.quantity').val()) || 1;
        const currentItemSerial = $row.find('.item-serial').val() || '';

        // Remove existing search if any
        $('.search-wrapper').remove();

        const searchWrapper = $('<div class="search-wrapper position-relative mb-3"></div>');
        const input = $('<input type="text" class="form-control" placeholder="Type product name to change..." autocomplete="off">');
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
                            $row.remove();
                            addItemRow(product, currentQuantity, currentItemSerial);
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

        input.on('blur', () => setTimeout(() => {
            dropdown.hide();
            searchWrapper.remove();
        }, 200));

        searchWrapper.append(input).append(dropdown);
        $row.before(searchWrapper);
        input.focus();
    });

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

    //#endregion


    //#region Barcode Scanner Implementation

    // Global variable to store barcode input
    let barcodeBuffer = '';
    let barcodeTimeout = null;

    // Initialize barcode scanner listener
    function initializeBarcodeScanner() {
        // Listen for keypress events (barcode scanners act like keyboards)
        $(document).on('keypress', function (e) {
            // Ignore if user is typing in an input field
            if ($(e.target).is('input, textarea')) {
                return;
            }

            // Check if Enter key (scanners typically end with Enter)
            if (e.which === 13) {
                if (barcodeBuffer.length > 0) {
                    processBarcodeInput(barcodeBuffer);
                    barcodeBuffer = '';
                }
            } else {
                // Add character to buffer
                barcodeBuffer += String.fromCharCode(e.which);

                // Clear timeout
                if (barcodeTimeout) {
                    clearTimeout(barcodeTimeout);
                }

                // Set timeout to clear buffer (scanners are fast, typing is slow)
                barcodeTimeout = setTimeout(function () {
                    barcodeBuffer = '';
                }, 100); // 100ms timeout
            }
        });
    }

    // Process barcode input
    function processBarcodeInput(barcode) {
        console.log('Barcode scanned:', barcode);

        // Show loading indicator
        toastr.info('Searching for product...', 'Barcode Scanned');

        // Call server to get product by barcode
        $.ajax({
            url: '/Invoice/GetProductByBarcode',
            method: 'GET',
            data: { barcode: barcode },
            success: function (product) {
                playBeep();
                if (product && product.id) {
                    // Check if product already exists in the list
                    let existingRow = null;
                    $('.item-row').each(function () {
                        const productId = parseInt($(this).find('input[name$=".ProductId"]').val());
                        if (productId === product.id) {
                            existingRow = $(this);
                            return false; // break the loop
                        }
                    });

                    if (existingRow) {
                        // Increment quantity if product already exists
                        const qtyInput = existingRow.find('.quantity');
                        const currentQty = parseFloat(qtyInput.val()) || 0;
                        qtyInput.val(currentQty + 1);
                        recalcTotals();
                        toastr.success('Quantity increased for: ' + product.name);
                    } else {
                        // Add new item
                        addItemRow(product);
                        toastr.success('Product added: ' + product.name);
                    }
                } else {
                    toastr.error('Product not found for barcode: ' + barcode);
                }
            },
            error: function (xhr, status, error) {
                console.error('Barcode lookup error:', error);
                toastr.error('Failed to fetch product by barcode: ' + barcode);
            }
        });
    }

    // Manual barcode entry button
    $('#add-item-barcode-btn').on('click', function () {
        const barcode = prompt("Enter product barcode:");

        if (barcode && barcode.trim() !== '') {
            processBarcodeInput(barcode.trim());
        }
    });

    // Initialize barcode scanner on document ready
    //$(document).ready(function () {
    //    initializeBarcodeScanner();
    //});


    function playBeep() {
        const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBTGH0fPTgjMGHm7A7+OZRQ8');
        audio.play();
    }

    // Add to processBarcodeInput success:
    playBeep();

    // Add to processBarcodeInput
    function flashScreen() {
        $('body').append('<div id="scan-flash" style="position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(0,255,0,0.3); z-index:9999;"></div>');
        setTimeout(function () {
            $('#scan-flash').fadeOut(200, function () {
                $(this).remove();
            });
        }, 100);
    }


    //#endregion

    // Per-item VAT % edit
    $(document).on('click', '.clickable-vat', function () {
        const $row = $(this).closest('.item-row');
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

            // Update all item prices with new AIT
            $('.item-row').each(function () {
                const $row = $(this);
                const basePrice = parseFloat($row.find('.unit-price').data('base-price')) || 0;
                const showAit = $ait.is(':checked');
                const newDisplayPrice = showAit ? basePrice * (1 + val / 100) : basePrice;

                $row.find('.unit-price').val(newDisplayPrice);
                $row.find('.item-ait-percent').val(val);
                $row.find('.price-display').text(newDisplayPrice.toFixed(2));
            });

            updateAitDisplay();
            recalcTotals();
        }
    });

    //#region Calculation Logic
    function attachCalcEvents() {
        $('.calc').off('input').on('input', recalcTotals);
    }

    $('#vatPercent, #aitPercent').on('input', recalcTotals);

    function renumberRows() {
        $('.item-row').each(function (i) {
            $(this).attr('data-index', i);
            $(this).find('.row-number').text(i + 1);
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
        let grossSubtotal = 0;

        // Object to store VAT amounts by percentage
        let vatByPercent = {};

        $('.item-row').each(function () {
            const $row = $(this);
            const quantity = parseFloat($row.find('.quantity').val()) || 0;
            let unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
            const basePrice = parseFloat($row.find('.unit-price').data('base-price')) || unitPrice;
            const itemVatPercent = parseFloat($row.find('.item-vat-percent').val()) || globalVatPercent;
            const itemAitPercent = parseFloat($row.find('.item-ait-percent').val()) || aitPercent;

            // Recalculate unit price with AIT if needed
            if (showAit) {
                unitPrice = basePrice * (1 + itemAitPercent / 100);
                $row.find('.unit-price').val(unitPrice);
                $row.find('.price-display').text(unitPrice.toFixed(2));
            } else {
                unitPrice = basePrice;
                $row.find('.unit-price').val(unitPrice);
                $row.find('.price-display').text(unitPrice.toFixed(2));
            }

            let itemNetAmount = 0;
            let itemVatAmount = 0;
            let itemTotalAmount = 0;

            // MODE 1: Each item price INCLUDING VAT
            if ($incVat.is(':checked')) {
                // Extract VAT from price
                const netPrice = unitPrice / (1 + itemVatPercent / 100);
                itemNetAmount = netPrice * quantity;
                itemVatAmount = (unitPrice - netPrice) * quantity;
                itemTotalAmount = unitPrice * quantity;

                // Show VAT amount in VAT column
                $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
            }
            // MODE 2: Price WITHOUT VAT
            else if ($noVat.is(':checked')) {
                itemNetAmount = unitPrice * quantity;
                itemVatAmount = (itemNetAmount * itemVatPercent) / 100;
                itemTotalAmount = itemNetAmount + itemVatAmount;

                // Show VAT amount in VAT column
                $row.find('.vat-amount').text(itemVatAmount.toFixed(2));
            }
            // MODE 3: VAT AFTER SUBTOTAL
            else if (vatAfterSubtotal) {
                itemNetAmount = unitPrice * quantity;
                itemTotalAmount = itemNetAmount;
                $row.find('.vat-amount').text('—');

                // Store item for later VAT calculation by percent
                if (!vatByPercent[itemVatPercent]) {
                    vatByPercent[itemVatPercent] = 0;
                }
                vatByPercent[itemVatPercent] += itemNetAmount;
            }
            // No VAT mode
            else {
                itemNetAmount = unitPrice * quantity;
                itemTotalAmount = itemNetAmount;
                $row.find('.vat-amount').text('0.00');
            }

            // Update item amount display
            $row.find('.amount').text(itemTotalAmount.toFixed(2));

            // Accumulate totals
            netSubtotal += itemNetAmount;
            if (!vatAfterSubtotal) {
                totalVAT += itemVatAmount;
            }
            grossSubtotal += itemTotalAmount;
        });

        // Calculate VAT after subtotal if that mode is active
        if (vatAfterSubtotal) {
            $('#vatBreakdownRows').empty();
            let totalVATSum = 0;

            // Sort VAT percentages for consistent display
            Object.keys(vatByPercent).sort((a, b) => parseFloat(a) - parseFloat(b)).forEach(function (percent) {
                const subtotalForPercent = vatByPercent[percent];
                const vatForPercent = (subtotalForPercent * parseFloat(percent)) / 100;
                totalVATSum += vatForPercent;

                const row = `<p class="mb-2 fw-bold">VAT @ ${parseFloat(percent).toFixed(2)}%: <span>${vatForPercent.toFixed(2)}</span> BDT</p>`;
                $('#vatBreakdownRows').append(row);
            });

            totalVAT = totalVATSum;
            grossSubtotal = netSubtotal + totalVAT;

            // Show total VAT row if there are multiple VAT percentages
            if (Object.keys(vatByPercent).length > 1) {
                $('#totalVatRow').show();
                $('#totalVatAmount').text(totalVAT.toFixed(2));
            } else {
                $('#totalVatRow').hide();
            }
        } else {
            $('#vatBreakdownRows').empty();
            $('#totalVatRow').hide();
        }

        // Update summary display
        $('#subTotal').text(netSubtotal.toFixed(2));
        $('#vatAmount').text(totalVAT.toFixed(2));
        $('#grandTotal').text(grossSubtotal.toFixed(2));

        // Update global VAT and AIT displays
        $('#globalVatPercentDisplay').text(globalVatPercent.toFixed(2) + '%');
        $('#globalAitPercentDisplay').text(aitPercent.toFixed(2) + '%');
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
        $('.item-row').each(function () {
            const quantity = parseFloat($(this).find('.quantity').val()) || 0;
            const price = parseFloat($(this).find('.unit-price').val()) || 0;
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