$(function () {

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

    // ==============================================================
    // CUSTOMER DROPDOWN HANDLING
    // ==============================================================
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

        // Auto-populate billing address
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

    // ==============================================================
    // SALES ORDER DROPDOWN HANDLING
    // ==============================================================
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

    // ==============================================================
    // LINE ITEMS - ADD/REMOVE
    // ==============================================================
    $('#add-item-btn').on('click', function () {
        const $table = $('#itemsTable tbody');
        const rowCount = $table.find('tr[data-index]').length;
        const newIndex = rowCount;

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

        attachCalcEvents();
        renumberRows();
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
                    $select.append(`<option value="${product.id}" data-price="${product.price}">${product.name}</option>`);
                });

                if (selectedValue) {
                    $select.val(selectedValue);
                }

                // Auto-fill price when product is selected
                $select.on('change', function () {
                    const selectedOption = $(this).find('option:selected');
                    const price = selectedOption.data('price');
                    const $row = $(this).closest('tr');
                    $row.find('input[name$=".UnitPrice"]').val(price);
                    recalcTotals();
                });
            },
            error: function () {
                console.error('Failed to load products');
            }
        });
    }

    // ==============================================================
    // CALCULATION LOGIC
    // ==============================================================
    function attachCalcEvents() {
        $('.calc').off('input').on('input', recalcTotals);
    }

    $('#vatPercent').on('input', function () {
        recalcTotals();
    });

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
        let sub = 0;
        $('#itemsTable tbody tr[data-index]').each(function () {
            const quantity = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
            const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;

            const amount = quantity * price;

            $(this).find('.amount').text(amount.toFixed(2));
            sub += amount;
        });

        const percentValue = parseFloat($('#vatPercent').val()) || 0;
        const vat = (sub * percentValue) / 100;
        const grand = sub + vat;

        $('#subTotal').text(sub.toFixed(2));
        $('#vatAmount').text(vat.toFixed(2));
        $('#grandTotal').text(grand.toFixed(2));
        $('#dueAmount').text(grand.toFixed(2));
    }

    attachCalcEvents();
    recalcTotals();

    // ==============================================================
    // COPY BILLING TO SHIPPING
    // ==============================================================
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

    // ==============================================================
    // AJAX FORM SUBMISSION
    // ==============================================================
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
                    toastr.info(response.message || 'Invoice saved unSuccessfull!');

                }
                
            },
            error: function () {
                toastr.error('Failed to save invoice. Please try again.');
            }
        });
    });

    $('#clearFormBtn').on('click', function () {
        if (confirm('Are you sure you want to clear the form?')) {
            location.reload();
        }
    });
});
