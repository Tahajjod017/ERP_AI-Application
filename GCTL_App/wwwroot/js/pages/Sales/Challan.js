$(function () {

    //#region Select 2

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

    //#region  Get Code
    GetNextCode();

    function GetNextCode() {
        $.ajax({
            url: '/Challan/GetNextShipmentNumber',
            method: 'GET',
            success: function (data) {
                $('#shipmentNumber').val(data);
            },
            error: function () {
                console.error('Failed to fetch shipment number');
            }
        });
    }
    //#endregion

    //#region Get Delivery Address

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

    // Load customers via AJAX
    function loadCustomers(saleOrderId, invoiceId) {
        $.ajax({
            url: '/Challan/GetAllAddresss',
            method: 'GET',
            data: { saleOrderId : saleOrderId, invoiceId : invoiceId },
            success: function (data) {
                customersData = data;
                populateDropdown();

                const preSelectedId = $('#preSelectedDeliveryId').val();
                if (preSelectedId) {
                    $('#showUserDropdownBtn').hide();
                    $('#userDropdownContainer').show();
                    $('#shippingAddressDropdown').val(preSelectedId);
                    showCustomer(preSelectedId);
                    loadQuotations(preSelectedId);
                }
            },
            error: function () {
                console.error('Failed to load customers');
                toastr.error('Failed to load customers. Please refresh the page.');
            }
        });
    }

    function populateDropdown() {
        const $dropdown = $('#shippingAddressDropdown');
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

    const saleOrderId  = $('#saleOrderId').val();
    const invoiceId = $('#invoiceId').val();


    loadCustomers(saleOrderId, invoiceId);

    function showCustomer(customerId) {
        if (!customerId) {
            $card.hide();
            $('#selectedDeliveryId').val('');
            $('#quotationDropdownContainer').hide();
            return;
        }

        const $option = $('#shippingAddressDropdown option[value="' + customerId + '"]');
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
        $('#shippingAddressDropdown').focus();
    });

    $('#shippingAddressDropdown').on('change', function () {
        const selectedId = $(this).val();
        showCustomer(selectedId);
    });

    //#endregion

    //#region  LOAD DROPDOWN DATA
    // ==============================================================
    let productsData = [];
    let locationsData = [];
    let addressesData = [];

    function loadProducts() {
        $.ajax({
            url: '/Challan/GetProducts',
            method: 'GET',
            success: function (data) {
                productsData = data;
                populateProductDropdowns();
            },
            error: function () {
                console.error('Failed to load products');
            }
        });
    }

    function loadLocations() {
        $.ajax({
            url: '/Challan/GetLocations',
            method: 'GET',
            success: function (data) {
                locationsData = data;
                populateLocationDropdowns();
            },
            error: function () {
                console.error('Failed to load locations');
            }
        });
    }

    function loadAddresses() {
        $.ajax({
            url: '/Challan/GetAddresses',
            method: 'GET',
            success: function (data) {
                addressesData = data;
                populateAddressDropdown();
            },
            error: function () {
                console.error('Failed to load addresses');
            }
        });
    }

    function populateProductDropdowns() {
        $('.productDD').each(function () {
            const $select = $(this);
            const currentValue = $select.val();

            $select.empty().append('<option value="">-- Select Product --</option>');
            productsData.forEach(function (product) {
                $select.append(`<option value="${product.id}">${product.name}</option>`);
            });

            if (currentValue) {
                $select.val(currentValue);
            }
        });
    }

    function populateLocationDropdowns() {
        $('.locationDD').each(function () {
            const $select = $(this);
            const currentValue = $select.val();

            $select.empty().append('<option value="">-- Select Location --</option>');
            locationsData.forEach(function (location) {
                $select.append(`<option value="${location.id}">${location.name}</option>`);
            });

            if (currentValue) {
                $select.val(currentValue);
            }
        });
    }

    function populateAddressDropdown() {
        const $dropdown = $('#shippingAddressDropdown');
        $dropdown.empty().append('<option value="">-- Select Shipping Address --</option>');

        addressesData.forEach(function (address) {
            const text = address.fullName + ' - ' + address.fullAddress;
            $dropdown.append(`<option value="${address.id}">${text}</option>`);
        });
    }

    // Initialize
    loadProducts();
    loadLocations();
    loadAddresses();

    // Address dropdown change - show address details
    $('#shippingAddressDropdown').on('change', function () {
        const addressId = $(this).val();
        if (!addressId) {
            $('#addressDisplay').hide();
            return;
        }

        const address = addressesData.find(a => a.id == addressId);
        if (address) {
            $('#addressDisplay .address-name').text(address.fullName);
            $('#addressDisplay .address-full').text(address.fullAddress);
            $('#addressDisplay .address-city').text(address.city + ', ' + address.state + ' ' + address.postalCode);
            $('#addressDisplay .address-phone').text('Phone: ' + address.phone);
            $('#addressDisplay').show();
        }
    });

    //#endregion

    //#region  LINE ITEMS - ADD/REMOVE
    // ==============================================================
    $('#add-item-btn').on('click', function () {
        const $table = $('#itemsTable tbody');
        const rowCount = $table.find('tr[data-index]').length;
        const newIndex = rowCount;

        const newRow = `
            <tr data-index="${newIndex}">
                <td class="fs-8 text-center align-middle">${newIndex + 1}</td>
                <td>
                    <select name="Items[${newIndex}].ProductId" class="form-select searchableSelect productDD" readonly>
                        <option value="">-- Select Product --</option>
                    </select>
                </td>
                <td>
                    <select name="Items[${newIndex}].FromLocationId" class="form-select searchableSelect locationDD" readonly>
                        <option value="">-- Select Location --</option>
                    </select>
                </td>
                <td><input name="Items[${newIndex}].OrderedQuantity" class="form-control" type="number" step="any" readonly /></td>
                <td><input name="Items[${newIndex}].AlreadyShipped" class="form-control" type="number" step="any" readonly /></td>
                <td><input name="Items[${newIndex}].ShippedQuantity" class="form-control" type="number" step="any" /></td>
                <td><input name="Items[${newIndex}].Note" class="form-control" /></td>
                <td class="text-center align-middle">
                    <button type="button" class="btn btn-sm btn-outline-danger mx-2 remove-item">
                        <i class="far fa-trash-alt text-black"></i>
                    </button>
                </td>
            </tr>`;

        $(this).closest('tr').before(newRow);

        // Populate new dropdowns
        const $newRow = $table.find(`tr[data-index="${newIndex}"]`);
        populateProductDropdowns();
        populateLocationDropdowns();

        // Reinitialize Select2 for new row
        $newRow.find('.searchableSelect').select2({
            width: '100%',
            allowClear: true,
            placeholder: 'Select an option'
        });

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
    });

    function renumberRows() {
        const $rows = $('#itemsTable tbody tr[data-index]');
        $rows.each(function (i) {
            $(this).attr('data-index', i);
            $(this).find('td:first').text(i + 1);
            $(this).find('input, select').each(function () {
                const name = $(this).attr('name');
                if (name) {
                    $(this).attr('name', name.replace(/\[\d+\]/, `[${i}]`));
                }
            });
        });
    }

    //#endregion ==============================================================

    //#region  FORM SUBMISSION
    // ==============================================================
    $('#shipmentForm').on('submit', function (e) {
        e.preventDefault();

        const shippingAddressId = $('#shippingAddressDropdown').val();

        if (!shippingAddressId) {
            toastr.warning('Please select a shipping address');
            return false;
        }

        let hasValidItem = false;
        $('#itemsTable tbody tr[data-index]').each(function () {
            const productId = $(this).find('select[name$=".ProductId"]').val();
            const locationId = $(this).find('select[name$=".FromLocationId"]').val();
            const shippedQty = parseFloat($(this).find('input[name$=".ShippedQuantity"]').val()) || 0;

            if (productId && locationId && shippedQty > 0) {
                hasValidItem = true;
                return false;
            }
        });

        if (!hasValidItem) {
            toastr.warning('Please add at least one item with Product, Location, and Shipped Quantity');
            return false;
        }

        const formData = $(this).serialize();

        $.ajax({
            url: '/Challan/Save',
            method: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Shipment saved successfully!');
                    window.location.href = '/ChallanDetails/index/' + response.shipmentId;
                } else {
                    toastr.error(response.message || 'Failed to save shipment');
                }
            },
            error: function () {
                toastr.error('Failed to save shipment. Please try again.');
            }
        });
    });

    //#endregion

    //#region Clear ordered que

    $('#clearFormBtn').on('click', function () {
        if (confirm('Are you sure you want to clear the form?')) {
            $('#shipmentForm')[0].reset();
            $('#addressDisplay').hide();
            GetNextCode();
        }
    });

    // Auto-fill shipped quantity from ordered quantity
    $(document).on('change', 'input[name$=".OrderedQuantity"]', function () {
        const $row = $(this).closest('tr');
        const orderedQty = $(this).val();
        const $shippedQtyInput = $row.find('input[name$=".ShippedQuantity"]');

        if ($shippedQtyInput.val() === '' || $shippedQtyInput.val() === '0') {
            $shippedQtyInput.val(orderedQty);
        }
    });

    //#endregion
});
