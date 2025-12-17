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
            url: '/PurchaseOrder/GetNextPurchaseOrderNumber',
            method: 'GET',
            success: function (data) {
                $('#purchaseOrderNumber').val(data);
            },
            error: function () {
                console.error('Failed to fetch purchase order number');
            }
        });
    }

    // ==============================================================
    // SUPPLIER DROPDOWN HANDLING
    // ==============================================================

  

    const $card = $('#supplierInfo');
    const $dropdownContainer = $('#supplierDropdownContainer');
    const $info = {
        company: $('.info-company'),
        addr1: $('.info-addr1'),
        addr2: $('.info-addr2'),
        tax: $('.info-tax'),
        phone: $('.info-phone'),
        email: $('.info-email')
    };

    let suppliersData = [];


    //#region Supplier Address Dropdown Handling

    // ==============================================================
    // CLOSE SUPPLIER ADDRESS CARD
    // ==============================================================

    $('#closeSupplierAddressBtn').on('click', function () {
        // Hide the address card
        $card.hide();

        // Clear the selected value
        $('#supplierDropdown').val('');
        $('#selectedShippingAddressId').val('');

        // Show the dropdown container and button again
        $('#supplierDropdownContainer').hide();
        $('#showSupplierDropdownBtn').show();
    });




    function loadSuppliers() {
        $.ajax({
            url: '/PurchaseOrder/GetAllSuppliers',
            method: 'GET',
            success: function (data) {
                suppliersData = data;
                populateDropdown();

                const preSelectedId = $('#preSelectedSupplierId').val();
                if (preSelectedId) {
                    $('#showSupplierDropdownBtn').hide();
                    $('#supplierDropdownContainer').show();
                    $('#supplierDropdown').val(preSelectedId);
                    showSupplier(preSelectedId);
                }
            },
            error: function () {
                console.error('Failed to load suppliers');
                alert('Failed to load suppliers. Please refresh the page.');
            }
        });
    }



    // Add after loadSuppliers() function:

    let addressesData = [];

    function loadAddresses() {
        $.ajax({
            url: '/PurchaseOrder/GetAddresses',
            method: 'GET',
            success: function (data) {
                addressesData = data;
                populateAddressDropdowns();
            },
            error: function () {
                console.error('Failed to load addresses');
            }
        });
    }

    function populateAddressDropdowns() {
        const $billingDropdown = $('#billingAddressDropdown');
        const $shippingDropdown = $('#shippingAddressDropdown');

        $billingDropdown.find('option:not(:first)').remove();
        $shippingDropdown.find('option:not(:first)').remove();

        addressesData.forEach(function (address) {
            const text = address.fullName + ' - ' + address.fullAddress;
            $billingDropdown.append(`<option value="${address.id}">${text}</option>`);
            $shippingDropdown.append(`<option value="${address.id}">${text}</option>`);
        });
    }

    // Same as Billing checkbox handler
    $('#sameAsBilling').on('change', function () {
        if ($(this).is(':checked')) {
            const billingValue = $('#billingAddressDropdown').val();
            $('#shippingAddressDropdown').val(billingValue);
        }
    });

    // Save new address
    $('#saveNewAddressBtn').on('click', function () {
        const address = {
            fullName: $('#newAddressFullName').val().trim(),
            fullAddress: $('#newAddressFullAddress').val().trim(),
            city: $('#newAddressCity').val().trim(),
            state: $('#newAddressState').val().trim(),
            postalCode: $('#newAddressPostalCode').val().trim(),
            phone: $('#newAddressPhone').val().trim(),
            email: $('#newAddressEmail').val().trim()
        };

        if (!address.fullAddress) {
            alert('Full Address is required');
            return;
        }

        $.ajax({
            url: '/PurchaseOrder/AddAddress',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(address)
        })
            .done(function (newAddress) {
                addressesData.push(newAddress);
                populateAddressDropdowns();

                $('#billingAddressDropdown').val(newAddress.id);

                $('#addAddressModal').modal('hide');
                $('#addAddressModal input, #addAddressModal textarea').val('');

                toastr.success('Address added successfully!');
            })
            .fail(function () {
                alert('Failed to save address');
            });
    });

    // Call this in document ready:
    loadAddresses();




    function populateDropdown() {
        const $dropdown = $('#supplierDropdown');
        $dropdown.find('option:not(:first)').remove();

        suppliersData.forEach(function (supplier) {
            const $option = $('<option>')
                .val(supplier.id)
                .text(supplier.companyName + ' (' + supplier.contactName + ')')
                .data({
                    company: supplier.companyName,
                    contact: supplier.contactName,
                    email: supplier.email,
                    phone: supplier.phone,
                    addr1: supplier.addressLine1,
                    addr2: supplier.addressLine2,
                    tax: supplier.taxNumber
                });

            $dropdown.append($option);
        });
    }

    loadSuppliers();

    function showSupplier(supplierId) {
        if (!supplierId) {
            $card.hide();
            $('#selectedSupplierId').val('');
            return;
        }

        const $option = $('#supplierDropdown option[value="' + supplierId + '"]');
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

        $('#selectedSupplierId').val(supplierId);
        $dropdownContainer.hide();
        $card.show();
    }

    $('#showSupplierDropdownBtn').on('click', function () {
        $(this).hide();
        $('#supplierDropdownContainer').show();
        $('#supplierDropdown').focus();
    });

    $('#supplierDropdown').on('change', function () {
        const selectedId = $(this).val();
        showSupplier(selectedId);
    });

    // ==============================================================
    // ADD NEW SUPPLIER
    // ==============================================================
    $('#saveNewSupplierBtn').on('click', function () {
        const supplier = {
            companyName: $('#newCompanyName').val().trim(),
            contactName: $('#newContactName').val().trim(),
            email: $('#newEmail').val().trim(),
            phone: $('#newPhone').val().trim(),
            addressLine1: $('#newAddress1').val().trim(),
            addressLine2: $('#newAddress2').val().trim(),
            taxNumber: $('#newTaxNumber').val().trim()
        };

        if (!supplier.companyName) {
            alert('Company Name is required');
            return;
        }

        $.ajax({
            url: '/PurchaseOrder/AddSupplier',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(supplier)
        })
            .done(function (newSupplier) {
                suppliersData.push(newSupplier);

                const $newOption = $('<option>')
                    .val(newSupplier.id)
                    .text(newSupplier.companyName + ' (' + newSupplier.contactName + ')')
                    .data({
                        company: newSupplier.companyName,
                        contact: newSupplier.contactName,
                        email: newSupplier.email,
                        phone: newSupplier.phone,
                        addr1: newSupplier.addressLine1,
                        addr2: newSupplier.addressLine2,
                        tax: newSupplier.taxNumber
                    });

                $('#supplierDropdown').append($newOption);
                $('#supplierDropdown').val(newSupplier.id);
                showSupplier(newSupplier.id);

                $('#addSupplierModal').modal('hide');
                $('#addSupplierModal input').val('');
            })
            .fail(function () {
                alert('Failed to save supplier');
            });
    });

    //#endregion


    // ==============================================================
    // SHIPPING ADDRESS DROPDOWN HANDLING
    // ==============================================================

    //#region Shipping Address Dropdown Handling
    const $shippingCard = $('#shippingAddressInfo');
    const $shippingDropdownContainer = $('#shippingAddressDropdownContainer');
    const $shippingInfo = {
        fullName: $('.shipping-info-fullName'),
        fullAddress: $('.shipping-info-fullAddress'),
        city: $('.shipping-info-city'),
        state: $('.shipping-info-state'),
        postalCode: $('.shipping-info-postalCode'),
        phone: $('.shipping-info-phone'),
        email: $('.shipping-info-email')
    };

    let shippingAddressesData = [];

    function loadShippingAddresses() {
        // Check if data is embedded in the page
        const embeddedData = $('#shippingAddressesData').html();
        if (embeddedData && embeddedData.trim() !== '') {
            try {
                shippingAddressesData = JSON.parse(embeddedData);
                populateShippingDropdown();

                const preSelectedId = $('#preSelectedShippingAddressId').val();
                if (preSelectedId) {
                    $('#showShippingAddressDropdownBtn').hide();
                    $('#shippingAddressDropdownContainer').show();
                    $('#shippingAddressDropdown').val(preSelectedId);
                    showShippingAddress(preSelectedId);
                }
            } catch (e) {
                console.error('Failed to parse embedded shipping addresses:', e);
                loadShippingAddressesFromServer();
            }
        } else {
            loadShippingAddressesFromServer();
        }
    }

    function loadShippingAddressesFromServer() {
        $.ajax({
            url: '/PurchaseOrder/GetShippingAddresses', // Adjust endpoint as needed
            method: 'GET',
            success: function (data) {
                shippingAddressesData = data;
                populateShippingDropdown();

                const preSelectedId = $('#preSelectedShippingAddressId').val();
                if (preSelectedId) {
                    $('#showShippingAddressDropdownBtn').hide();
                    $('#shippingAddressDropdownContainer').show();
                    $('#shippingAddressDropdown').val(preSelectedId);
                    showShippingAddress(preSelectedId);
                }
            },
            error: function () {
                console.error('Failed to load shipping addresses');
            }
        });
    }

    function populateShippingDropdown() {
        const $dropdown = $('#shippingAddressDropdown');
        $dropdown.find('option:not(:first)').remove();

        shippingAddressesData.forEach(function (address) {
            const displayText = address.fullName + (address.fullAddress ? ' - ' + address.fullAddress.substring(0, 30) + '...' : '');
            const $option = $('<option>')
                .val(address.id)
                .text(displayText)
                .data({
                    fullName: address.fullName,
                    fullAddress: address.fullAddress,
                    city: address.city,
                    state: address.state,
                    postalCode: address.postalCode,
                    phone: address.phone,
                    email: address.email
                });

            $dropdown.append($option);
        });
    }

    function showShippingAddress(addressId) {
        if (!addressId) {
            $shippingCard.hide();
            $('#selectedShippingAddressId').val('');
            return;
        }

        const $option = $('#shippingAddressDropdown option[value="' + addressId + '"]');
        if ($option.length === 0) {
            $shippingCard.hide();
            return;
        }

        const fullName = $option.data('fullname');
        const fullAddress = $option.data('fulladdress');
        const city = $option.data('city');
        const state = $option.data('state');
        const postalCode = $option.data('postalcode');
        const phone = $option.data('phone');
        const email = $option.data('email');

        $shippingInfo.fullName.text(fullName || '');
        $shippingInfo.fullAddress.text(fullAddress || '');
        $shippingInfo.city.text(city || '');
        $shippingInfo.state.text(state || '');
        $shippingInfo.postalCode.text(postalCode || '');
        $shippingInfo.phone.text(phone || '');
        $shippingInfo.email.text(email || '');

        $('#selectedShippingAddressId').val(addressId);
        $shippingDropdownContainer.hide();
        $shippingCard.show();
    }

    // Event Handlers
    $('#showShippingAddressDropdownBtn').on('click', function () {
        $(this).hide();
        $('#shippingAddressDropdownContainer').show();
        $('#shippingAddressDropdown').focus();
    });

    $('#shippingAddressDropdown').on('change', function () {
        const selectedId = $(this).val();
        showShippingAddress(selectedId);
    });

    // ==============================================================
    // ADD NEW SHIPPING ADDRESS
    // ==============================================================
    $('#saveNewShippingAddressBtn').on('click', function () {
        const address = {
            fullName: $('#newShippingFullName').val().trim(),
            fullAddress: $('#newShippingFullAddress').val().trim(),
            city: $('#newShippingCity').val().trim(),
            state: $('#newShippingState').val().trim(),
            postalCode: $('#newShippingPostalCode').val().trim(),
            phone: $('#newShippingPhone').val().trim(),
            email: $('#newShippingEmail').val().trim()
        };

        if (!address.fullName || !address.fullAddress) {
            alert('Full Name and Full Address are required');
            return;
        }

        $.ajax({
            url: '/PurchaseOrder/AddShippingAddress', // Adjust endpoint as needed
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(address)
        })
            .done(function (newAddress) {
                shippingAddressesData.push(newAddress);

                const displayText = newAddress.fullName + (newAddress.fullAddress ? ' - ' + newAddress.fullAddress.substring(0, 30) + '...' : '');
                const $newOption = $('<option>')
                    .val(newAddress.id)
                    .text(displayText)
                    .data({
                        fullName: newAddress.fullName,
                        fullAddress: newAddress.fullAddress,
                        city: newAddress.city,
                        state: newAddress.state,
                        postalCode: newAddress.postalCode,
                        phone: newAddress.phone,
                        email: newAddress.email
                    });

                $('#shippingAddressDropdown').append($newOption);
                $('#shippingAddressDropdown').val(newAddress.id);
                showShippingAddress(newAddress.id);

                $('#addShippingAddressModal').modal('hide');
                $('#addShippingAddressModal input, #addShippingAddressModal textarea').val('');

                toastr.success('Shipping address added successfully!');
            })
            .fail(function () {
                alert('Failed to save shipping address');
            });
    });

    // ==============================================================
    // SAME AS BILLING FUNCTIONALITY
    // ==============================================================
    $('#sameAsShipping').on('change', function () {
        if ($(this).is(':checked')) {
            // Get billing address value and set it as shipping
            const billingValue = $('#billingAddressDropdown').val();
            if (billingValue) {
                $('#shippingAddressDropdown').val(billingValue);
                showShippingAddress(billingValue);
            }
        }
    });

    // ==============================================================
    // CLOSE SHIPPING ADDRESS CARD
    // ==============================================================
    $('#closeShippingAddressBtn').on('click', function () {
        // Hide the address card
        $shippingCard.hide();

        // Clear the selected value
        $('#shippingAddressDropdown').val('');
        $('#selectedShippingAddressId').val('');

        // Show the dropdown container and button again
        $('#shippingAddressDropdownContainer').hide();
        $('#showShippingAddressDropdownBtn').show();
    });

    // Initialize on document ready
    //$(document).ready(function () {
        loadShippingAddresses();
    //});

   //#endregion


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
                    <button type="button" class="btn btn-sm btn-outline-danger mx-2 remove-item">
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
            alert('At least one item is required');
            return;
        }

        $(this).closest('tr').remove();
        renumberRows();
        recalcTotals();
    });

    function loadProductOptions($select) {
        $.ajax({
            url: '/PurchaseOrder/GetProducts',
            method: 'GET',
            success: function (products) {
                initializeSelect();
                $select.empty().append('<option value="">-- Select Product --</option>');
                $.each(products, function (i, product) {
                    $select.append(`<option value="${product.id}" data-price="${product.price}">${product.name}</option>`);
                });
            },
            error: function () {
                console.error('Failed to load products');
            }
        });
    }

    // Auto-fill price when product is selected
    $(document).on('change', '.productDD', function () {
        const $row = $(this).closest('tr');
        const price = $(this).find('option:selected').data('price') || 0;
        $row.find('input[name$=".UnitPrice"]').val(price);
        recalcTotals();
    });

    // ==============================================================
    // CALCULATION LOGIC
    // ==============================================================
    function attachCalcEvents() {
        $('.calc').off('input').on('input', recalcTotals);
    }

    $('#taxPercent').on('input', function () {
        recalcTotals();
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

    function recalcTotals() {
        let sub = 0;
        $('#itemsTable tbody tr[data-index]').each(function () {
            const qty = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
            const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;

            const amount = qty * price;
            $(this).find('.amount').text(amount.toFixed(2));
            sub += amount;
        });

        const taxPercent = parseFloat($('#taxPercent').val()) || 0;
        const tax = (sub * taxPercent) / 100;
        const grand = sub + tax;

        $('#subTotal').text(sub.toFixed(2));
        $('#taxAmount').text(tax.toFixed(2));
        $('#grandTotal').text(grand.toFixed(2));
    }

    attachCalcEvents();
    recalcTotals();

    // ==============================================================
    // FORM SUBMISSION
    // ==============================================================
    $('#purchaseOrderForm').on('submit', function (e) {
        e.preventDefault();

        const supplierId = $('#selectedSupplierId').val();

        if (!supplierId) {
            alert('Please select a supplier before saving the purchase order');
            return false;
        }

        let hasValidItem = false;
        $('#itemsTable tbody tr[data-index]').each(function () {
            const qty = parseFloat($(this).find('input[name$=".Quantity"]').val()) || 0;
            const price = parseFloat($(this).find('input[name$=".UnitPrice"]').val()) || 0;
            if (qty > 0 && price > 0) {
                hasValidItem = true;
                return false;
            }
        });

        if (!hasValidItem) {
            alert('Please add at least one item with Quantity and Unit Price');
            return false;
        }

        const formData = $(this).serialize();

        $.ajax({
            url: '/PurchaseOrder/Save',
            method: 'POST',
            data: formData,
            success: function (response) {
                toastr.success(response.message || 'Purchase Order saved successfully!');
                window.location.href = '/PurchaseOrderDetails/index/' + response.purchaseOrderId;
            },
            error: function () {
                alert('Failed to save purchase order. Please try again.');
            }
        });
    });

    $('#clearFormBtn').on('click', function () {
        if (confirm('Are you sure you want to clear the form?')) {
            $('#purchaseOrderForm')[0].reset();
            GetNextCode();
            recalcTotals();
        }
    });
});
