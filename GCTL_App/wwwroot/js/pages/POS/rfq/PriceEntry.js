$(document).ready(function () {
    const vatRate = 0.0; // 10% VAT

    // Show alert message
    function showAlert(message, type = 'success') {

        toastr[type](message);

        //const alert = $('#alertMessage');
        //alert.removeClass('alert-success alert-danger alert-warning')
        //     .addClass('alert-' + type);
        //$('#alertText').text(message);
        //alert.fadeIn();

        //// Auto hide after 5 seconds
        //setTimeout(() => {
        //    alert.fadeOut();
        //}, 5000);
    }

    // Update row total
    function updateRowTotal(vIndex, rowIndex) {
        const input = $(`.price-input[data-vendor="${vIndex}"][data-row="${rowIndex}"]`);
        const price = parseFloat(input.val()) || 0;
        const qty = parseInt($('.fixed-column tbody tr').eq(rowIndex).find('td:nth-child(3)').text());
        const total = price * qty;

        $(`.vendor-column:eq(${vIndex}) tbody tr:eq(${rowIndex}) .total-cell`)
            .text('$' + total.toFixed(2))
            .attr('data-total', total);

        updateVendorTotals(vIndex);
    }

    // Update vendor totals
    function updateVendorTotals(vIndex) {
        let subTotal = 0;
        const rowCount = 0; // @Model.Items.Count;

        // Calculate subtotal
        for (let i = 0; i < rowCount; i++) {
            const total = parseFloat($(`.vendor-column:eq(${vIndex}) .total-cell[data-row="${i}"]`).attr('data-total')) || 0;
            subTotal += total;
        }

        // Get VAT status
        const vatToggle = $(`.vat-toggle[data-vendor="${vIndex}"]`);
        const vatIncluded = vatToggle.text().includes('Included');
        const vatAmount = vatIncluded ? subTotal * vatRate : 0;
        const grandTotal = vatIncluded ? subTotal + vatAmount : subTotal;

        // Update display
        const footer = $(`.vendor-column:eq(${vIndex}) .card-footer`);
        footer.find('.sub-total').text(subTotal.toFixed(2));
        footer.find('.vat-amount').text(vatAmount.toFixed(2));
        footer.find('.grand-total').text(grandTotal.toFixed(2));

        return { subTotal, vatAmount, grandTotal, vatIncluded };
    }

    // Price input change event
    $(document).on('input', '.price-input', function () {
        const vIndex = $(this).data('vendor');
        const rowIndex = $(this).data('row');
        updateRowTotal(vIndex, rowIndex);
    });

    // VAT toggle event
    $(document).on('click', '.vat-toggle', function () {
        const vIndex = $(this).data('vendor');
        const vendorCode = $(this).data('vendor-code');
        const button = $(this);

        // Toggle text
        if (button.text().includes('Included')) {
            button.html('<i class="bi bi-percent"></i> VAT Excluded');
        } else {
            button.html('<i class="bi bi-percent"></i> VAT Included');
        }

        updateVendorTotals(vIndex);
    });

    // Save button functionality
    $('#saveBtn').on('click', function () {
        const saveBtn = $(this);
        const originalHtml = saveBtn.html();

        // Disable button and show loading
        saveBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Saving...');

        // Prepare data for all vendors
        const vendorUpdates = [];
        const vendorCount = 0; // @Model.Vendors.Count;
        const itemCount = 0; // @Model.Items.Count;

        for (let vIndex = 0; vIndex < vendorCount; vIndex++) {
            const vendorCode = $(`.vat-toggle[data-vendor="${vIndex}"]`).data('vendor-code');
            const prices = [];

            // Collect all prices for this vendor
            for (let i = 0; i < itemCount; i++) {
                const price = parseFloat($(`.price-input[data-vendor="${vIndex}"][data-row="${i}"]`).val()) || 0;
                prices.push(price);
            }

            const vatToggle = $(`.vat-toggle[data-vendor="${vIndex}"]`);
            const vatIncluded = vatToggle.text().includes('Included');

            vendorUpdates.push({
                vendorCode: vendorCode,
                prices: prices,
                vatIncluded: vatIncluded
            });
        }

        // Send AJAX request
        $.ajax({
            url: '@Url.Action("SavePrices", "PriceEntry")',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ vendorPrices: vendorUpdates }),
            success: function (response) {
                if (response.success) {
                    showAlert(response.message, 'success');
                } else {
                    showAlert(response.message, 'danger');
                }
            },
            error: function (xhr, status, error) {
                showAlert('Error saving prices: ' + error, 'danger');
            },
            complete: function () {
                // Re-enable button
                saveBtn.prop('disabled', false).html(originalHtml);
            }
        });
    });

    // Initialize row totals
    function initializeRowTotals() {
        const vendorCount = 0; // @Model.Vendors.Count;
        const itemCount = 0; // @Model.Items.Count;

        for (let vIndex = 0; vIndex < vendorCount; vIndex++) {
            for (let i = 0; i < itemCount; i++) {
                updateRowTotal(vIndex, i);
            }
        }
    }

    // Close alert on button click
    $(document).on('click', '.btn-close', function () {
        $(this).closest('.alert').fadeOut();
    });

    // Initialize on page load
    initializeRowTotals();
});