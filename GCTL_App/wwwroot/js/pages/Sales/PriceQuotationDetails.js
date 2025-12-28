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

   // GetNextCode();

    function GetNextCode() {
        $.ajax({
            url: '/PriceQuotation/GetNextQuotationNumber', // Adjust route as needed
            method: 'GET',
            success: function (data) {
                $('#quotationNumber').val(data);
            },
            error: function () {
                console.error('Failed to fetch quotation number');
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

    // Store customers data
    let customersData = [];

    // Load customers via AJAX
    function loadCustomers() {
        $.ajax({
            url: '/PriceQuotation/GetAllCustomers',
            method: 'GET',
            success: function (data) {
                //debugger
                customersData = data;
                populateDropdown();

                //debugger

                // Show customer on page load if already selected
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
                toastr.success('Failed to load customers. Please refresh the page.');
            }
        });
    }

    // Populate dropdown with customers
    function populateDropdown() {
        const $dropdown = $('#customerDropdown');

        // Clear existing options except first one
        $dropdown.find('option:not(:first)').remove();

        // Add customer options
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

    // Initialize - Load customers via AJAX
    loadCustomers();

    // Show customer info in card
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

    // Toggle "Add User" button to show dropdown
    $('#showUserDropdownBtn').on('click', function () {
        $(this).hide();
        $('#userDropdownContainer').show();
        $('#customerDropdown').focus();
    });

    // Handle dropdown change
    $('#customerDropdown').on('change', function () {
        const selectedId = $(this).val();
        showCustomer(selectedId);
    });

    // ==============================================================
    // ADD NEW CUSTOMER
    // ==============================================================
    $('#saveNewCustomerBtn').on('click', function () {
        const cust = {
            companyName: $('#newCompanyName').val().trim(),
            contactName: $('#newContactName').val().trim(),
            email: $('#newEmail').val().trim(),
            phone: $('#newPhone').val().trim(),
            addressLine1: $('#newAddress1').val().trim(),
            addressLine2: $('#newAddress2').val().trim(),
            taxNumber: $('#newTaxNumber').val().trim()
        };

        if (!cust.companyName) {
            toastr.success('Company Name is required');
            return;
        }

        $.ajax({
            url: '/PriceQuotation/AddCustomer',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(cust)
        })
            .done(function (newCust) {
                // Add to customersData array
                customersData.push(newCust);

                // Add new option to dropdown
                const $newOption = $('<option>')
                    .val(newCust.id)
                    .text(newCust.companyName + ' (' + newCust.contactName + ')')
                    .data({
                        company: newCust.companyName,
                        contact: newCust.contactName,
                        email: newCust.email,
                        phone: newCust.phone,
                        addr1: newCust.addressLine1,
                        addr2: newCust.addressLine2,
                        tax: newCust.taxNumber
                    });

                $('#customerDropdown').append($newOption);
                $('#customerDropdown').val(newCust.id);
                showCustomer(newCust.id);

                // Close modal and clear form
                $('#addUserModal').modal('hide');
                $('#addUserModal input').val('');
            })
            .fail(function () {
                toastr.warning('Failed to save customer');
            });
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
                    <select name="Items[${newIndex}].Product" class="form-select searchableSelect unitDD" >
                        <option value="">-- Select Product --</option>
                    </select>
                   
                </td>
                <!--<td class="stock text-center align-middle"></td>-->
                <td><input asp-for="Items[${newIndex}].Stock" class="form-control stock" type="text"  /></td>

                <td><input name="Items[${newIndex}].Area" class="form-control calc" type="number" step="any" /></td>
                <td><input name="Items[${newIndex}].Rate" class="form-control calc" type="number" step="any" /></td>
                <td class="amount text-end align-middle">0.00</td>
              
                <td class="text-center align-middle">
                    <button type="button" class="btn btn-sm btn-outline-danger remove-item"> <i class="far fa-trash-alt text-black "></i></button>
                </td>
            </tr>`;

        // Insert before the "Add Item" row
        $(this).closest('tr').before(newRow);

        //<td><input name="Items[${newIndex}].PercentInBill" class="form-control calc-percent" type="number" step="any" value="100" /></td>

        // Load units into the newly added select
        const $newSelect = $table.find(`tr[data-index="${newIndex}"] .unitDD`);
        loadUnitOptions($newSelect);




        // Re-attach event handlers
        attachCalcEvents();
        renumberRows();
    });

    // Remove item
    $(document).on('click', '.remove-item', function () {
        const $rows = $('#itemsTable tbody tr[data-index]');

        // Prevent removing if only one row exists
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
            url: '/PriceQuotation/GetProduct', // Replace with your actual endpoint
            method: 'GET',
            success: function (units) {
                initializeSelect();
                $select.empty().append('<option value="">-- Select Product --</option>');
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
        $('.calc, .calc-percent').off('input').on('input', recalcTotals);
    }

    $('#retentionPercent').on('input', function () {
        recalcTotals();
    });

    function renumberRows() {
        const $rows = $('#itemsTable tbody tr[data-index]');
        $rows.each(function (i) {
            $(this).attr('data-index', i);
            $(this).find('td:first').text(i + 1);
            $(this).find('input, textarea').each(function () {
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
            const area = parseFloat($(this).find('input[name$=".Area"]').val()) || 0;
            const rate = parseFloat($(this).find('input[name$=".Rate"]').val()) || 0;
            const percent = parseFloat($(this).find('input[name$=".PercentInBill"]').val()) || 100;

            const amount = area * rate;
            const amountWithPercent = amount * (percent / 100);

            $(this).find('.amount').text(amountWithPercent.toFixed(2));
            sub += amountWithPercent;
        });

        const percentValue = parseFloat($('#retentionPercent').val());

        //const retention = sub * 0.05; // 5%
        const retention = (sub * percentValue) / 100; // 5%
        const grand = sub - retention;

        $('#subTotal').text(sub.toFixed(2));
        $('#retention').text(retention.toFixed(2));
        $('#grandTotal').text(grand.toFixed(2));
    }

    // Initial setup
    attachCalcEvents();
    recalcTotals();

    // ==============================================================
    // CLEAR FORM FUNCTION
    // ==============================================================
    function clearForm() {
        // Reset form fields
        $('#quotationForm')[0].reset();

        // Clear customer selection
        $('#selectedCustomerId').val('');
        $('#customerDropdown').val('');
        $('#userInfo').hide();
        $('#showUserDropdownBtn').show();
        $('#userDropdownContainer').hide();

        // Clear items table - keep only one empty row
        $('#itemsTable tbody tr[data-index]').remove();
        const newRow = `
        <tr data-index="0">
            <td class="fs-8 text-center align-middle">1</td>
            <td><input name="Items[0].Description" class="form-control" /></td>
            <td><input name="Items[0].Product" class="form-control" value="Sft" /></td>
            <td><input asp-for="Items[i].Stock" class="form-control stock" type="text" /></td>

            <td><input name="Items[0].Area" class="form-control calc" type="number" step="any" /></td>
            <td><input name="Items[0].Rate" class="form-control calc" type="number" step="any" /></td>
            <td class="amount text-end align-middle">0.00</td>
          
            <td class="text-center align-middle">
                <button type="button" class="btn btn-sm btn-danger remove-item">X</button>
            </td>
        </tr>`;
        $('#itemsTable tbody').prepend(newRow);

        //<td><input name="Items[0].PercentInBill" class="form-control calc-percent" type="number" step="any" value="100" /></td>

        // Reset totals
        recalcTotals();
        attachCalcEvents();

        // Generate new invoice number
        const newInvoiceNo = 'INV-' + new Date().toISOString().replace(/[-:T.]/g, '').substring(0, 14);
        $('input[name="InvoiceNumber"]').val(newInvoiceNo);
    }

    // ==============================================================
    // AJAX FORM SUBMISSION
    // ==============================================================
    $('#quotationForm').on('submit', function (e) {
        e.preventDefault(); // Prevent default form submission

        const customerId = $('#selectedCustomerId').val();

        if (!customerId) {
            toastr.warning('Please select a customer before saving the quotation');
            return false;
        }

        // Check if at least one item has values
        let hasValidItem = false;
        $('#itemsTable tbody tr[data-index]').each(function () {
            const area = parseFloat($(this).find('input[name$=".Area"]').val()) || 0;
            const rate = parseFloat($(this).find('input[name$=".Rate"]').val()) || 0;
            if (area > 0 && rate > 0) {
                hasValidItem = true;
                return false; // break loop
            }
        });

        if (!hasValidItem) {
            toastr.warning('Please add at least one item with Area and Rate');
            return false;
        }

        // Serialize form data
        const formData = $(this).serialize();

        // Submit via AJAX
        $.ajax({
            url: '/PriceQuotation/Save',
            method: 'POST',
            data: formData,
            success: function (response) {
                //debugger
                toastr.success(response.message || 'Quotation saved successfully!');
                clearForm(); // Clear form after successful save

                window.location.href = '/PriceQuotationDetails/Index/' + response.quotationId;
            },
            error: function () {
                toastr.warning(response.message || 'Failed to save quotation. Please try again.');
            }
        });
    });

    // ==============================================================
    // CLEAR BUTTON (Add this if you want a clear button)
    // ==============================================================
    $('#clearFormBtn').on('click', function () {
        if (confirm('Are you sure you want to clear the form?')) {
            clearForm();
        }
    });

    // ==============================================================
    // SEND TO BUTTON (Optional)
    // ==============================================================
    $('#sendToBtn').on('click', function () {
        toastr.warning('Send To functionality - to be implemented');
        // You can implement email sending or other actions here
    });


    //==============================================
    // onchng
    //=============================================

    $(document).on("change", ".unitDD", function () {
        var productId = $(this).val();
        var locationId = $('#LocationId').val();
        var row = $(this).closest("tr");

        if (productId) {
            $.ajax({
                url: '/PriceQuotation/GetStockQuantity',
                type: 'GET',
                data: { productId: productId, locationId: locationId },
                success: function (response) {
                    row.find(".stock").val(response.available ?? "0.00");
                },
                error: function () {
                    row.find(".stock").val("Error");
                }
            });
        } else {
            row.find(".stock").val("");
        }
    });

    // যখন Location dropdown পরিবর্তন হবে
    $(document).on("change", "#LocationId", function () {
        var locationId = $(this).val();

        // প্রতিটি row ঘুরে productId নিয়ে stock আপডেট করুন
        $("tr[data-index]").each(function () {
            var row = $(this);
            var productId = row.find(".unitDD").val();

            if (productId) {
                $.ajax({
                    url: '/PriceQuotation/GetStockQuantity',
                    type: 'GET',
                    data: { productId: productId, locationId: locationId },
                    success: function (response) {
                        row.find(".stock").val(response.available ?? "0.00");
                    },
                    error: function () {
                        row.find(".stock").val("Error");
                    }
                });
            } else {
                row.find(".stock").val("");
            }
        });
    });



});





