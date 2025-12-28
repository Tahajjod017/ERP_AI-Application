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
            url: '/SalesOrder/GetAllCustomers',
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
    // LINE ITEMS - ADD/REMOVE
    // ==============================================================
    $('#add-item-btn').on('click', function () {
        const $table = $('#itemsTable tbody');
        const rowCount = $table.find('tr[data-index]').length;
        const newIndex = rowCount;

        const newRow = `
            <tr data-index="${newIndex}">
                <td class="fs-8 text-center align-middle">${newIndex + 1}</td>
                <td><input name="Items[${newIndex}].Description" class="form-control" /></td>
                <td>
                    <select name="Items[${newIndex}].Product" class="form-select searchableSelect unitDD">
                        <option value="">-- Select Product --</option>
                    </select>
                </td>
                <!-- <td><input name="Items[${newIndex}].Area" class="form-control calc" type="number" step="any" /></td> -->
                <td><input name="Items[${newIndex}].Rate" class="form-control calc" type="number" step="any" /></td>
                <td><input name="Items[${newIndex}].Quantity" class="form-control calc-qty" type="number" step="any" /></td>
            
                <td class="amount text-end align-middle">0.00</td>
                <td class="text-center align-middle">
                    <button type="button" class="btn btn-sm btn-outline-danger remove-item">
                        <i class="far fa-trash-alt text-black"></i>
                    </button>
                </td>
            </tr>`;

        $(this).closest('tr').before(newRow);

        const $newSelect = $table.find(`tr[data-index="${newIndex}"] .unitDD`);
        loadUnitOptions($newSelect);

        attachCalcEvents();
        renumberRows();

            //<td><input name="Items[${newIndex}].LIP" class="form-control" /></td>
            //<td><input name="Items[${newIndex}].LMAC" class="form-control" /></td>
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

    function loadUnitOptions($select) {
        $.ajax({
            url: '/SalesOrder/GetProduct',
            method: 'GET',
            success: function (units) {
                initializeSelect();
                $select.empty().append('<option value="">-- Select Unit --</option>');
                $.each(units, function (i, unit) {
                    $select.append(`<option value="${unit.id}">${unit.name}</option>`);
                });
            },
            error: function () {
                console.error('Failed to load units');
            }
        });
    }

    // ==============================================================
    // CALCULATION LOGIC
    // ==============================================================
    function attachCalcEvents() {
        $('.calc, .calc-qty').off('input').on('input', recalcTotals);
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
            const area = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
            const rate = parseFloat($(this).find('input[name$=".Rate"]').val()) || 0;

            const amount = area * rate;

            $(this).find('.amount').text(amount.toFixed(2));
            sub += amount;
        });

        const percentValue = parseFloat($('#vatPercent').val()) || 0;
        const vat = (sub * percentValue) / 100;
        const grand = sub - vat;

        $('#subTotal').text(sub.toFixed(2));
        $('#vatAmount').text(vat.toFixed(2));
        $('#grandTotal').text(grand.toFixed(2));
    }

    attachCalcEvents();
    recalcTotals();

    // ==============================================================
    // AJAX FORM SUBMISSION
    // ==============================================================
    $('#salesOrderForm').on('submit', function (e) {
        e.preventDefault();

        const customerId = $('#selectedCustomerId').val();

        if (!customerId) {
            toastr.warning('Please select a customer before saving the sales order');
            return false;
        }

        let hasValidItem = false;
        $('#itemsTable tbody tr[data-index]').each(function () {
            const area = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
            const rate = parseFloat($(this).find('input[name$=".Rate"]').val()) || 0;
            if (area > 0 && rate > 0) {
                hasValidItem = true;
                return false;
            }
        });

        if (!hasValidItem) {
            toastr.warning('Please add at least one item with Area and Rate');
            return false;
        }

        const formData = $(this).serialize();

        $.ajax({
            url: '/SalesOrder/Save',
            method: 'POST',
            data: formData,
            success: function (response) {
                toastr.success(response.message || 'Sales Order saved successfully!');
                window.location.href = '/SalesOrderDetails/Index/' + response.salesOrderId;
            },
            error: function () {
                toastr.error('Failed to save sales order. Please try again.');
            }
        });
    });

    $('#clearFormBtn').on('click', function () {
        if (confirm('Are you sure you want to clear the form?')) {
            location.reload();
        }
    });
});
